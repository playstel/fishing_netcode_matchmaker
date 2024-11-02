using GunFishing;
using GunFishing.Gun;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public GameObject bulletPrefab;

    private void Update()
    {
        if (IsOwner && Input.GetButtonDown("Fire1"))
        {
            Vector3 fireDirection = transform.forward;
            FireBulletServerRpc(fireDirection);
        }
    }

    [ServerRpc]
    private void FireBulletServerRpc(Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<NetworkObject>().Spawn();
        bullet.GetComponent<Bullet>().Initialize(direction);
    }
}