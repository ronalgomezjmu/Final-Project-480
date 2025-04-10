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
            // Check if the object has a ZombieHealth component
            ZombieHealth zombieHealth = collision.gameObject.GetComponent<ZombieHealth>();
            
            if (zombieHealth != null)
            {
                // Apply damage to the zombie
                zombieHealth.TakeDamage(damageAmount);
            }
            else
            {
                // If not a zombie or doesn't have health, destroy it directly (old behavior)
                Destroy(collision.gameObject);
            }
            
            // Always destroy the bullet
            Destroy(gameObject);
        }
    }
}