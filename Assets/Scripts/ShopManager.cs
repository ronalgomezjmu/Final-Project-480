using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    // Reference to the player's points system
    public int playerPoints;
    
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
        // Update button interactability based on points
        if (smgButton != null)
            smgButton.interactable = playerPoints >= smgPrice;
            
        if (healButton != null)
            healButton.interactable = playerPoints >= healPrice;
    }
    
    public void BuySMG()
    {
        Debug.Log("BuySMG method called. Current points: " + playerPoints + ", SMG price: " + smgPrice);
        
        if (playerPoints >= smgPrice)
        {
            // Deduct points
            playerPoints -= smgPrice;
            
            // Spawn the gun at the designated spawn point
            if (gunSpawnPoint != null && smgPrefab != null)
            {
                Instantiate(smgPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
                Debug.Log("SMG purchased and spawned!");
            }
            else
            {
                Debug.LogError("Missing gunSpawnPoint or smgPrefab reference!");
            }
            
            // Update button state after purchase
            UpdateButtonState();
        }
        else
        {
            Debug.Log("Not enough points to buy SMG!");
        }
    }
    
    public void BuyHeal()
    {
        Debug.Log("BuyHeal method called. Current points: " + playerPoints + ", Heal price: " + healPrice);
        
        if (playerPoints >= healPrice)
        {
            // Deduct points
            playerPoints -= healPrice;
            
            // Spawn the heal item at the designated spawn point
            if (gunSpawnPoint != null && healItemPrefab != null)
            {
                Instantiate(healItemPrefab, gunSpawnPoint.position, gunSpawnPoint.rotation);
                Debug.Log("Heal purchased and spawned!");
            }
            else
            {
                Debug.LogError("Missing gunSpawnPoint or healItemPrefab reference!");
            }
            
            // Update button state after purchase
            UpdateButtonState();
        }
        else
        {
            Debug.Log("Not enough points to buy Heal!");
        }
    }
}