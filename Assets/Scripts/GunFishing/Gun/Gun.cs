
using GunFishing.Fish;
using UnityEngine;

namespace GunFishing.Gun
{
    public class Gun : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public string bulletTag = "Bullet";

        public float fireRate = 12f;         
        private float nextShotTime = 0f;     
        private bool isAutomaticMode = false; 
        
        private Camera _cameraMain;
        private Transform _transform;
        private float posY;
        
        [SerializeField] private int shotCount = 0;       
        [SerializeField] private int hitCount = 0;        
        private int correctionThreshold = 12;  

        private void Start()
        {
            _cameraMain = Camera.main;
            _transform = transform;
            posY = _transform.position.y;
        }

        private void Update()
        {
            GunShoot();
            GunMove();
        }

        private void GunShoot()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isAutomaticMode = !isAutomaticMode;
                Debug.Log("Fire mode: " + (isAutomaticMode ? "Auto" : "Single"));
            }

            if (isAutomaticMode)
            {
                if (Input.GetMouseButton(0) && Time.time >= nextShotTime)
                {
                    Shoot();
                    nextShotTime = Time.time + (1f / fireRate); 
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0) && Time.time >= nextShotTime)
                {
                    Shoot();
                    nextShotTime = Time.time + (1f / fireRate);
                }
            }
        }

        private void GunMove()
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector2 worldPosition = _cameraMain.ScreenToWorldPoint(mousePosition);
            _transform.position = new Vector2(worldPosition.x, posY);
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

            if (shotCount >= correctionThreshold)
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