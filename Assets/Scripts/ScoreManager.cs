using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int pointsPerZombieKill = 10;
    [SerializeField] private int pointsPerWaveCompleted = 50;
    [SerializeField] private int waveCompletionBonusMultiplier = 2; // Bonus multiplier per wave
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI pointsPopupText;
    [SerializeField] private float pointsPopupDuration = 2f;
    [SerializeField] private Color pointsPopupColor = Color.yellow;
    
    private int currentScore = 0;
    private Coroutine pointsPopupCoroutine;
    
    // Define events for score changes
    public static event Action<int> OnScoreChanged;
    public static event Action<int, Vector3, bool> OnPointsAdded; // points, position, isWaveBonus
    public static event Action<int> OnWaveCompleted; // wave number
    
    // Singleton instance
    private static ScoreManager _instance;
    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScoreManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ScoreManager");
                    _instance = obj.AddComponent<ScoreManager>();
                }
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        // Ensure there's only one instance of ScoreManager
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    
    void Start()
    {
        UpdateScoreDisplay();
    }
    
    // Method to add points for killing a zombie
    public void AddZombieKillPoints(int waveNumber, Vector3 zombiePosition)
    {
        // Calculate points with wave-based multiplier
        int points = pointsPerZombieKill * waveNumber;
        
        // Add points to the total score
        AddPoints(points, zombiePosition, false);
        
        // Log for debugging
        Debug.Log($"Added {points} points for killing a zombie in wave {waveNumber}. Total score: {currentScore}");
    }
    
    // Method to add completion bonus when a wave is finished
    public void AddWaveCompletionBonus(int waveNumber)
    {
        // Calculate wave completion bonus
        int bonus = pointsPerWaveCompleted + (waveNumber * waveCompletionBonusMultiplier * pointsPerWaveCompleted);
        
        // Add bonus to total score
        AddPoints(bonus, Vector3.zero, true);
        
        // Trigger wave completion event
        OnWaveCompleted?.Invoke(waveNumber);
        
        // Log for debugging
        Debug.Log($"Added {bonus} points for completing wave {waveNumber}. Total score: {currentScore}");
    }
    
    // Generic method to add points
    private void AddPoints(int points, Vector3 position, bool isWaveBonus)
    {
        currentScore += points;
        UpdateScoreDisplay();
        
        // Trigger the points added event
        OnPointsAdded?.Invoke(points, position, isWaveBonus);
        
        // Trigger the score changed event
        OnScoreChanged?.Invoke(currentScore);
        
        // Show points popup if UI element exists and position is valid
        if (pointsPopupText != null && position != Vector3.zero)
        {
            ShowPointsPopup(points, position);
        }
    }
    
    // Update the score display UI
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }
    
    // Method to show a temporary points popup at the zombie's position
    private void ShowPointsPopup(int points, Vector3 position)
    {
        // Only implement if you have a world space canvas for the popup
        // This is optional and can be expanded based on your UI setup
        if (pointsPopupText != null)
        {
            // If there's already a popup, stop it
            if (pointsPopupCoroutine != null)
            {
                StopCoroutine(pointsPopupCoroutine);
            }
            
            // Start a new popup coroutine
            pointsPopupCoroutine = StartCoroutine(AnimatePointsPopup(points, position));
        }
    }
    
    // Coroutine to animate the points popup
    private IEnumerator AnimatePointsPopup(int points, Vector3 position)
    {
        // This is a simplified version - you'd need a proper world space canvas setup
        // for this to work well in-game
        
        // Set the popup text and position
        pointsPopupText.text = $"+{points}";
        pointsPopupText.color = pointsPopupColor;
        pointsPopupText.gameObject.SetActive(true);
        
        // Position the text in world space (requires a world space canvas)
        // You'll need to adjust this based on your actual UI setup
        // pointsPopupText.transform.position = Camera.main.WorldToScreenPoint(position + Vector3.up);
        
        // Simple animation - fade out
        float startTime = Time.time;
        float elapsedTime = 0f;
        
        while (elapsedTime < pointsPopupDuration)
        {
            elapsedTime = Time.time - startTime;
            float normalizedTime = elapsedTime / pointsPopupDuration;
            
            // Move upward slightly
            // pointsPopupText.transform.position += Vector3.up * Time.deltaTime;
            
            // Fade out
            Color newColor = pointsPopupText.color;
            newColor.a = Mathf.Lerp(1f, 0f, normalizedTime);
            pointsPopupText.color = newColor;
            
            yield return null;
        }
        
        // Hide the popup when done
        pointsPopupText.gameObject.SetActive(false);
        pointsPopupCoroutine = null;
    }
    
    // Method to get the current score (for other systems that might need it)
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    // Reset score (e.g. for new game)
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
        OnScoreChanged?.Invoke(currentScore);
    }
}