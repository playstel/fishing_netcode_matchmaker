using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text shotResultsText; // Текст для отображения последних 12 выстрелов
    public Text playerScoreText; // Текст для отображения общего счёта текущего игрока
    public Text otherPlayersScoreText; // Текст для отображения общего счёта других игроков

    private List<string> shotResults = new List<string>(); // Хранение информации о последних 12 выстрелах
    private int playerScore = 0; // Общий счёт для текущего игрока
    private Dictionary<ulong, int> otherPlayersScores = new Dictionary<ulong, int>(); // Счёт для других игроков

    private void Start()
    {
        UpdateShotResults();
        UpdatePlayerScore();
        UpdateOtherPlayersScore();
    }

    // Вызывается при каждом выстреле. 0 за промах, очки и тип рыбы за попадание.
    public void RecordShotResult(bool hit, int score, string fishType)
    {
        if (hit)
        {
            shotResults.Add($"{score} pts - {fishType}");
            playerScore += score;
        }
        else
        {
            shotResults.Add("0"); // Промах
        }

        if (shotResults.Count > 12)
        {
            shotResults.RemoveAt(0); // Удалить старый результат, если их больше 12
        }

        UpdateShotResults();
        UpdatePlayerScore();
    }

    // Обновляет текст с результатами последних 12 выстрелов
    private void UpdateShotResults()
    {
        shotResultsText.text = "Last 12 Shots:\\n";
        foreach (string result in shotResults)
        {
            shotResultsText.text += result + "\\n";
        }
    }

    // Обновляет текст общего счёта текущего игрока
    private void UpdatePlayerScore()
    {
        playerScoreText.text = "Your Score: " + playerScore;
    }

    // Обновляет общий счёт для других игроков
    public void UpdateOtherPlayersScore(ulong playerId, int score)
    {
        if (!otherPlayersScores.ContainsKey(playerId))
        {
            otherPlayersScores[playerId] = score;
        }
        else
        {
            otherPlayersScores[playerId] = score;
        }

        UpdateOtherPlayersScore();
    }

    // Обновляет текст с общим счётом для других игроков
    private void UpdateOtherPlayersScore()
    {
        otherPlayersScoreText.text = "Other Players Scores:\\n";
        foreach (var kvp in otherPlayersScores)
        {
            otherPlayersScoreText.text += $"Player {kvp.Key}: {kvp.Value} pts\\n";
        }
    }
}
