using System;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing.Gun
{
    public class GunBullet : NetworkBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private GameObject fxPrefab;

        private Rigidbody2D _rb;
        private Gun _gun;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _rb.velocity = Vector2.up * speed;
        }

        private void OnBecameInvisible()
        {
            Instantiate(fxPrefab, transform.position, Quaternion.identity, parent: null);
        }
        
        public void SetHost(Gun shooting)
        {
            _gun = shooting;
        }

        public void RegisterHit()
        {
            _gun.RegisterHit();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Bounds"))
            {
                Vector2 newVelocity = _rb.velocity;

                if (Mathf.Abs(collision.contacts[0].normal.x) > 0.5f)
                {
                    newVelocity.x = -newVelocity.x; 
                }
                if (Mathf.Abs(collision.contacts[0].normal.y) > 0.5f)
                {
                    newVelocity.y = -newVelocity.y; 
                }

                _rb.velocity = newVelocity.normalized * speed; 
            }
        }
    }
}