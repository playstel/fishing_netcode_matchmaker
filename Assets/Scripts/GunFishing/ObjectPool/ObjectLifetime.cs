namespace GunFishing.ObjectPool
{
    using UnityEngine;
    
    public class ObjectLifetime : MonoBehaviour
    {
        public float _lifeTime = 0.5f;
        
        private void OnEnable()
        {
            Invoke(nameof(ReturnToPool), _lifeTime);
        }

        private void ReturnToPool()
        {
            ObjectPool.Instance.ReturnToPool(gameObject);
        }
    }
}