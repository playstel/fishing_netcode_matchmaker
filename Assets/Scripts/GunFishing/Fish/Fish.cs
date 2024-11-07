using System;
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

        public float baseSpeed = 1;
        public NetworkVariable<float> speed = new();         
        public FishType fishType = new FishType();
        
        public NetworkVariable<VolatilityLevel> volatility = new NetworkVariable<VolatilityLevel>();
        public NetworkVariable<int> points = new NetworkVariable<int>();        
        
        public GameObject fxPrefab;
        
        private Vector2 movementDirection;
        private float _timer;
        private float stepTime = 0.01f;
        
        public NetworkVariable<bool> isCaught = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);
        
        private void Start()
        {
            SetVolatilityLevelServerRpc();
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SetVolatilityLevelServerRpc()
        {
            if (isReady.Value) return;
            
            isReady.Value = true;

            SetVolatilityLevelClientRpc((int)GetRandomVolatilityLevel());
        }

        private VolatilityLevel GetRandomVolatilityLevel()
        {
            return Random.value switch
            {
                < 0.6f => Fish.VolatilityLevel.Common,    
                < 0.9f => Fish.VolatilityLevel.Rare,  
                _ => Fish.VolatilityLevel.Legendary
            };
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void SetVolatilityLevelClientRpc(int level)
        {
            volatility = new NetworkVariable<VolatilityLevel>((VolatilityLevel)level);       
            
            switch (level)
            {
                case 0:
                    speed = new NetworkVariable<float>(baseSpeed);
                    points = new NetworkVariable<int>(10);
                    break;
                case 1:
                    speed = new NetworkVariable<float>(baseSpeed * 1.5f);
                    points = new NetworkVariable<int>(20);
                    break;
                case 2:
                    speed = new NetworkVariable<float>(baseSpeed * 2.5f);
                    points = new NetworkVariable<int>(40);
                    break;
            }

            movementDirection = GetRandomDirection();
        }

        
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

                Vector2 newPosition = (Vector2)transform.position + movementDirection * speed.Value * Time.deltaTime;
                transform.position = newPosition;
            }
        }
        
        private Vector2 GetRandomDirection()
        {
            float angle = Random.Range(0f, 2f * Mathf.PI);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        private float GetVolatilityChangeRate()
        {
            return volatility.Value switch
            {
                VolatilityLevel.Common => 0.01f,   
                VolatilityLevel.Rare => 0.05f,
                VolatilityLevel.Legendary => 0.1f,   
                _ => 0.01f
            };
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null)
            {
                Debug.LogError("Failed to find trigger collider");
                return;
            }
            
            if (!other.CompareTag("Bullet")) return;

            if (other.TryGetComponent(out GunFishing.Gun.GunBullet bullet))
            {
                CatchFishServerRpc(bullet.OwnerClientId);
            }
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void CatchFishServerRpc(ulong playerId)
        {
            if (isCaught.Value) return;
            
            isCaught.Value = true;
            
            UpdateClientRpc(playerId);
            
            RoomInfoUi.Instance.AddTotalScore(points.Value);

            NetworkObject.Destroy(gameObject);
        }

        [ClientRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void UpdateClientRpc(ulong playerId)
        {
            Debug.Log($"Fish caught by player {playerId}");

            RoomInfoUi.Instance.RegisterShot(points.Value, $"{volatility.Value} {fishType} fish", playerId);
            
            if (playerId == NetworkManager.Singleton.LocalClientId)
            {
                RoomPlayersManager.Instance.RegisterHit(playerId);
            }
        }

        public override void OnDestroy()
        {
            Instantiate(fxPrefab, transform.position, Quaternion.identity, parent: null);
        }
    }
}