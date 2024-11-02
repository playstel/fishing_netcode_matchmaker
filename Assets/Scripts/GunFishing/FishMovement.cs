using UnityEngine;

namespace GunFishing
{
    public class FishMovement : MonoBehaviour
    {
        public float speed = 2f;               
        public float directionChangeInterval = 2f;  
        private Vector2 _moveDirection;

        private void Start()
        {
            SetRandomDirection();
            
            InvokeRepeating(nameof(SetRandomDirection), directionChangeInterval, directionChangeInterval);
        }

        private void Update()
        {
            transform.Translate(_moveDirection * speed * Time.deltaTime);
        }

        private void SetRandomDirection()
        {
            _moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }

        private void OnBecameInvisible()
        {
            ObjectPool.ObjectPool.Instance.ReturnToPool(gameObject);
        }
    }

}