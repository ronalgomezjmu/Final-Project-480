using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarrierManager : MonoBehaviour
{
    public GameObject barrier; // Reference to the barrier object
    public Button unlockButton; // Reference to the unlock button
    public int unlockCost = 200; // Cost to unlock the barrier
    public TextMeshProUGUI costText; // Text to display the cost

    private void Start()
    {
        // Initialize the button
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(UnlockBarrier);
            UpdateButtonState();
        }

        // Set initial cost text
        if (costText != null)
        {
            costText.text = $"Unlock Cost: {unlockCost}";
        }
    }

    private void Update()
    {
        // Update button interactability based on current score
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (unlockButton != null)
        {
            unlockButton.interactable = ScoreManager.Instance != null && 
                                      ScoreManager.Instance.GetScore() >= unlockCost;
        }
    }

    private void UnlockBarrier()
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.GetScore() >= unlockCost)
        {
            // Deduct points
            ScoreManager.Instance.AddPoints(-unlockCost);
            
            // Disable the barrier
            if (barrier != null)
            {
                barrier.SetActive(false);
            }
            
            // Disable the button
            if (unlockButton != null)
            {
                unlockButton.gameObject.SetActive(false);
            }
        }
    }
} 