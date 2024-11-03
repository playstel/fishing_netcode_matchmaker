using GunFishing.Fish;
using GunFishing.Score;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing.Gun
{
    public class Gun : NetworkBehaviour
    {
        public GameObject bulletPrefab;
        public string bulletTag = "Bullet";

        public float fireRate = 12f;
        
        [SerializeField] private int shotCount = 0;       
        [SerializeField] private int hitCount = 0;
        [SerializeField] private Camera cameraMain;
        
        private Transform _transform;
        
        private float _nextShotTime = 0f;     
        private bool _isAutomaticMode = false;

        private const float PosY = -3.7f;
        private const int CorrectionThreshold = 12;
        private const int MouseBounds = 7;
        
        public override void OnNetworkSpawn()
        {
            // Этот код выполняется, когда объект появляется в сети
            if (IsOwner)
            {
                // Выполняется только для клиента, которому принадлежит объект
                Debug.Log("OnNetworkSpawn as owner");
                cameraMain = Camera.main;
                _transform = transform;
            }
            Debug.Log("OnNetworkSpawn");
        }
        
        private void Update()
        {
            if (IsOwner)
            {
                GunShoot();
                GunMove();
            }
        }

        private void GunShoot()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isAutomaticMode = !_isAutomaticMode;
                UpdateFireModeUi();
            }

            if (_isAutomaticMode)
            {
                if (Input.GetMouseButton(0) && Time.time >= _nextShotTime)
                {
                    Shoot();
                    _nextShotTime = Time.time + (1f / fireRate); 
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && Time.time >= _nextShotTime)
                {
                    Shoot();
                    _nextShotTime = Time.time + (1f / fireRate);
                }
            }
        }

        private void UpdateFireModeUi()
        {
            if (RoomInfoUi.Instance)
            {
                RoomInfoUi.Instance.ChangeShootingMode("Fire mode: " + (_isAutomaticMode ? "Auto" : "Single") + " (press Space to change)");
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
                    _transform.position = new Vector2(worldPosition.x, PosY);
                }
            }
        }

        
        private void Shoot()
        {
            var bulletInstance = GetObjectFromPool();

            bulletInstance.TryGetComponent(out Bullet bullet);
            {
                bullet.SetHost(this);
                SuccessRateCheck();
            }
        }

        private GameObject GetObjectFromPool()
        {
            return ObjectPool.ObjectPool.Instance
                .SpawnFromPool(bulletTag, _transform.position + Vector3.up, bulletPrefab.transform.rotation);
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
    }
}