using UnityEngine;

namespace GunFishing
{
    public class FishSpawner : MonoBehaviour
    {
        [Header("Fish setup")]
        public string[] fishTags;   
        public string easyFishTag;        
        
        [Header("Spawn setup")]
        public float spawnInterval = 2f;
        public float spawnRangeX = 8f;
        public float spawnRangeY = 4f;

        public static FishSpawner Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            InvokeRepeating(nameof(SpawnFish), spawnInterval, spawnInterval);
        }

        private void SpawnFish()
        {
            SpawnFish(GetRandomFishTag(), SpawnPosition());
        }

        public void SpawnEasyFish()
        {
            SpawnFish(easyFishTag, SpawnPosition());
        }
        
        private string GetRandomFishTag()
        {
            return fishTags[Random.Range(0, fishTags.Length)];
        }
        
        private void SpawnFish(string fishTag, Vector2 spawnPosition)
        {
            var fish = ObjectPool.ObjectPool.Instance.SpawnFromPool(fishTag, spawnPosition, Quaternion.identity);
            fish.GetComponent<Fish>().SetVolatilityLevel(GetRandomVolatilityLevel());
        }
        
        private Vector2 SpawnPosition()
        {
            return new Vector2(Random.Range(-spawnRangeX, spawnRangeX), Random.Range(-spawnRangeY, spawnRangeY));
        }

        private Fish.VolatilityLevel GetRandomVolatilityLevel()
        {
            return Random.value switch
            {
                < 0.6f => Fish.VolatilityLevel.Common,    
                < 0.9f => Fish.VolatilityLevel.Rare,  
                _ => Fish.VolatilityLevel.Legendary
            };
        }
    }

}