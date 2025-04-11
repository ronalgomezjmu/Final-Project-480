using UnityEngine;

public class DestroyOnCollision : MonoBehaviour
{
    public bool destroyWithAnyObject = false;
    public string collisionTag = "";
    public int damageAmount = 1;

    void OnCollisionEnter(Collision collision)
    {
        if (destroyWithAnyObject || collision.gameObject.CompareTag(collisionTag))
        {
            // Look specifically for ZombieController 
            ZombieController zombieController = collision.gameObject.GetComponent<ZombieController>();
            
            if (zombieController != null)
            {
                // Apply damage to the zombie
                zombieController.TakeDamage(damageAmount);
            }
            else
            {
                // If not a zombie, destroy it directly
                Destroy(collision.gameObject);
            }
            
            // Always destroy the bullet
            Destroy(gameObject);
        }
    }
}