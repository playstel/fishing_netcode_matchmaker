using System;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing
{
    public class Fish : NetworkBehaviour
    {
        public NetworkVariable<bool> isCaught = new NetworkVariable<bool>(false);
        public string fxTag;
        
        private void Update()
        {
            if (isCaught.Value)
                return;
        
            MoveFish();
        }

        public void OnDisable()
        {
            ObjectPool.ObjectPool.Instance.SpawnFromPool(fxTag, transform.position, Quaternion.identity);
        }

        private void MoveFish()
        {
            // Логика случайного перемещения рыбы
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
            Debug.Log("OnTriggerEnter: " + other.tag);
            
            if (other.CompareTag("Bullet"))
            {
                ObjectPool.ObjectPool.Instance.ReturnToPool(other.gameObject);
                
                ObjectPool.ObjectPool.Instance.ReturnToPool(gameObject);
            }
        }
    }
}