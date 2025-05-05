using System.Collections;
using UnityEngine;
using TMPro; // For TextMeshPro UI

[System.Serializable]
public class Wave
{
    public string waveName = "Wave 1";
    public ZombieSpawner spawner;
    public float waveStartDelay = 3f;
    public float waveEndDelay = 5f;   
}

public class WaveSystem : MonoBehaviour
{
    public Wave[] waves;
    public TextMeshProUGUI waveText;
    public float textDisplayDuration = 3f;
    
    private int currentWaveIndex = 0;
    private bool isWaveActive = false;
    
    void Start()
    {
        Debug.Log("Wave System starting...");
        
        // Disable all spawners at start
        foreach (Wave wave in waves)
        {
            if (wave.spawner)
            {
                wave.spawner.enabled = false;
                Debug.Log("Disabled " + wave.spawner.gameObject.name);
            }
        }
        
        StartCoroutine(StartWaveSequence());
    }
    
    IEnumerator StartWaveSequence()
    {
        while (currentWaveIndex < waves.Length)
        {
            Debug.Log("Starting wave " + currentWaveIndex);
            
            // Show wave text
            ShowWaveText(waves[currentWaveIndex].waveName);
            yield return new WaitForSeconds(textDisplayDuration);
            HideWaveText();
            
            // Wait for wave start delay
            Debug.Log("Waiting for wave start delay: " + waves[currentWaveIndex].waveStartDelay);
            yield return new WaitForSeconds(waves[currentWaveIndex].waveStartDelay);
            
            // Start the current wave
            Debug.Log("Starting wave: " + waves[currentWaveIndex].waveName);
            StartWave(currentWaveIndex);
            
            // Wait for wave to complete
            while (isWaveActive)
            {
                yield return null;
            }
            
            Debug.Log("Wave complete! Waiting for end delay...");
            
            // Wait for end delay
            yield return new WaitForSeconds(waves[currentWaveIndex].waveEndDelay);
            
            currentWaveIndex++;
        }
        
        Debug.Log("All waves completed!");
    }
    
    void StartWave(int waveIndex)
    {
        isWaveActive = true;
        waves[waveIndex].spawner.enabled = true;
        waves[waveIndex].spawner.onAllZombiesKilled += OnWaveComplete;
        Debug.Log("Wave " + waveIndex + " started. Enabled spawner: " + waves[waveIndex].spawner.gameObject.name);
    }
    
    void OnWaveComplete()
    {
        Debug.Log("OnWaveComplete called for wave " + currentWaveIndex);
        waves[currentWaveIndex].spawner.onAllZombiesKilled -= OnWaveComplete;
        waves[currentWaveIndex].spawner.enabled = false;
        isWaveActive = false;
        Debug.Log("Wave " + currentWaveIndex + " completed. Setting isWaveActive to false");
    }
    
    void ShowWaveText(string text)
    {
        if (waveText)
        {
            waveText.text = text;
            waveText.gameObject.SetActive(true);
            Debug.Log("Showing wave text: " + text);
        }
    }
    
    void HideWaveText()
    {
        if (waveText)
        {
            waveText.gameObject.SetActive(false);
            Debug.Log("Hiding wave text");
        }
    }
}