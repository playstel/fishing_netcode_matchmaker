using System;
using System.Collections.Generic;
using UnityEngine;

namespace GunFishing.Score
{
    public class RoomPlayersManager : MonoBehaviour
    {
        public static RoomPlayersManager Instance;

        public Dictionary<ulong, Gun.Gun> playerList = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("Set RoomPlayersManager as singleton");
            }
            else
                Destroy(gameObject);
        }

        public void AddPlayer(ulong playerId, Gun.Gun playerPrefab)
        {
            if(playerList.ContainsKey(playerId)) return;
            playerList.Add(playerId, playerPrefab);
        }
        
        public void AddPlayerScore(ulong playerId, int score)
        {
            playerList.TryGetValue(playerId, out var gun);
            
            // if (gun)
            // {
            //     gun
            // }
        }
        
        public void RegisterHit(ulong playerId)
        {
            playerList.TryGetValue(playerId, out var gun);
            
            if (gun)
            {
                gun.RegisterHit();
            }
        }
    }
}