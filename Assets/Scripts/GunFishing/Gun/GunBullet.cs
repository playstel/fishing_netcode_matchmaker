using System;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing.Gun
{
    public class Bullet : NetworkBehaviour
    {
        public float speed = 10f;
        //public string fxTag;
        public GameObject fxPrefab;
        private float maxLifeTime = 5f;
        private Rigidbody2D rb;
        private Gun _gun;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            rb.velocity = Vector2.up * speed;
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
    }
}