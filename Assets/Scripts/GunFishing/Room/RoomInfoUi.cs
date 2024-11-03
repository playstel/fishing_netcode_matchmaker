using TMPro;

namespace GunFishing.Score
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class RoomInfoUi : MonoBehaviour
    {
        public static RoomInfoUi Instance;

        private int totalScore = 0;
        private List<RoomShotResult> recentShots = new List<RoomShotResult>();  

        public TMP_Text recentShotsText;   
        public TMP_Text totalScoreText;    
        public TMP_Text shootingModeText;    

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void RegisterShot(int score, string fishType)
        {
            recentShots.Add(new RoomShotResult(score, fishType));
            
            if (recentShots.Count > 12)
            {
                recentShots.RemoveAt(0); 
            }

            totalScore += score;
            
            UpdateUI();
        }

        public void ChangeShootingMode(string mode)
        {
            shootingModeText.text = mode;
        }

        private void UpdateUI()
        {
            recentShotsText.text = null;
            
            foreach (RoomShotResult shot in recentShots)
            {
                recentShotsText.text += $"+{shot.score} for {shot.fishName}\n";
            }

            totalScoreText.text = "Total score: " + totalScore;
        }

        public int GetTotalScore()
        {
            return totalScore;
        }
    }

}