using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GunFishing
{
    public class PlayerShooting : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public string bulletTag = "Bullet";
        public float shootForce = 100f;
        
        public float fireRate = 12f;         
        private float nextShotTime = 0f;     
        private bool isAutomaticMode = false; 
        
        private Camera _cameraMain;
        private Transform _transform;
        private float posY;

        private void Start()
        {
            _cameraMain = Camera.main;
            _transform = transform;
            posY = _transform.position.y;
        }

        void Update()
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
            var position = _transform.position;

            var bulletInstance = ObjectPool.ObjectPool.Instance
                .SpawnFromPool(bulletTag, position + Vector3.up, bulletPrefab.transform.rotation);
            
            var rb = bulletInstance.GetComponent<Rigidbody2D>();
            
            rb.AddForce(shootForce * Vector2.up, ForceMode2D.Impulse);
        }
    }
}