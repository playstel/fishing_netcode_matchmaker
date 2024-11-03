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
        
        // Сетевые переменные для синхронизации
        private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>();
        private NetworkVariable<bool> isShooting = new NetworkVariable<bool>();

        
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
                // Если это не владелец, подписываемся на обновления сетевой позиции
                networkPosition.OnValueChanged += OnNetworkPositionChanged;
            }
        }
        
        private void OnNetworkPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        {
            // Обновляем локальную позицию, если это не владелец
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
                // Обновляем позицию для других клиентов
                _transform.position = networkPosition.Value;
            }
        }

        private void GunShoot()
        {
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     _isAutomaticMode = !_isAutomaticMode;
            //     UpdateFireModeUi();
            // }
            //
            // if (_isAutomaticMode)
            // {
            //     if (Input.GetMouseButton(0) && Time.time >= _nextShotTime)
            //     {
            //         Shoot();
            //         _nextShotTime = Time.time + (1f / fireRate); 
            //     }
            // }
            // else
            // {
            //     if (Input.GetMouseButtonDown(0) && Time.time >= _nextShotTime)
            //     {
            //         Shoot();
            //         _nextShotTime = Time.time + (1f / fireRate);
            //     }
            // }
            
            
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
            // Сообщаем всем клиентам, что был произведен выстрел
            ShootClientRpc();
        }
        
        [ClientRpc]
        private void ShootClientRpc()
        {
            // Выполняем локальную логику для всех клиентов
            Shoot();
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
        
        private GameObject GetObjectFromPool()
        {
            return ObjectPool.ObjectPool.Instance
                .SpawnFromPool(bulletTag, _transform.position + Vector3.up, bulletPrefab.transform.rotation);
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
                    var newPosition = new Vector2(worldPosition.x, PosY);
                    _transform.position = newPosition;
                    
                    // Обновляем сетевую позицию на сервере
                    UpdatePositionServerRpc(newPosition);
                }
            }
        }
        
        [ServerRpc]
        private void UpdatePositionServerRpc(Vector2 newPosition)
        {
            // Обновляем сетевую позицию, которая синхронизируется с клиентами
            networkPosition.Value = newPosition;
        }
    }
}