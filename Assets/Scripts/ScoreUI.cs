using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject pointsPopupPrefab;
    [SerializeField] private Transform pointsPopupParent;
    
    [Header("Animation Settings")]
    [SerializeField] private float scoreAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve scoreAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color positivePointsColor = Color.green;
    [SerializeField] private Color waveCompletionColor = Color.yellow;

    private int currentDisplayedScore = 0;
    private Coroutine scoreAnimationCoroutine;
    private ScoreManager scoreManager;
    
    private const string HIGH_SCORE_KEY = "ZombieGameHighScore";
    private int highScore = 0;

    void Start()
    {
        // Find the ScoreManager
        scoreManager = ScoreManager.Instance;
        if (scoreManager == null)
        {
            Debug.LogError("ScoreManager not found! Make sure it exists in the scene.");
        }
        
        // Load high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateHighScoreDisplay();
        
        // Register for score update events
        ScoreManager.OnScoreChanged += HandleScoreChanged;
        ScoreManager.OnPointsAdded += HandlePointsAdded;
        ScoreManager.OnWaveCompleted += HandleWaveCompleted;
        
        // Initialize score display
        if (scoreManager != null)
        {
            currentDisplayedScore = scoreManager.GetCurrentScore();
            UpdateScoreDisplay(currentDisplayedScore);
        }
    }
    
    void OnDestroy()
    {
        // Unregister from events to prevent memory leaks
        ScoreManager.OnScoreChanged -= HandleScoreChanged;
        ScoreManager.OnPointsAdded -= HandlePointsAdded;
        ScoreManager.OnWaveCompleted -= HandleWaveCompleted;
    }
    
    // Handle score change event
    private void HandleScoreChanged(int newScore)
    {
        // Animate score change
        if (scoreAnimationCoroutine != null)
        {
            StopCoroutine(scoreAnimationCoroutine);
        }
        
        scoreAnimationCoroutine = StartCoroutine(AnimateScoreChange(currentDisplayedScore, newScore));
        
        // Check for new high score
        if (newScore > highScore)
        {
            highScore = newScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            UpdateHighScoreDisplay();
            
            // Could add a visual effect for new high score here
        }
    }
    
    // Handle points added event (for popup)
    private void HandlePointsAdded(int points, Vector3 position, bool isWaveBonus)
    {
        if (pointsPopupPrefab != null && pointsPopupParent != null)
        {
            // Create popup at the position
            GameObject popup = Instantiate(pointsPopupPrefab, pointsPopupParent);
            
            // Set position (convert from world to canvas space if needed)
            RectTransform rectTransform = popup.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // If using a world space canvas, set world position
                if (pointsPopupParent.GetComponent<Canvas>().renderMode == RenderMode.WorldSpace)
                {
                    popup.transform.position = position + Vector3.up;
                }
                else
                {
                    // If using a screen space canvas, convert to screen position
                    Vector2 screenPos = Camera.main.WorldToScreenPoint(position + Vector3.up);
                    rectTransform.position = screenPos;
                }
            }
            
            // Set text and color
            TextMeshProUGUI popupText = popup.GetComponent<TextMeshProUGUI>();
            if (popupText != null)
            {
                popupText.text = "+" + points;
                popupText.color = isWaveBonus ? waveCompletionColor : positivePointsColor;
            }
            
            // Set up animation (handled by PointsPopupController component on the prefab)
            PointsPopupController popupController = popup.GetComponent<PointsPopupController>();
            if (popupController != null)
            {
                popupController.Initialize(points, isWaveBonus);
            }
            else
            {
                // If no controller is present, destroy after a delay
                Destroy(popup, 2f);
            }
        }
    }
    
    // Handle wave completion event
    private void HandleWaveCompleted(int waveNumber)
    {
        // Could add special visual effects for wave completion here
        // For example, a screen flash, wave complete banner, etc.
    }
    
    // Animate score change
    private IEnumerator AnimateScoreChange(int fromScore, int toScore)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < scoreAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / scoreAnimationDuration;
            float curveValue = scoreAnimationCurve.Evaluate(normalizedTime);
            
            // Calculate interpolated score
            int displayScore = Mathf.RoundToInt(Mathf.Lerp(fromScore, toScore, curveValue));
            UpdateScoreDisplay(displayScore);
            
            yield return null;
        }
        
        // Ensure we end at exactly the target score
        UpdateScoreDisplay(toScore);
        currentDisplayedScore = toScore;
        scoreAnimationCoroutine = null;
    }
    
    // Update the score text
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString("N0");
        }
    }
    
    // Update high score display
    private void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore.ToString("N0");
        }
    }
}

// Optional component for points popup animation
public class PointsPopupController : MonoBehaviour
{
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float moveDistance = 100f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float scaleBoost = 1.5f;
    [SerializeField] private float scaleDuration = 0.3f;
    
    private RectTransform rectTransform;
    private TextMeshProUGUI text;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    
    public void Initialize(int points, bool isWaveBonus)
    {
        rectTransform = GetComponent<RectTransform>();
        text = GetComponent<TextMeshProUGUI>();
        
        if (rectTransform != null)
        {
            startPosition = rectTransform.position;
            targetPosition = startPosition + new Vector3(0, moveDistance, 0);
            
            StartCoroutine(AnimatePopup(isWaveBonus));
        }
    }
    
    private IEnumerator AnimatePopup(bool isWaveBonus)
    {
        // Initial scale animation (pop effect)
        float scaleElapsed = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 boostedScale = originalScale * scaleBoost;
        
        if (isWaveBonus)
        {
            // Add a special effect for wave bonus (maybe pulse or bigger scale)
            boostedScale *= 1.5f;
        }
        
        while (scaleElapsed < scaleDuration)
        {
            scaleElapsed += Time.deltaTime;
            float normalizedTime = scaleElapsed / scaleDuration;
            
            // Scale up quickly, then back down to normal
            float scale = normalizedTime < 0.5f ? 
                Mathf.Lerp(1f, scaleBoost, normalizedTime * 2f) : 
                Mathf.Lerp(scaleBoost, 1f, (normalizedTime - 0.5f) * 2f);
                
            transform.localScale = originalScale * scale;
            yield return null;
        }
        
        transform.localScale = originalScale;
        
        // Movement and fade animation
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            
            // Apply animation curves
            float moveProgress = moveCurve.Evaluate(normalizedTime);
            float fadeProgress = fadeOutCurve.Evaluate(normalizedTime);
            
            // Move upward
            rectTransform.position = Vector3.Lerp(startPosition, targetPosition, moveProgress);
            
            // Fade out
            if (text != null)
            {
                Color color = text.color;
                color.a = fadeProgress;
                text.color = color;
            }
            
            yield return null;
        }
        
        // Destroy when animation is complete
        Destroy(gameObject);
    }
}