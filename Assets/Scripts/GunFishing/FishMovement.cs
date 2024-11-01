using UnityEngine;

namespace GunFishing
{
    public class FishMovement : MonoBehaviour
    {
        public float speed = 2f;

        void Update()
        {
            transform.Translate(Vector2.down * speed * Time.deltaTime);
        }
    }

}