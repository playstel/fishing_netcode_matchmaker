using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunFishing
{
    public class Fish : NetworkBehaviour
    {
        public enum VolatilityLevel { Low, Medium, High }
        
        public VolatilityLevel volatility = VolatilityLevel.Low;
        public int points = 10;        
        public float baseSpeed = 2f;   
        private float speed;           
        
        private Vector2 movementDirection;
        
        public NetworkVariable<bool> isCaught = new NetworkVariable<bool>(false);
        public string fxTag;

        public void SetVolatilityLevel(VolatilityLevel level)
        {
            volatility = level;
            
            switch (level)
            {
                case VolatilityLevel.Low:
                    speed = baseSpeed;
                    points = 10;
                    break;
                case VolatilityLevel.Medium:
                    speed = baseSpeed * 1.5f;
                    points = 20;
                    break;
                case VolatilityLevel.High:
                    speed = baseSpeed * 2.5f;
                    points = 40;
                    break;
            }

            movementDirection = GetRandomDirection();
        }

        private float _timer;
        private float stepTime = 0.01f;
        
        private void Update()
        {
            _timer += Time.deltaTime;
            
            if (_timer >= stepTime)
            {
                _timer = 0;
                
                if (Random.value < GetVolatilityChangeRate())
                {
                    movementDirection = GetRandomDirection();
                    Debug.Log("movementDirection");
                }
            }
            
            transform.Translate(movementDirection * speed * Time.deltaTime);
        }
        
        private Vector2 GetRandomDirection()
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private float GetVolatilityChangeRate()
        {
            return volatility switch
            {
                VolatilityLevel.Low => 0.01f,   
                VolatilityLevel.Medium => 0.05f,
                VolatilityLevel.High => 0.1f,   
                _ => 0.01f
            };
        }

        public void OnDisable()
        {
            ObjectPool.ObjectPool.Instance.SpawnFromPool(fxTag, transform.position, Quaternion.identity);
        }

        [ServerRpc]
        public void CatchFishServerRpc(ulong playerId)
        {
            if (!isCaught.Value)
            {
                isCaught.Value = true;
                Debug.Log($"Fish caught by player {playerId}");
                
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Bullet"))
            {
                other.GetComponent<Bullet>().RegisterHit();
                
                ObjectPool.ObjectPool.Instance.ReturnToPool(other.gameObject);
                
                ObjectPool.ObjectPool.Instance.ReturnToPool(gameObject);
                
                GameManager.Instance.AddScore(points);
            }
        }
    }
}