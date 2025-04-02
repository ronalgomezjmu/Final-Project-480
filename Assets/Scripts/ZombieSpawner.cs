using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int zombiesPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 30f;
    [SerializeField] private float timeBetweenSpawns = 2f;
    
    private int currentWave = 0;
    private int zombiesAlive = 0;
    
    void Start()
    {
        StartCoroutine(SpawnWave());
    }
    
    IEnumerator SpawnWave()
    {
        currentWave++;
        Debug.Log("Starting Wave " + currentWave);
        
        // Increase difficulty with each wave
        int zombiesToSpawn = zombiesPerWave + (currentWave - 1);
        
        if (currentWave == 1)
        {
            // First wave: Spawn zombies in a row using available spawn points
            int spawnPointsToUse = Mathf.Min(zombiesToSpawn, spawnPoints.Length);
            
            for (int i = 0; i < spawnPointsToUse; i++)
            {
                SpawnZombieAtPoint(spawnPoints[i]);
            }
            
            // If we need more zombies than spawn points, add them at random positions
            for (int i = spawnPointsToUse; i < zombiesToSpawn; i++)
            {
                Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                SpawnZombieAtPoint(randomPoint);
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }
        else
        {
            // Subsequent waves: Random spawning with delay between zombies
            for (int i = 0; i < zombiesToSpawn; i++)
            {
                SpawnZombie();
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }
        
        // Wait until all zombies in the wave are defeated
        while (zombiesAlive > 0)
        {
            yield return new WaitForSeconds(1f);
            // Debug.Log("Zombies still alive: " + zombiesAlive);
        }
        
        Debug.Log("Wave " + currentWave + " complete. Waiting for next wave...");
        
        // Wait before starting next wave
        yield return new WaitForSeconds(timeBetweenWaves);
        
        // Start next wave
        StartCoroutine(SpawnWave());
    }
    
    void SpawnZombie()
    {
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        SpawnZombieAtPoint(spawnPoint);
    }
    
    void SpawnZombieAtPoint(Transform spawnPoint)
    {
        // Spawn the zombie
        GameObject zombie = Instantiate(zombiePrefab, spawnPoint.position, Quaternion.identity);
        
        // Make zombie face toward the center of the play area
        Vector3 centerDirection = new Vector3(0, 0, 0) - spawnPoint.position;
        centerDirection.y = 0; // Keep upright
        if (centerDirection != Vector3.zero)
        {
            zombie.transform.rotation = Quaternion.LookRotation(centerDirection);
        }
        
        // Increase counter
        zombiesAlive++;
        
        // Add component to track when zombie is destroyed
        ZombieTracker tracker = zombie.AddComponent<ZombieTracker>();
        tracker.spawner = this;
    }
    
    public void ZombieDestroyed()
    {
        zombiesAlive--;
    }
}

// Helper class to track zombie destruction
public class ZombieTracker : MonoBehaviour
{
    public ZombieSpawner spawner;
    
    void OnDestroy()
    {
        if (spawner != null)
            spawner.ZombieDestroyed();
    }
}