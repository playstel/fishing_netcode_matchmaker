using TMPro;

namespace GunFishing
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance;

        private int totalScore = 0;
        private List<ShotResult> recentShots = new List<ShotResult>();  // Хранит последние 12 выстрелов

        public TMP_Text recentShotsText;   // UI текст для отображения последних 12 выстрелов
        public TMP_Text totalScoreText;    // UI текст для отображения общего счета

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
                recentShots.RemoveAt(0); // Удаляем самый старый выстрел, если больше 12 записей
            }

            totalScore += score;
            
            UpdateUI();
        }

        private void UpdateUI()
        {
            recentShotsText.text = null;
            
            // Обновление UI с результатами последних 12 выстрелов
            foreach (ShotResult shot in recentShots)
            {
                Debug.Log("Add: " + shot.fishName);
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