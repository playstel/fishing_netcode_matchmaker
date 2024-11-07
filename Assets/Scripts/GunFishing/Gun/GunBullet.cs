using System;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing.Gun
{
    public class Bullet : NetworkBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private GameObject fxPrefab;

        private Transform _transform;
        private Rigidbody2D _rb;
        private Gun _gun;
        
        // Определяем NetworkVariable для синхронизации позиции
        //public NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>();


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
        
        // private void Update()
        // {
        //     if (IsOwner)
        //     {
        //         UpdatePositionServerRpc(_transform.position);
        //     }
        //     else
        //     {
        //         _transform.position = networkPosition.Value;
        //     }
        // }
        
        private void Update()
        {
            // if (IsOwner)
            // {
            //     // Обновляем networkPosition только если положение изменилось значительно
            //     if ((networkPosition.Value - (Vector2)_transform.position).sqrMagnitude > 0.01f)
            //     {
            //         networkPosition.Value = _transform.position;
            //     }
            // }
            // else
            // {
            //     // Интерполяция для плавного обновления позиции
            //     _transform.position = Vector2.Lerp(_transform.position, networkPosition.Value, Time.deltaTime * 10f);
            // }
        }

        public override void OnNetworkSpawn()
        {
            _transform = transform;
            
            if (IsOwner)
            {
                Debug.Log("OnNetworkSpawn as owner");
            }
            else
            {
                //networkPosition.OnValueChanged += OnNetworkPositionChanged;
            }
        }
        
        //[ServerRpc]
        // private void UpdatePositionServerRpc(Vector2 newPosition)
        // {
        //     networkPosition.Value = newPosition;
        // }
        
        // private void OnNetworkPositionChanged(Vector2 oldPosition, Vector2 newPosition)
        // {
        //     if (!IsOwner)
        //     {
        //         _transform.position = newPosition;
        //     }
        // }
    }
}