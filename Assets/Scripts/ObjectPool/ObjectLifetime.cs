using System;
using UnityEngine;

namespace GunFishing.Effects
{
    public class ObjectLifetime : MonoBehaviour
    {
        public float _lifeTime = 0.5f;
        
        private void OnEnable()
        {
            Invoke(nameof(ReturnToPool), _lifeTime);
        }

        private void ReturnToPool()
        {
            ObjectPool.ObjectPool.Instance.ReturnToPool(gameObject);
        }
    }
}