using UnityEngine;

namespace GunFishing
{
    public class FishSpawner : MonoBehaviour
    {
        public GameObject fishPrefab;
        public float spawnInterval = 2f;
        public float spawnRangeX = 8f;
        public float spawnRangeY = 4f;

        private void Start()
        {
            InvokeRepeating("SpawnFish", spawnInterval, spawnInterval);
        }

        private void SpawnFish()
        {
            Vector2 spawnPosition = new Vector2(Random.Range(-spawnRangeX, spawnRangeX), Random.Range(-spawnRangeY, spawnRangeY));
            Instantiate(fishPrefab, spawnPosition, fishPrefab.transform.rotation);
        }
    }

}