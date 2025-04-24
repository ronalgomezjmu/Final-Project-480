using UnityEngine;
using UnityEngine.Events;

public class XRPlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    
    public UnityEvent onDamage;
    public UnityEvent onDeath;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        
        if (onDamage != null)
            onDamage.Invoke();
            
        if (currentHealth <= 0)
        {
            if (onDeath != null)
                onDeath.Invoke();
                
            // Handle player death
            Debug.Log("Player died");
        }
    }
}