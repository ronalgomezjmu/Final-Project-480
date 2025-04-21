using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq; // For FirstOrDefault
using TMPro; // Add this for scene management

public class PlayerDamageEffects : MonoBehaviour
{
    public Image bloodSplatterImage;
    private CanvasGroup canvasGroup;
    public AudioClip[] hurtSounds;
    public AudioClip deathSound; // Add a death sound
    private AudioSource audioSource;
    
    [Header("Health Settings")]
    public int maxHealth = 3; // Player can take 3 hits
    private int currentHealth;
    public TextMeshProUGUI healthText; // Optional UI element to show health
    
    [Header("Game Over")]
    public float delayBeforeGameOver = 2f; // Time before quitting/restarting
    public GameObject gameOverPanel; // Optional Game Over UI panel

    [Header("UI Elements")]
    public Slider healthSlider;

    // Add these to your existing class variables
    [Header("Health Slider Colors")]
    public Color healthColorFull = Color.green;       // 3/3 health - green
    public Color healthColorMedium = Color.white;     // 2/3 health - white
    public Color healthColorLow = Color.red;          // 1/3 or 0/3 health - red
    private Image sliderFillImage;                    // Reference to the fill image of the slider
    private Image sliderBackgroundImage;              // Reference to the background image of the slider
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get the Canvas Group
        if (bloodSplatterImage != null)
        {
            canvasGroup = bloodSplatterImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = bloodSplatterImage.gameObject.AddComponent<CanvasGroup>();
            }
            // Make sure image is invisible at start
            canvasGroup.alpha = 0;
            Debug.Log("Blood splatter system initialized");
        }
        else
        {
            Debug.LogError("Blood Splatter Image not assigned!");
        }
        
        // Initialize health
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            
            // Get the fill image from the slider
            sliderFillImage = healthSlider.fillRect.GetComponent<Image>();
            if (sliderFillImage == null)
            {
                Debug.LogError("Could not find fill image on health slider!");
            }
            
            // Get the background image (the part to the right of the fill)
            Transform backgroundRect = healthSlider.transform.Find("Background");
            if (backgroundRect != null)
            {
                sliderBackgroundImage = backgroundRect.GetComponent<Image>();
            }
            else
            {
                // Try alternative method to find background
                sliderBackgroundImage = healthSlider.GetComponentsInChildren<Image>()
                    .FirstOrDefault(img => img.gameObject != sliderFillImage.gameObject);
            }
            
            if (sliderBackgroundImage == null)
            {
                Debug.LogError("Could not find background image on health slider!");
            }
            
            // Set initial color
            UpdateSliderColor();
        }
        
        // Hide game over panel if assigned
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    void Update()
    {
        // Test with keyboard input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - showing blood splatter");
            TakeDamage();
        }
        
        // Test instant death (for debugging)
        if (Input.GetKeyDown(KeyCode.K))
        {
            currentHealth = 0;
            PlayerDied();
        }
    }
    
    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth + "/" + maxHealth;
        }

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
            UpdateSliderColor();
        }
    }
    
    // Public method that zombies can call to damage the player
    public void TakeDamage()
    {
        Debug.Log("Player taking damage!");
        currentHealth--;
        UpdateHealthUI();
        
        ShowBloodSplatter();
        PlayHurtSound();
        
        // Check if player died
        if (currentHealth <= 0)
        {
            PlayerDied();
        }
    }
    
    void PlayerDied()
    {
        Debug.Log("Player has died!");
        
        // Play death sound if available
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Show game over panel if available
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Make blood splatter stay on screen
        if (canvasGroup != null)
        {
            StopAllCoroutines();
            canvasGroup.alpha = 1;
        }
        
        // End the game after delay
        StartCoroutine(EndGameAfterDelay());
    }
    
    IEnumerator EndGameAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);
        
        // For a build game, you'd use:
        #if UNITY_EDITOR
            // If in editor, just stop playing
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // In a build, either quit or reload the scene
            // Option 1: Quit the application
            Application.Quit();
            
            // Option 2: Reload the current scene (comment out Option 1 if using this)
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        #endif
    }
    
    void ShowBloodSplatter()
    {
        Debug.Log("ShowBloodSplatter called. CanvasGroup is " + (canvasGroup != null ? "valid" : "NULL"));
        
        if (canvasGroup != null)
        {
            StopAllCoroutines();
            Debug.Log("Setting alpha to 1");
            canvasGroup.alpha = 1;
            StartCoroutine(FadeOutBloodSplatter());
        }
    }
    
    IEnumerator FadeOutBloodSplatter()
    {
        yield return new WaitForSeconds(2f);
        
        float fadeSpeed = 0.5f;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 0;
    }
    
    void PlayHurtSound()
    {
        if (audioSource != null && hurtSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, hurtSounds.Length);
            audioSource.PlayOneShot(hurtSounds[randomIndex]);
        }
    }

    void UpdateSliderColor()
    {
        if (sliderFillImage != null)
        {
            // Change color based on health remaining
            if (currentHealth == maxHealth)
            {
                sliderFillImage.color = healthColorFull;      // Green at full health
            }
            else if (currentHealth == 2)
            {
                sliderFillImage.color = healthColorMedium;    // White at medium health
            }
            else
            {
                sliderFillImage.color = healthColorLow;       // Red at low health
            }
            
            // Special case for zero health - make background red too
            if (sliderBackgroundImage != null)
            {
                if (currentHealth <= 0)
                {
                    sliderBackgroundImage.color = healthColorLow;  // Make background red at zero health
                }
                else
                {
                    // Reset to default color when not at zero health
                    sliderBackgroundImage.color = Color.white;     // Or whatever your default background color is
                }
            }
        }
    }
}