using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject zombiePrefab;
    public GameObject dirtParticlesPrefab;
    public Transform[] spawnPoints;
    public int maxZombies = 10;
    public float spawnRate = 3f;
    public float spawnDelay = 1f;
    public float spawnPositionRadius = 5f;
    public float minDistanceBetweenZombies = 2f;
    
    [Header("Rising Animation")]
    public float riseHeight = 1.8f;
    public float riseDuration = 2f;
    
    [Header("Zombie Type Settings")]
    public int zombieHealth = 1;
    
    private List<Vector3> activeZombiePositions = new List<Vector3>();
    private int zombiesAlive = 0;
    private bool spawningComplete = false;
    private int totalZombiesSpawned = 0;
    
    // Event for wave system
    public delegate void AllZombiesKilled();
    public event AllZombiesKilled onAllZombiesKilled;
    
    void OnEnable()
    {
        // Reset when enabled for a new wave
        zombiesAlive = 0;
        spawningComplete = false;
        totalZombiesSpawned = 0;
        activeZombiePositions.Clear();
        
        Debug.Log(gameObject.name + " enabled - Starting spawning process");
        
        // If no spawn points assigned, use this object's position as default
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = new Transform[1];
            spawnPoints[0] = transform;
            Debug.Log("No spawn points assigned. Using spawner position as default.");
        }
        
        StartCoroutine(SpawnZombies());
    }
    
    IEnumerator SpawnZombies()
    {
        Debug.Log("Starting zombie spawning process for " + gameObject.name);
        
        while (totalZombiesSpawned < maxZombies)
        {
            SpawnZombie();
            totalZombiesSpawned++;
            zombiesAlive++;
            yield return new WaitForSeconds(spawnDelay);
        }
        
        spawningComplete = true;
        Debug.Log("All zombies spawned for this wave. Spawned: " + totalZombiesSpawned + ", Alive: " + zombiesAlive);
        
        // Check if wave is complete immediately after spawning
        if (zombiesAlive <= 0)
        {
            Debug.Log("Wave complete immediately (no zombies alive)!");
            onAllZombiesKilled?.Invoke();
        }
    }
    
    public void ZombieDestroyed(Vector3 position)
    {
        activeZombiePositions.Remove(position);
        zombiesAlive--;
        
        Debug.Log(gameObject.name + " - Zombie destroyed. Zombies alive: " + zombiesAlive + ", Spawning complete: " + spawningComplete);
        
        // Check if wave is complete
        if (spawningComplete && zombiesAlive <= 0)
        {
            Debug.Log(gameObject.name + " - WAVE COMPLETE! Triggering event...");
            onAllZombiesKilled?.Invoke();
        }
    }
    
    // Rest of the methods remain the same...
    void SpawnZombie()
    {
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = GetSpawnPositionWithOffset(spawnPoint);
        
        if (spawnPosition != Vector3.zero)
        {
            // Create zombie below ground
            Vector3 belowGround = new Vector3(spawnPosition.x, spawnPosition.y - riseHeight, spawnPosition.z);
            
            Debug.Log("Spawning zombie " + totalZombiesSpawned + " at: " + belowGround);
            
            GameObject zombie = Instantiate(zombiePrefab, belowGround, Quaternion.identity);
            zombie.name = "Zombie_" + totalZombiesSpawned;
            
            // Set zombie health
            ZombieHealth healthComponent = zombie.GetComponent<ZombieHealth>();
            if (healthComponent == null)
            {
                healthComponent = zombie.AddComponent<ZombieHealth>();
            }
            healthComponent.maxHealth = zombieHealth;
            healthComponent.currentHealth = zombieHealth;
            healthComponent.spawner = this; // Important: link zombie to this spawner
            
            // Make zombie face toward the center
            Transform player = GameObject.Find("XR Origin (XR Rig)").transform;
            if (player)
            {
                Vector3 dirToPlayer = player.position - spawnPosition;
                dirToPlayer.y = 0;
                zombie.transform.rotation = Quaternion.LookRotation(dirToPlayer);
            }
            
            // Start rising animation
            StartCoroutine(RiseFromGround(zombie, spawnPosition));
            
            // Add to active positions list
            activeZombiePositions.Add(spawnPosition);
        }
        else
        {
            Debug.LogWarning("Could not find valid spawn position!");
        }
    }
    
    Vector3 GetSpawnPositionWithOffset(Transform spawnPoint)
    {
        Vector3 spawnPosition;
        int maxAttempts = 10;
        int attempts = 0;
        
        do
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnPositionRadius, spawnPositionRadius),
                0,
                Random.Range(-spawnPositionRadius, spawnPositionRadius)
            );
            
            spawnPosition = spawnPoint.position + randomOffset;
            attempts++;
            
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Could not find suitable spawn position after " + maxAttempts + " attempts.");
                return Vector3.zero;
            }
            
        } while (IsTooCloseToOtherZombies(spawnPosition));
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPosition, out hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            Debug.LogWarning("Position not on NavMesh: " + spawnPosition);
            return Vector3.zero;
        }
    }
    
    bool IsTooCloseToOtherZombies(Vector3 position)
    {
        foreach (Vector3 existingPosition in activeZombiePositions)
        {
            float distance = Vector3.Distance(position, existingPosition);
            if (distance < minDistanceBetweenZombies)
            {
                return true;
            }
        }
        return false;
    }
    
    IEnumerator RiseFromGround(GameObject zombie, Vector3 spawnPosition)
    {
        Vector3 startPos = zombie.transform.position;
        Vector3 targetPos = spawnPosition;
        float elapsed = 0f;
        
        Debug.Log("Zombie rising from " + startPos + " to " + targetPos);
        
        NavMeshAgent agent = zombie.GetComponent<NavMeshAgent>();
        if (agent) agent.enabled = false;
        
        Collider[] colliders = zombie.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = false;
        
        if (dirtParticlesPrefab)
        {
            Instantiate(dirtParticlesPrefab, 
                new Vector3(spawnPosition.x, spawnPosition.y + 0.1f, spawnPosition.z), 
                Quaternion.Euler(270, 0, 0));
        }
        
        while (elapsed < riseDuration)
        {
            zombie.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / riseDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        zombie.transform.position = targetPos;
        
        if (agent) agent.enabled = true;
        foreach (Collider col in colliders) col.enabled = true;
    }
}



public class ZombieHealth : MonoBehaviour
{
    public int maxHealth = 1;
    public int currentHealth;
    public ZombieSpawner spawner;
    
    private Animator animator;
    private NavMeshAgent agent;
    private bool isDead = false;
    
    // For death animation
    public AudioClip deathClip;
    private AudioSource audioSource;
    
    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
        Debug.Log("Zombie health started. Max health: " + maxHealth + ", Spawner: " + (spawner != null ? spawner.gameObject.name : "null"));
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return; // Don't take damage if already dead
        
        currentHealth -= damage;
        Debug.Log("Zombie took damage. Current health: " + currentHealth);
        
        if (currentHealth <= 0)
        {
            StartDeathSequence();
        }
    }
    
    void StartDeathSequence()
    {
        isDead = true;
        
        // Stop movement
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // Stop attacking (if zombie has attack component)
        ZombieController controller = GetComponent<ZombieController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("die");
        }
        
        // Play death sound
        if (audioSource != null && deathClip != null)
        {
            audioSource.PlayOneShot(deathClip);
        }
        
        // Delay destruction to allow animation to play
        StartCoroutine(DelayedDeath());
    }
    
    System.Collections.IEnumerator DelayedDeath()
    {
        // Wait for animation to finish (adjust time as needed)
        yield return new WaitForSeconds(3f);
        
        Die();
    }
    
    void Die()
    {
        Debug.Log("Zombie died. Notifying spawner...");
        
        // Notify the specific spawner that created this zombie
        if (spawner != null)
        {
            Debug.Log("Notifying spawner: " + spawner.gameObject.name);
            spawner.ZombieDestroyed(transform.position);
        }
        else
        {
            Debug.LogWarning("Spawner reference is null! Trying fallback method...");
            // Fallback method if spawner reference is null
            ZombieSpawner[] spawners = FindObjectsByType<ZombieSpawner>(FindObjectsSortMode.None);
            foreach (ZombieSpawner s in spawners)
            {
                s.ZombieDestroyed(transform.position);
            }
        }
        
        Destroy(gameObject);
    }
}