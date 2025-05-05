using UnityEngine;

public class FlickeringLight : MonoBehaviour {
    public Light lightComponent;
    public float minIntensity = 1.5f;
    public float maxIntensity = 1.75f;
    public float minTimeBetween = 2f;  // Minimum 3 seconds between flickers
    public float maxTimeBetween = 5f;  // Maximum 8 seconds between flickers
    
    void Start() {
        if (lightComponent == null) {
            lightComponent = GetComponentInChildren<Light>();
        }
        InvokeRepeating("Flicker", 2f, 0f);
    }
    
    void Flicker() {
        lightComponent.intensity = minIntensity;
        Invoke("RestoreLight", 0.1f);
        float nextFlicker = Random.Range(minTimeBetween, maxTimeBetween);
        CancelInvoke("Flicker");
        Invoke("Flicker", nextFlicker);
    }
    
    void RestoreLight() {
        lightComponent.intensity = maxIntensity;
    }
}