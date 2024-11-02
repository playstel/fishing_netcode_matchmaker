using System;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing
{
    public class Bullet : NetworkBehaviour
    {
        public float speed = 10f;
        private Vector3? direction;

        public void Initialize(Vector3 fireDirection)
        {
            direction = fireDirection;
        }

        private void OnBecameInvisible()
        {
            ObjectPool.ObjectPool.Instance.ReturnToPool(gameObject);
        }

        // private void Update()
        // {
        //     if (direction == null) return;
        //
        //     //transform.position += (Vector3)direction * speed * Time.deltaTime;
        //
        //     //if (transform.position.magnitude > 50f) Destroy(gameObject);
        // }

        private void OnTriggerEnter(Collider other)
        {
            if (TryGetComponent<Fish>(out Fish fish))
            {
                if (fish != null && !fish.isCaught.Value)
                {
                    fish.CatchFishServerRpc(OwnerClientId);
                }
            }
        }
    }
}