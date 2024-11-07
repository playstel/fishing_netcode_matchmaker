using GunFishing.Gun;
using GunFishing.Score;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GunFishing.Fish
{
    public class Fish : NetworkBehaviour
    {
        public enum VolatilityLevel { Common, Rare, Legendary }
        public enum FishType { Rust, Purple, Gold }

        public FishType fishType;
        public VolatilityLevel volatility = VolatilityLevel.Common;
        public int points = 10;        
        public float baseSpeed = 2f;   
        public GameObject fxPrefab;
        private float speed;           
        
        private Vector2 movementDirection;
        
        //public NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>();
        public NetworkVariable<bool> isCaught = new NetworkVariable<bool>(false);

        public void SetVolatilityLevel(VolatilityLevel level)
        {
            volatility = level;
            
            switch (level)
            {
                case VolatilityLevel.Common:
                    speed = baseSpeed;
                    points = 10;
                    break;
                case VolatilityLevel.Rare:
                    speed = baseSpeed * 1.5f;
                    points = 20;
                    break;
                case VolatilityLevel.Legendary:
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
            if (IsServer && !isCaught.Value)
            {
                _timer += Time.deltaTime;
            
                if (_timer >= stepTime)
                {
                    _timer = 0;
                
                    if (Random.value < GetVolatilityChangeRate())
                    {
                        movementDirection = GetRandomDirection();
                    }
                }
            
                //transform.Translate(movementDirection * speed * Time.deltaTime);
                
                Vector2 newPosition = (Vector2)transform.position + movementDirection * speed * Time.deltaTime;
                //networkPosition.Value = newPosition;
                transform.position = newPosition;
            }
            else
            {
                //transform.position = networkPosition.Value;
            }
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
                VolatilityLevel.Common => 0.01f,   
                VolatilityLevel.Rare => 0.05f,
                VolatilityLevel.Legendary => 0.1f,   
                _ => 0.01f
            };
        }

        public override void OnDestroy()
        {
            Instantiate(fxPrefab, transform.position, Quaternion.identity, parent: null);
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void CatchFishServerRpc(ulong playerId)
        {
            if (isCaught.Value) return;
            
            isCaught.Value = true;
                
            Debug.Log($"Fish caught by player {playerId}");
                
            RoomInfoUi.Instance.RegisterShot(points, $"{volatility} {fishType} fish");
                
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                Debug.LogError("Failed to find trigger collider");
                return;
            }
            
            if (!other.CompareTag("Bullet")) return;

            // if (other.GetComponent<Bullet>())
            // {
            //     other.GetComponent<Bullet>().RegisterHit();
            // }
                
            CatchFishServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
}