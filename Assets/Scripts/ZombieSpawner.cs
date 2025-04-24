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
    public float spawnRate = 3f;             // Time between waves or replacements
    public float spawnDelay = 1f;            // NEW: Time between individual zombie spawns
    public float spawnPositionRadius = 5f;
    public float minDistanceBetweenZombies = 2f;
    
    [Header("Rising Animation")]
    public float riseHeight = 1.8f;
    public float riseDuration = 2f;
    
    private List<Vector3> activeZombiePositions = new List<Vector3>();
    private int zombiesAlive = 0;
    
    void Start()
    {
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
        Debug.Log("Starting zombie spawning process");
        
        while (zombiesAlive < maxZombies)
        {
            SpawnZombie();
            zombiesAlive++;
            yield return new WaitForSeconds(spawnDelay); // Changed to spawnDelay
        }
    }
    
    void SpawnZombie()
    {
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = GetSpawnPositionWithOffset(spawnPoint);
        
        if (spawnPosition != Vector3.zero)
        {
            // Create zombie below ground
            Vector3 belowGround = new Vector3(spawnPosition.x, spawnPosition.y - riseHeight, spawnPosition.z);
            
            Debug.Log("Spawning zombie at: " + belowGround);
            
            GameObject zombie = Instantiate(zombiePrefab, belowGround, Quaternion.identity);
            zombie.name = "Zombie_" + zombiesAlive;
            
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
        // Rest of the method unchanged
        Vector3 spawnPosition;
        int maxAttempts = 10;
        int attempts = 0;
        
        do
        {
            // Add random offset to spawn position
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
        
        // Ensure it's on the NavMesh
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
        // Method unchanged
        foreach (Vector3 existingPosition in activeZombiePositions)
        {
            float distance = Vector3.Distance(position, existingPosition);
            if (distance < minDistanceBetweenZombies)
            {
                return true; // Too close to an existing zombie
            }
        }
        return false; // Position is acceptable
    }
    
    IEnumerator RiseFromGround(GameObject zombie, Vector3 spawnPosition)
    {
        // Method unchanged
        Vector3 startPos = zombie.transform.position;
        Vector3 targetPos = spawnPosition;
        float elapsed = 0f;
        
        Debug.Log("Zombie rising from " + startPos + " to " + targetPos);
        
        // Temporarily disable nav agent and colliders
        NavMeshAgent agent = zombie.GetComponent<NavMeshAgent>();
        if (agent) agent.enabled = false;
        
        Collider[] colliders = zombie.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = false;
        
        // Spawn dirt particles
        if (dirtParticlesPrefab)
        {
            Instantiate(dirtParticlesPrefab, 
                new Vector3(spawnPosition.x, spawnPosition.y + 0.1f, spawnPosition.z), 
                Quaternion.Euler(270, 0, 0));
        }
        
        // Animate rising from ground
        while (elapsed < riseDuration)
        {
            zombie.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / riseDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        zombie.transform.position = targetPos;
        
        // Re-enable components
        if (agent) agent.enabled = true;
        foreach (Collider col in colliders) col.enabled = true;
    }
    
    public void ZombieDestroyed(Vector3 position)
    {
        activeZombiePositions.Remove(position);
        zombiesAlive--;
        
        // Spawn a new zombie to replace the destroyed one
        if (zombiesAlive < maxZombies)
        {
            StartCoroutine(SpawnReplacement());
        }
    }
    
    IEnumerator SpawnReplacement()
    {
        yield return new WaitForSeconds(spawnRate); // Keep using spawnRate for replacements
        SpawnZombie();
        zombiesAlive++;
    }
}