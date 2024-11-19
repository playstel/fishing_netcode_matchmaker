using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GunFishing.Fish;
using GunFishing.Score;
using Network;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GunFishing.Gun
{
    public class Gun : NetworkBehaviour
    {
        public GameObject bulletPrefab;
        public NetworkObject bulletObject;

        public float fireRate = 12f;
        
        [SerializeField] private int shotCount = 0;       
        [SerializeField] private int hitCount = 0;
        
        private Camera cameraMain;
        private Transform _transform;
        
        private float _nextShotTime = 0f;     
        private bool _isAutomaticMode = false;

        private const float PosY = -3.7f;
        private const int CorrectionThreshold = 12;
        private const int MouseBounds = 7;
        private const int StartPause = 3;
        private bool _readyToShoot;
        
        private NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>();
        
        public override async void OnNetworkSpawn()
        {
            await UniTask.WaitUntil(() => SceneManager.GetActiveScene().buildIndex > 0);
            
            _transform = transform;

            if (IsOwner)
            {
                cameraMain = Camera.main;
                UpdatePosition(Vector2.zero);
            }
            else
            {
                _networkPosition.OnValueChanged += OnNetworkPositionChanged;
            }
        }

        private async void Start()
        {
            await UniTask.WaitUntil(() => SceneManager.GetActiveScene().buildIndex > 0);
            
            if (IsOwner)
            {
                SetPlayerInfoServerRpc(NetworkManager.Singleton.LocalClientId);
                await UniTask.Delay(1000);
                _readyToShoot = true;
            }
        }

        private void OnNetworkPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            if (!IsOwner)
            {
                _transform.position = newPosition;
            }
        }
        
        private void Update()
        {
            if (!_readyToShoot) return;
            
            if (IsOwner)
            {
                GunShoot();
                GunMove();
            }
            else
            {
                _transform.position = Vector2.Lerp(_transform.position, _networkPosition.Value, Time.deltaTime * 10f);
            }
        }

        private void GunShoot()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isAutomaticMode = !_isAutomaticMode;
                UpdateFireModeUi();
            }

            var canShoot = _isAutomaticMode ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
            
            Debug.Log("canShoot: " + canShoot);

            if (!canShoot || !(Time.time >= _nextShotTime)) return;
            
            _nextShotTime = Time.time + (1f / fireRate);

            Debug.Log("ShootServerRpc: " + canShoot);
            
            ShootServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        
        private void UpdateFireModeUi()
        {
            if (RoomInfoUi.Instance)
            {
                RoomInfoUi.Instance.ChangeShootingMode("Fire mode: " + (_isAutomaticMode ? "Auto" : "Single") + " (press Space to change)");
            }
        }
        
        [ServerRpc]
        private void ShootServerRpc(ulong owner)
        {
            var bulletInstance = NetworkManager.Singleton.SpawnManager
                .InstantiateAndSpawn(bulletObject, ownerClientId: owner, position: _transform.position + Vector3.up, rotation: bulletPrefab.transform.rotation);
            
            bulletInstance.TryGetComponent(out GunBullet bullet);
            {
                bullet.SetHost(this);
                SuccessRateCheck();
            }
        }

        private void SuccessRateCheck()
        {
            shotCount++;

            if (shotCount >= CorrectionThreshold)
            {
                CheckSuccessRate();
                shotCount = 0;
                hitCount = 0;
            }
        }

        public void RegisterHit()
        {
            hitCount++;
        }

        private void CheckSuccessRate()
        {
            if (hitCount < 3)
            {
                Debug.Log("Making the game easier: spawn easy fish");
                FishSpawner.Instance.SpawnEasyFish();
            }
        }
        
        private void GunMove()
        {
            if (cameraMain == null)
            {
                cameraMain = Camera.main; 
            }
            
            Vector2 mousePosition = Input.mousePosition;

            if (cameraMain is not null)
            {
                Vector2 worldPosition = cameraMain.ScreenToWorldPoint(mousePosition);
            
                if (worldPosition.x is < MouseBounds and > -MouseBounds)
                {
                    UpdatePosition(worldPosition);
                }
            }
        }

        private void UpdatePosition(Vector2 worldPosition)
        {
            var newPosition = new Vector2(worldPosition.x, PosY);
            _transform.position = newPosition;
            UpdatePositionServerRpc(newPosition);
        }
        
        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        private void UpdatePositionServerRpc(Vector2 newPosition)
        {
            if ((_networkPosition.Value - newPosition).sqrMagnitude > 0.01f)
            {
                _networkPosition.Value = newPosition;
            }
        }
        
        [ServerRpc]
        private void SetPlayerInfoServerRpc(ulong playerId)
        {
            if (RoomPlayersManager.Instance == null)
            {
                Debug.LogError("Failed to find RoomPlayersManager.Instance");
                return;
            }
            
            RoomPlayersManager.Instance.AddPlayer(playerId, this);
        }
    }
}