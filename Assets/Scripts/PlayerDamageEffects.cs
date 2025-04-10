using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageEffects : MonoBehaviour
{
    public Image bloodSplatterImage;
    private CanvasGroup canvasGroup;
    public AudioClip[] hurtSounds;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Get the Canvas Group
        if (bloodSplatterImage != null)
        {
            canvasGroup = bloodSplatterImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = bloodSplatterImage.gameObject.AddComponent<CanvasGroup>();
            }
            // Make sure image is invisible at start
            canvasGroup.alpha = 0;
            Debug.Log("Blood splatter system initialized");
        }
        else
        {
            Debug.LogError("Blood Splatter Image not assigned!");
        }
    }
    
    void Update()
    {
        // Test with keyboard input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed - showing blood splatter");
            ShowBloodSplatter();
            PlayHurtSound();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision with: " + other.gameObject.name);
        
        if (other.CompareTag("Zombie"))
        {
            Debug.Log("Zombie hit detected!");
            ShowBloodSplatter();
            PlayHurtSound();
        }
    }
    
    void ShowBloodSplatter()
    {
        Debug.Log("ShowBloodSplatter called. CanvasGroup is " + (canvasGroup != null ? "valid" : "NULL"));
        
        if (canvasGroup != null)
        {
            StopAllCoroutines();
            Debug.Log("Setting alpha to 1");
            canvasGroup.alpha = 1;
            StartCoroutine(FadeOutBloodSplatter());
        }
    }
    
    IEnumerator FadeOutBloodSplatter()
    {
        yield return new WaitForSeconds(2f);
        
        float fadeSpeed = 0.5f;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 0;
    }
    
    void PlayHurtSound()
    {
        if (audioSource != null && hurtSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, hurtSounds.Length);
            audioSource.PlayOneShot(hurtSounds[randomIndex]);
        }
    }
}