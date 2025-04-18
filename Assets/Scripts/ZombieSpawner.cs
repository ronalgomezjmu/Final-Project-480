using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int zombiesPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 30f;
    [SerializeField] private float timeBetweenSpawns = 2f;
    [SerializeField] private float spawnPositionRadius = 5f;
    [SerializeField] private float minDistanceBetweenZombies = 4f;
    
    // Add UI text components
    [SerializeField] private TextMeshProUGUI waveText; // For displaying current wave
    [SerializeField] private TextMeshProUGUI waveCompleteText; // For wave complete message
    [SerializeField] private float waveCompleteDisplayTime = 3f; // How long to show the message

    private int currentWave = 0;
    private int zombiesAlive = 0;
    private List<Vector3> activeSpawnPositions = new List<Vector3>();
    private bool waveInProgress = false;

    void Start()
    {
        // Hide the wave complete text at start
        if (waveCompleteText != null)
            waveCompleteText.gameObject.SetActive(false);
            
        // Update wave display
        UpdateWaveText();
        
        StartCoroutine(SpawnWave());
    }

    // New method to update the wave text
    void UpdateWaveText()
    {
        if (waveText != null)
            waveText.text = "Wave: " + currentWave;
    }

    // New method to display wave complete message
    IEnumerator ShowWaveCompleteMessage()
    {
        if (waveCompleteText != null)
        {
            waveCompleteText.text = "Wave " + currentWave + " Complete!";
            waveCompleteText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(waveCompleteDisplayTime);
            
            waveCompleteText.gameObject.SetActive(false);
        }
    }

    IEnumerator SpawnWave()
    {
        waveInProgress = true;
        currentWave++;
        Debug.Log("Starting Wave " + currentWave);
        
        // Update wave display
        UpdateWaveText();
        
        // Clear the list of active spawn positions at the start of each wave
        activeSpawnPositions.Clear();

        // Increase difficulty with each wave
        int zombiesToSpawn = zombiesPerWave + (currentWave - 1);
        zombiesAlive = zombiesToSpawn; // Set this before spawning to avoid race conditions

        if (currentWave == 1)
        {
            // First wave: Spawn zombies with random offsets using available spawn points
            int spawnPointsToUse = Mathf.Min(zombiesToSpawn, spawnPoints.Length);

            for (int i = 0; i < spawnPointsToUse; i++)
            {
                SpawnZombieWithOffset(spawnPoints[i]);
                yield return new WaitForSeconds(timeBetweenSpawns);
            }

            // If we need more zombies than spawn points, add them at random positions
            for (int i = spawnPointsToUse; i < zombiesToSpawn; i++)
            {
                Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                SpawnZombieWithOffset(randomPoint);
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
            Debug.Log("Zombies still alive: " + zombiesAlive);
        }

        Debug.Log("Wave " + currentWave + " complete. Waiting for next wave...");
        waveInProgress = false;

        // Award wave completion bonus
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddWaveCompletionBonus(currentWave);
        }

        // Show the wave complete message
        StartCoroutine(ShowWaveCompleteMessage());

        // Wait before starting next wave
        yield return new WaitForSeconds(timeBetweenWaves);

        // Start next wave
        StartCoroutine(SpawnWave());
    }

    void SpawnZombie()
    {
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        SpawnZombieWithOffset(spawnPoint);
    }

    void SpawnZombieWithOffset(Transform spawnPoint)
    {
        Vector3 spawnPosition;
        int maxAttempts = 10; // Limit attempts to prevent infinite loops
        int attempts = 0;
        
        do
        {
            // Add random offset to spawn position
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnPositionRadius, spawnPositionRadius),
                0, // Keep y-coordinate the same to avoid floating zombies
                Random.Range(-spawnPositionRadius, spawnPositionRadius)
            );
            
            spawnPosition = spawnPoint.position + randomOffset;
            attempts++;
            
            // If we've tried too many times, just use the last position to avoid an infinite loop
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning("Could not find suitable spawn position after " + maxAttempts + " attempts.");
                break;
            }
            
        } while (IsTooCloseToOtherZombies(spawnPosition));
        
        // Add this position to our active positions list
        activeSpawnPositions.Add(spawnPosition);
        
        // Spawn the zombie at the offset position
        GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
    
        // Make zombie face toward the center
        Vector3 centerDirection = new Vector3(0, 0, 0) - spawnPosition;
        centerDirection.y = 0; // Keep upright
        if (centerDirection != Vector3.zero)
        {
            zombie.transform.rotation = Quaternion.LookRotation(centerDirection);
        }
    
        // Set the health and pass the spawner reference directly
        ZombieController controller = zombie.GetComponent<ZombieController>();
        if (controller != null)
        {
            controller.SetHealth(this, currentWave);
        }
    }

    // Check if a position is too close to any existing zombies
    bool IsTooCloseToOtherZombies(Vector3 position)
    {
        foreach (Vector3 existingPosition in activeSpawnPositions)
        {
            float distance = Vector3.Distance(position, existingPosition);
            if (distance < minDistanceBetweenZombies)
            {
                return true; // Too close to an existing zombie
            }
        }
        return false; // Position is acceptable
    }

    public void ZombieDestroyed(Vector3 position)
    {
        zombiesAlive--;
        Debug.Log("Zombie destroyed! Zombies left: " + zombiesAlive);
        
        // Remove this position from our active positions list
        activeSpawnPositions.Remove(position);
    }
    
    // Method to check if a wave is currently in progress (for debugging)
    public bool IsWaveInProgress()
    {
        return waveInProgress;
    }
    
    // Method to get the current wave number (for scoring)
    public int GetCurrentWave()
    {
        return currentWave;
    }
}
