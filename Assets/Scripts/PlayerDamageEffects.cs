using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;

public class PlayerDamageEffects : MonoBehaviour
{
    [Header("Blood Splatter Effect")]
    public Image bloodSplatterImage;
    private CanvasGroup canvasGroup;

    [Header("Audio")]
    public AudioClip[] hurtSounds;
    public AudioClip deathSound;
    private AudioSource audioSource;

    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("Game Over")]
    public float delayBeforeGameOver = 2f;
    public GameObject gameOverPanel;

    [Header("UI Elements")]
    public Slider healthSlider;
    public Image sliderFillImage;
    public Image sliderBackgroundImage;

    [Header("Health Slider Colors")]
    public Color healthColorFull = Color.green;
    public Color healthColorMedium = Color.white;
    public Color healthColorLow = Color.red;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        // Setup canvas group for blood effect
        if (bloodSplatterImage != null)
        {
            canvasGroup = bloodSplatterImage.GetComponent<CanvasGroup>() ?? bloodSplatterImage.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
        }
        else
        {
            Debug.LogError("Blood Splatter Image not assigned!");
        }

        // Setup health slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        UpdateHealthUI();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        // For testing in editor
        //if (Input.GetKeyDown(KeyCode.Space)) TakeDamage();
        //if (Input.GetKeyDown(KeyCode.K))
        {
            //currentHealth = 0;
            //PlayerDied();
        }
    }

    public void TakeDamage()
    {
        if (currentHealth <= 0) return;

        currentHealth--;
        UpdateHealthUI();
        ShowBloodSplatter();
        PlayHurtSound();

        if (currentHealth <= 0)
        {
            PlayerDied();
        }
    }

    public void Heal()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.text = $"Health: {currentHealth}/{maxHealth}";

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
            UpdateSliderColor();
        }
    }

    void UpdateSliderColor()
    {
        if (sliderFillImage == null || sliderBackgroundImage == null) return;

        if (currentHealth == maxHealth)
            sliderFillImage.color = healthColorFull;
        else if (currentHealth == 2)
            sliderFillImage.color = healthColorMedium;
        else
            sliderFillImage.color = healthColorLow;

        sliderBackgroundImage.color = currentHealth <= 0 ? healthColorLow : Color.white;
    }

    void ShowBloodSplatter()
    {
        if (canvasGroup == null) return;

        StopAllCoroutines();
        canvasGroup.alpha = 1;
        StartCoroutine(FadeOutBloodSplatter());
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
            int index = Random.Range(0, hurtSounds.Length);
            audioSource.PlayOneShot(hurtSounds[index]);
        }
    }

    void PlayerDied()
    {
        Debug.Log("Player has died!");

        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (canvasGroup != null)
        {
            StopAllCoroutines();
            canvasGroup.alpha = 1;
        }

        StartCoroutine(EndGameAfterDelay());
    }

    IEnumerator EndGameAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);

        // Reload scene (recommended for VR projects)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
