using TMPro;
using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int score = 0;
    public TextMeshProUGUI scoreText; // Drag in inspector

    // Add event for other systems to listen to score changes
    public event Action<int> OnScoreChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddPoints(int points)
    {
        score += points;

        // Update UI
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        // Trigger the event so other systems can respond
        OnScoreChanged?.Invoke(score);
    }

    // Get current score
    public int GetScore()
    {
        return score;
    }
}