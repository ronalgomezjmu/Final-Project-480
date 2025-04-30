using UnityEngine;
using UnityEngine.SceneManagement;

public class XRPlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Player hit! Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Player died!");
            ReturnToStartScene();
        }
    }

    void ReturnToStartScene()
    {
        // Replace "StartScene" with your actual scene name
        SceneManager.LoadScene("Zombiespawnertest");
    }
}
