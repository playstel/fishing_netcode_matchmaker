using UnityEngine;

namespace GunFishing
{
    public class FishSpawner : MonoBehaviour
    {
        public string[] fishTags;       

        public float spawnInterval = 2f;
        public float spawnRangeX = 8f;
        public float spawnRangeY = 4f;

        private void Start()
        {
            InvokeRepeating(nameof(SpawnFish), spawnInterval, spawnInterval);
        }

        private void SpawnFish()
        {
            var fishName = fishTags[Random.Range(0, fishTags.Length)];

            Vector2 spawnPosition = new Vector2(Random.Range(-spawnRangeX, spawnRangeX), Random.Range(-spawnRangeY, spawnRangeY));

            ObjectPool.ObjectPool.Instance.SpawnFromPool(fishName, spawnPosition, Quaternion.identity);
        }
    }

}