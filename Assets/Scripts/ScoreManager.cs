using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int score = 0;
    public int scoreToUnlock = 100;
    public TextMeshProUGUI scoreText; // Drag in inspector

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddPoints(int points)
    {
        score += points;
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (score >= scoreToUnlock)
        {
            UnlockNextArea();
        }
    }

    void UnlockNextArea()
    {
        GameObject[] barriers = GameObject.FindGameObjectsWithTag("Barrier");
        foreach (GameObject barrier in barriers)
        {
            Destroy(barrier);
        }

        Debug.Log("Next area unlocked!");
    }
}