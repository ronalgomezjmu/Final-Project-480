using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    // Gun prefabs
    public GameObject smgPrefab;
    public GameObject healItemPrefab;
    
    // Gun prices
    public int smgPrice = 30;
    public int healPrice = 20;
    
    // Spawn location
    public Transform gunSpawnPoint;
    
    // UI Buttons
    public Button smgButton;
    public Button healButton;
    
    void Start()
    {
        // Add listeners to buttons
        if (smgButton != null)
            smgButton.onClick.AddListener(BuySMG);
        else
            Debug.LogError("SMG Button not assigned in ShopManager!");
        
        if (healButton != null)
            healButton.onClick.AddListener(BuyHeal);
        else
            Debug.LogError("Heal Button not assigned in ShopManager!");
        
        // Ensure GunSpawnPoint is assigned
        if (gunSpawnPoint == null)
        {
            gunSpawnPoint = GameObject.Find("GunSpawnPoint")?.transform;
            if (gunSpawnPoint == null)
                Debug.LogError("GunSpawnPoint not found in scene!");
        }

        // Update UI buttons' interactable state
        UpdateButtonState();
    }

    // Add an Update method to check if player has enough points
    void Update()
    {
        // This ensures buttons are properly enabled/disabled based on current points
        UpdateButtonState();
    }
    
    void UpdateButtonState()
    {
        int currentPoints = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        if (smgButton != null)
            smgButton.interactable = currentPoints >= smgPrice;
        if (healButton != null)
            healButton.interactable = currentPoints >= healPrice;
    }
    
    public void BuySMG()
    {
        int currentPoints = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        Debug.Log("BuySMG method called. Current points: " + currentPoints + ", SMG price: " + smgPrice);
        if (currentPoints >= smgPrice)
        {
            ScoreManager.Instance.AddPoints(-smgPrice);
            if (gunSpawnPoint != null && smgPrefab != null)
            {
                Instantiate(smgPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
                Debug.Log("SMG purchased and spawned!");
            }
            else
            {
                Debug.LogError("Missing gunSpawnPoint or smgPrefab reference!");
            }
            UpdateButtonState();
        }
        else
        {
            Debug.Log("Not enough points to buy SMG!");
        }
    }
    
    public void BuyHeal()
    {
        int currentPoints = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0;
        Debug.Log("BuyHeal method called. Current points: " + currentPoints + ", Heal price: " + healPrice);
        if (currentPoints >= healPrice)
        {
            ScoreManager.Instance.AddPoints(-healPrice);
            if (gunSpawnPoint != null && healItemPrefab != null)
            {
                Instantiate(healItemPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
                Debug.Log("Heal purchased and spawned!");
            }
            else
            {
                Debug.LogError("Missing gunSpawnPoint or healItemPrefab reference!");
            }
            UpdateButtonState();
        }
        else
        {
            Debug.Log("Not enough points to buy Heal!");
        }
    }
}