using System;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float speed = 10f;
    private Vector3? direction;

    public void Initialize(Vector3 fireDirection)
    {
        direction = fireDirection;
    }

    private void Start()
    {
        Invoke(nameof(DestroyBullet), 1f);
    }

    private void DestroyBullet()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        if(direction == null) return;
        
        //transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (transform.position.magnitude > 50f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter: " + other.tag);
        if (other.CompareTag("Fish"))
        {
            var fish = other.GetComponent<Fish>();
            if (fish != null && !fish.isCaught.Value)
            {
                fish.CatchFishServerRpc(OwnerClientId);
                Destroy(gameObject);
            }
        }
    }
    
    
}