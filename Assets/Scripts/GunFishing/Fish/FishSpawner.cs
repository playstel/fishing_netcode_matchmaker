using Network;
using Unity.Netcode;
using UnityEngine;

namespace GunFishing.Fish
{
    public class FishSpawner : NetworkBehaviour
    {
        [Header("Fish prefabs")]
        public NetworkObject[] fishPrefabs;   
        public NetworkObject easyFishPrefab; 
        
        [Header("Spawn setup")]
        public float spawnInterval = 2f;
        public float spawnRangeX = 8f;
        public float spawnRangeY = 4f;

        public static FishSpawner Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        
        private void Start()
        {
            if (IsServer)
            {
                InvokeRepeating(nameof(SpawnFishServerRpc), spawnInterval, spawnInterval);
            }
        }

        [ServerRpc]
        private void SpawnFishServerRpc()
        {
            SpawnFish(GetRandomFishPrefab(), SpawnPosition());
        }

        public void SpawnEasyFish()
        {
            SpawnEasyFishServerRpc();
        }
        
        [ServerRpc]
        private void SpawnEasyFishServerRpc()
        {
            SpawnFish(easyFishPrefab, SpawnPosition());
        }
        
        private NetworkObject GetRandomFishPrefab()
        {
            return fishPrefabs[Random.Range(0, fishPrefabs.Length)];
        }
        
        private void SpawnFish(NetworkObject fishPrefab, Vector2 spawnPosition)
        {
            NetworkManager.Singleton.SpawnManager
                .InstantiateAndSpawn(fishPrefab, position:spawnPosition, rotation: Quaternion.identity);
        }
        
        private Vector2 SpawnPosition()
        {
            return new Vector2(Random.Range(-spawnRangeX, spawnRangeX), Random.Range(-spawnRangeY, spawnRangeY));
        }
    }

}