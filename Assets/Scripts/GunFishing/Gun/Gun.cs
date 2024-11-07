using GunFishing.Fish;
using GunFishing.Score;
using Network;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing.Gun
{
    public class Gun : NetworkBehaviour
    {
        public GameObject bulletPrefab;
        public NetworkObject bulletObject;

        public float fireRate = 12f;
        
        private int shotCount = 0;       
        private int hitCount = 0;
        private Camera cameraMain;
        
        // private float _lastPositionUpdateTime = 0f;
        // private const float PositionUpdateInterval = 0.005f;

        
        private Transform _transform;
        
        private float _nextShotTime = 0f;     
        private bool _isAutomaticMode = false;

        private const float PosY = -3.7f;
        private const int CorrectionThreshold = 12;
        private const int MouseBounds = 7;
        
        private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>();

        
        public override void OnNetworkSpawn()
        {
            _transform = transform;
            
            if (IsOwner)
            {
                Debug.Log("OnNetworkSpawn as owner");
                cameraMain = Camera.main;
            }
            else
            {
                networkPosition.OnValueChanged += OnNetworkPositionChanged;
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
            if (IsOwner)
            {
                GunShoot();
                GunMove();
            }
            else
            {
                _transform.position = Vector2.Lerp(_transform.position, networkPosition.Value, Time.deltaTime * 10f);
            }
        }

        private void GunShoot()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isAutomaticMode = !_isAutomaticMode;
                UpdateFireModeUi();
            }

            bool canShoot = _isAutomaticMode ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
            
            if (canShoot && Time.time >= _nextShotTime)
            {
                _nextShotTime = Time.time + (1f / fireRate);

                // Вызываем стрельбу на сервере, чтобы все клиенты увидели её
                ShootServerRpc();
            }
        }
        
        private void UpdateFireModeUi()
        {
            if (RoomInfoUi.Instance)
            {
                RoomInfoUi.Instance.ChangeShootingMode("Fire mode: " + (_isAutomaticMode ? "Auto" : "Single") + " (press Space to change)");
            }
        }
        
        [ServerRpc]
        private void ShootServerRpc()
        {
            //ShootClientRpc();
            Shoot();
        }
        
        // [ClientRpc]
        // private void ShootClientRpc()
        // {
        //     Shoot();
        // }
        
        private void Shoot()
        {
            //var bulletInstance = GetObjectFromPool();
            //var bulletInstance = Instantiate(bulletPrefab, _transform.position + Vector3.up, bulletPrefab.transform.rotation);

            var bulletInstance = NetworkRelay.Instance.NetworkManager.SpawnManager
                .InstantiateAndSpawn(bulletObject, position: _transform.position + Vector3.up, rotation: bulletPrefab.transform.rotation);
            
            bulletInstance.TryGetComponent(out Bullet bullet);
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
        
        // private GameObject GetObjectFromPool()
        // {
        //     //return ObjectPool.ObjectPool.Instance.SpawnFromPool(bulletTag, _transform.position + Vector3.up, bulletPrefab.transform.rotation);
        // }

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
                    var newPosition = new Vector2(worldPosition.x, PosY);
                    _transform.position = newPosition;
                    UpdatePositionServerRpc(newPosition);
                    
                    // if (Time.deltaTime - _lastPositionUpdateTime >= PositionUpdateInterval)
                    // {
                    //     _lastPositionUpdateTime = Time.deltaTime;
                    //     UpdatePositionServerRpc(newPosition);
                    // }
                }
            }
        }
        
        [ServerRpc]
        private void UpdatePositionServerRpc(Vector2 newPosition)
        {
            if ((networkPosition.Value - newPosition).sqrMagnitude > 0.01f)
            {
                networkPosition.Value = newPosition;
            }
        }
    }
}