using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    private int maxHealth;
    private int currentHealth;
    private ZombieSpawner spawner;

    public void Initialize(ZombieSpawner spawnerRef, int wave)
    {
        spawner = spawnerRef;
        // Health starts at 2 for wave 1, and increases by 1 for each wave
        maxHealth = 1 + wave;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // You could add hit effects or animations here
        
        if (currentHealth <= 0)
        {
            // Zombie is defeated
            Destroy(gameObject);
        }
    }
}