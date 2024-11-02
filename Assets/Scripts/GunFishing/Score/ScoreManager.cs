using TMPro;

namespace GunFishing.Score
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;

        private int totalScore = 0;
        private List<ShotResult> recentShots = new List<ShotResult>();  

        public TMP_Text recentShotsText;   
        public TMP_Text totalScoreText;    

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void RegisterShot(int score, string fishType)
        {
            Debug.Log("RegisterShot: " + score + " | fishType: " + fishType);
            
            recentShots.Add(new ShotResult(score, fishType));
            
            if (recentShots.Count > 12)
            {
                recentShots.RemoveAt(0); 
            }

            totalScore += score;
            
            UpdateUI();
        }

        private void UpdateUI()
        {
            recentShotsText.text = null;
            
            foreach (ShotResult shot in recentShots)
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