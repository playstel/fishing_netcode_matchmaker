using System;
using TMPro;
using Unity.Netcode;

namespace GunFishing.Score
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class RoomInfoUi : NetworkBehaviour
    {
        public static RoomInfoUi Instance;

        private int TotalScore;
        private NetworkVariable<int> _totalScore = new();
        private List<RoomShotResult> _recentShots = new List<RoomShotResult>();  

        public TMP_Text recentShotsText;   
        public TMP_Text totalScoreText;    
        public TMP_Text shootingModeText;    

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            UpdateTotalScoreUi();
        }

        public void AddTotalScore(int score)
        {
            TotalScore += score;
        }
        
        public void RegisterShot(int score, string fishType, ulong playerId)
        {
            if (playerId == NetworkManager.Singleton.LocalClientId)
            {
                _recentShots.Add(new RoomShotResult(score, fishType));
            
                if (_recentShots.Count > 12)
                {
                    _recentShots.RemoveAt(0); 
                }
            }
            
            UpdateShotLogUi();
            UpdateTotalScoreUi();
        }

        private float timer;
        private float updateTime = 0.2f;
        private void Update()
        {
            timer += Time.deltaTime;
            
            if (timer > updateTime)
            {
                updateTime = 0;
                UpdateTotalScoreServerRpc();
                UpdateTotalScoreUi();
            }
        }

        [ServerRpc(RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
        private void UpdateTotalScoreServerRpc()
        {
            _totalScore.Value = TotalScore;
        }
        public void ChangeShootingMode(string mode)
        {
            shootingModeText.text = mode;
        }

        private void UpdateShotLogUi()
        {
            Debug.Log("UpdateShotLog");
            
            recentShotsText.text = null;
            
            foreach (RoomShotResult shot in _recentShots)
            {
                recentShotsText.text += $"+{shot.score} for {shot.fishName}\n";
            }
        }

        private void UpdateTotalScoreUi()
        {
            totalScoreText.text = "Total score: " + _totalScore.Value;
        }
    }
}