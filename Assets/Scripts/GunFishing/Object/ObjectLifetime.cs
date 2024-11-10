using UnityEngine;

namespace GunFishing.Object
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
            Destroy(gameObject);
        }
    }
}