using System;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing
{
    public class Bullet : NetworkBehaviour
    {
        public float speed = 10f;
        public string fxTag;
        private float maxLifeTime = 5f;
        private Vector3? direction;
        private Rigidbody2D rb;
        private GameObject _gameObject;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            _gameObject = gameObject;
        }

        private void OnEnable()
        {
            rb.velocity = Vector2.up * speed;
        }

        public void Initialize(Vector3 fireDirection)
        {
            direction = fireDirection;
        }

        private void OnBecameInvisible()
        {
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            ObjectPool.ObjectPool.Instance.SpawnFromPool(fxTag, transform.position, Quaternion.identity);
            ObjectPool.ObjectPool.Instance.ReturnToPool(gameObject);
        }

        private PlayerShooting _playerShooting;
        public void SetHost(PlayerShooting shooting)
        {
            _playerShooting = shooting;
        }

        public void RegisterHit()
        {
            _playerShooting.RegisterHit();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Bounds"))
            {
                Vector2 newVelocity = rb.velocity;

                if (Mathf.Abs(collision.contacts[0].normal.x) > 0.5f)
                {
                    newVelocity.x = -newVelocity.x; 
                }
                if (Mathf.Abs(collision.contacts[0].normal.y) > 0.5f)
                {
                    newVelocity.y = -newVelocity.y; 
                }

                rb.velocity = newVelocity.normalized * speed; 
            }
        }

        private float _lifeTime;
        private void Update()
        {
            if (_gameObject.activeSelf)
            {
                _lifeTime += Time.deltaTime;

                if (_lifeTime > maxLifeTime)
                {
                    _lifeTime = 0;
                    ReturnToPool();
                }
            }
            else
            {
                _lifeTime = 0;
            }
        }
    }
}