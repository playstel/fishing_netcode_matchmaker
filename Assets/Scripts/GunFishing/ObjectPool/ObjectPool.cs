using Unity.Netcode;

namespace GunFishing.ObjectPool
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectPool : NetworkBehaviour
    {
        // public static ObjectPool Instance;
        //
        // [System.Serializable]
        // public class Pool
        // {
        //     public string tag;
        //     public GameObject prefab;
        //     public int size;
        // }
        //
        // public List<Pool> pools;
        // public Dictionary<string, Queue<GameObject>> poolDictionary = new();
        //
        // private void Awake()
        // {
        //     if (Instance == null)
        //     {
        //         Instance = this;
        //         InitializePool();
        //     }
        //     else
        //     {
        //         Destroy(gameObject);
        //     }
        // }
        //
        // private void InitializePool()
        // {
        //     poolDictionary = new Dictionary<string, Queue<GameObject>>();
        //
        //     foreach (Pool pool in pools)
        //     {
        //         Queue<GameObject> objectPool = new Queue<GameObject>();
        //
        //         for (int i = 0; i < pool.size; i++)
        //         {
        //             GameObject obj = Instantiate(pool.prefab, parent: gameObject.transform);
        //             //obj.SetActive(false);
        //             objectPool.Enqueue(obj);
        //         }
        //
        //         poolDictionary.Add(pool.tag, objectPool);
        //     }
        // }
        //
        // public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        // {
        //     if (!IsServer)
        //     {
        //         Debug.LogWarning("SpawnFromPool can only be called on the server.");
        //         return null;
        //     }
        //
        //     if (!poolDictionary.ContainsKey(tag))
        //     {
        //         Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
        //         return null;
        //     }
        //
        //     GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        //
        //     objectToSpawn.SetActive(true);
        //     objectToSpawn.transform.position = position;
        //     objectToSpawn.transform.rotation = rotation;
        //
        //     poolDictionary[tag].Enqueue(objectToSpawn);
        //
        //     return objectToSpawn;
        // }
        //
        // public void ReturnToPool(GameObject obj)
        // {
        //     if (!obj.TryGetComponent(out NetworkObject networkObject))
        //     {
        //         Debug.LogWarning("Returned object doesn't have a NetworkObject component.");
        //         return;
        //     }
        //
        //     if (networkObject.IsSpawned)
        //     {
        //         networkObject.Despawn();
        //     }
        //     
        //     obj.SetActive(false);
        //     poolDictionary[obj.tag].Enqueue(obj);
        // }
    }

}