using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    public bool destroyWithAnyObject = false;
    public string collisionTag = "";
    public int damageAmount = 1;

    void OnCollisionEnter(Collision collision)
    {
        // Check if we should destroy this object based on collision tag
        if (destroyWithAnyObject || (collision.gameObject.CompareTag(collisionTag) && !string.IsNullOrEmpty(collisionTag)))
        {
            // Look specifically for ZombieController 
            ZombieController zombieController = collision.gameObject.GetComponent<ZombieController>();
            
            if (zombieController != null)
            {
                // Apply damage to the zombie
                zombieController.TakeDamage(damageAmount);
            }
            
            // Always destroy the bullet
            Destroy(gameObject);
        }
    }
}