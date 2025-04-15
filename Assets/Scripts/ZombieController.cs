using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class ZombieController : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float emergeSpeed = 1f;
    [SerializeField] private float emergeDuration = 2f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackDistance = 2f;
    
    [Header("Animation")]
    [SerializeField] private float animationSpeedMultiplier = 1f;
    [SerializeField] private float footStepHeight = 0.1f;
    
    [Header("Effects")]
    [SerializeField] private GameObject dirtEffectPrefab;
    
    [Header("Audio")]
    [SerializeField] private AudioClip zombieSound;
    [SerializeField] private float minTimeBetweenSounds = 3f;
    [SerializeField] private float maxTimeBetweenSounds = 8f;
    [SerializeField] private float zombieSoundVolume = 0.7f;
    
    [Header("Player Damage")]
    [SerializeField] private float damageRange = 1.5f;
    [SerializeField] private float damageInterval = 1.0f;
    private float nextDamageTime = 0f;
    private PlayerDamageEffects playerDamageEffects;
    
    [Header("Death Settings")]
    [SerializeField] private float deathStayTime = 5f;
    [SerializeField] private float deathFadeTime = 2f;
    
    private Transform player;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private Vector3 startPosition;
    private float emergeTimer = 0f;
    private bool hasEmerged = false;
    private float lastStepTime = 0f;
    private float nextSoundTime = 0f;
    
    // Animation parameter check
    private bool hasChaseParameter = false;
    private bool hasAttackParameter = false;
    private bool hasEmergeParameter = false;
    private bool hasSpeedParameter = false;
    private bool hasIsWalkingParameter = false;
    private bool hasDeathParameter = false;
    
    // States
    private enum ZombieState { Emerging, Chasing, Attacking, Dying, Dead }
    private ZombieState currentState;

    [Header("Health")]
    private int maxHealth;
    private int currentHealth;
    private ZombieSpawner spawner;
    private Vector3 spawnPosition; // Store the spawn position for reference when destroyed
    private int waveNumber; // Store the wave number for score calculation

    public void SetHealth(ZombieSpawner spawnerRef, int wave)
    {
        spawner = spawnerRef;
        waveNumber = wave; // Store the wave number for scoring
        // Health starts at 2 for wave 1, and increases by 1 for each wave
        maxHealth = 1 + wave;
        currentHealth = maxHealth;
        // Store the spawn position
        spawnPosition = transform.position;
        Debug.Log("Zombie initialized with " + currentHealth + " health in wave " + wave);
    }

    public void TakeDamage(int damage)
    {
        // Only process damage if not already dying or dead
        if (currentState == ZombieState.Dying || currentState == ZombieState.Dead)
            return;
            
        currentHealth -= damage;
        Debug.Log("Zombie took damage! Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("Zombie is defeated!");
            
            // Award points to the player through ScoreManager
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddZombieKillPoints(waveNumber, transform.position);
            }
            
            // Notify the spawner immediately that this zombie is dead
            if (spawner != null)
            {
                spawner.ZombieDestroyed(spawnPosition);
            }
            
            // Then trigger death sequence
            StartCoroutine(DieWithPhysics());
        }
    }
    
    // New coroutine for physics-based death
    private IEnumerator DieWithPhysics()
    {
        // Set state to dying
        currentState = ZombieState.Dying;

        // Disable the NavMeshAgent so it stops navigating
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Get or add a Rigidbody for physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Make sure rigidbody is not kinematic and uses gravity
        rb.isKinematic = false;
        rb.useGravity = true;

        // Determine forward direction so we can fall face down
        Vector3 forwardDir = transform.forward;

        // Stop any current movement
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Apply force to make it fall forward (face down)
        rb.AddForce(forwardDir * 2.5f, ForceMode.Impulse);
        // Add some downward force to make it fall faster
        rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
        // Add some torque to make it rotate forward (face down)
        rb.AddTorque(-transform.right * 5f, ForceMode.Impulse);

        // Play death sound
        PlayZombieSound();

        // Disable any colliders that might interfere with falling
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            // Keep the main collider but make it non-trigger
            col.isTrigger = false;
        }

        // Play death animation if available
        if (animator != null)
        {
            // Check if you have a death animation parameter
            animator.SetBool("IsWalking", false);

            // If you have a "Death" parameter, use it
            // Otherwise, you could disable the animator to let physics take over
            if (hasDeathParameter)
            {
                animator.SetTrigger("Death");
            }
            else
            {
                animator.enabled = false;
            }
        }

        // Set state to dead
        currentState = ZombieState.Dead;

        // Wait for some time to let the zombie stay visible after death
        yield return new WaitForSeconds(deathStayTime);

        // Slowly fade out
        float elapsed = 0;

        // Get all renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Original materials and colors
        List<Material> materials = new List<Material>();
        List<Color> originalColors = new List<Color>();

        // Store original colors
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                materials.Add(mat);
                originalColors.Add(mat.color);
            }
        }

        // Fade out loop
        while (elapsed < deathFadeTime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / deathFadeTime;

            // Fade out all materials
            for (int i = 0; i < materials.Count; i++)
            {
                Color newColor = originalColors[i];
                newColor.a = Mathf.Lerp(originalColors[i].a, 0f, normalizedTime);
                materials[i].color = newColor;
            }

            yield return null;
        }

        // Finally destroy the gameObject
        Destroy(gameObject);
    }
    
    // Rest of the original ZombieController methods remain the same
    // ... (Start, Update, PlayZombieSound, HandleEmerging, ChasePlayer, AttackPlayer, ReturnToChasing)
    
    void Start()
    {
        // Find the XR Origin/Player
        GameObject xrOrigin = GameObject.FindWithTag("XR Origin");
        if (xrOrigin != null)
        {
            Camera xrCamera = xrOrigin.GetComponentInChildren<Camera>();
            if (xrCamera != null)
                player = xrCamera.transform;
            else
                player = xrOrigin.transform;
        }
        else
        {
            player = Camera.main.transform;
        }
        
        // Find player damage effects component
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            playerObject = GameObject.FindWithTag("XR Origin"); // Try alternative tag
        }
        
        if (playerObject != null)
        {
            playerDamageEffects = playerObject.GetComponent<PlayerDamageEffects>();
            if (playerDamageEffects == null)
            {
                // Try to find it in children
                playerDamageEffects = playerObject.GetComponentInChildren<PlayerDamageEffects>();
            }
            
            if (playerDamageEffects == null)
            {
                Debug.LogWarning("PlayerDamageEffects component not found on player!");
            }
            else
            {
                Debug.Log("Found PlayerDamageEffects component");
            }
        }
        
        // Cache other references
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Set up audio source if it doesn't exist
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // Make sound 3D
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
        }
        
        // Set the zombie sound clip if not assigned in inspector
        if (zombieSound == null)
        {
            // Try to find the sound in the project
            zombieSound = Resources.Load<AudioClip>("454832__misterkidx__zombie_walk_1");
        }
        
        // Schedule first sound
        nextSoundTime = Time.time + Random.Range(0.1f, 1.0f);
        
        // Check which animation parameters exist
        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Chase") hasChaseParameter = true;
                if (param.name == "Attack") hasAttackParameter = true;
                if (param.name == "Emerge") hasEmergeParameter = true;
                if (param.name == "Speed") hasSpeedParameter = true;
                if (param.name == "IsWalking") hasIsWalkingParameter = true;
                if (param.name == "Death") hasDeathParameter = true;
            }
        }
        
        // Setup for emerging from ground
        startPosition = transform.position;
        transform.position = new Vector3(startPosition.x, startPosition.y - 1.5f, startPosition.z);
        
        // Disable NavMeshAgent until fully emerged
        if (agent != null)
        {
            agent.enabled = false;
            agent.speed = moveSpeed;
        }
        
        currentState = ZombieState.Emerging;
        
        // Set animation state if you have an emerge animation
        if (animator != null && hasEmergeParameter)
            animator.SetTrigger("Emerge");
            
        // Play initial zombie sound
        PlayZombieSound();
    }
    
    void Update()
    {
        // Only process AI logic if not dying or dead
        if (currentState != ZombieState.Dying && currentState != ZombieState.Dead)
        {
            switch (currentState)
            {
                case ZombieState.Emerging:
                    HandleEmerging();
                    break;
                case ZombieState.Chasing:
                    ChasePlayer();
                    break;
                case ZombieState.Attacking:
                    AttackPlayer();
                    break;
            }
            
            // Periodically play zombie sounds
            if (Time.time > nextSoundTime)
            {
                PlayZombieSound();
                // Schedule next sound
                nextSoundTime = Time.time + Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
            }
        }
    }
    
    void PlayZombieSound()
    {
        if (audioSource != null && zombieSound != null)
        {
            audioSource.clip = zombieSound;
            audioSource.volume = zombieSoundVolume;
            audioSource.pitch = Random.Range(0.8f, 1.2f); // Add some variety
            audioSource.Play();
        }
    }
    
    void HandleEmerging()
    {
        if (emergeTimer < emergeDuration)
        {
            emergeTimer += Time.deltaTime;
            float t = emergeTimer / emergeDuration;
            
            // Move zombie upward from the ground
            transform.position = Vector3.Lerp(
                new Vector3(startPosition.x, startPosition.y - 1.5f, startPosition.z),
                startPosition,
                t
            );
            
            // Spawn dirt effect at the beginning of emergence
            if (emergeTimer < 0.1f && dirtEffectPrefab != null)
            {
                Instantiate(dirtEffectPrefab, 
                    new Vector3(transform.position.x, startPosition.y, transform.position.z), 
                    Quaternion.identity);
            }
            
            // Add some zombie-like movement while emerging
            transform.Rotate(0, Random.Range(-5f, 5f) * Time.deltaTime, 0);
        }
        else if (!hasEmerged)
        {
            hasEmerged = true;
            currentState = ZombieState.Chasing;
            
            // Enable NavMesh Agent for pathfinding to player
            if (agent != null)
            {
                agent.enabled = true;
            }
            
            // Set animation state to walking/chasing
            if (animator != null)
            {
                if (hasChaseParameter)
                    animator.SetTrigger("Chase");
                
                if (hasIsWalkingParameter)
                    animator.SetBool("IsWalking", true);
                
                if (hasSpeedParameter)
                    animator.SetFloat("Speed", moveSpeed * animationSpeedMultiplier);
            }
            
            // Play sound when finished emerging
            PlayZombieSound();
        }
    }
    
    void ChasePlayer()
    {
        if (player != null && agent != null && agent.enabled)
        {
            // Update destination to player's position
            agent.SetDestination(player.position);
            
            // Get the desired movement from the NavMeshAgent
            Vector3 targetPosition = agent.nextPosition;
            
            // Calculate distance to target
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            
            if (distanceToTarget > 0.1f)
            {
                // Get desired rotation from the NavMeshAgent's desired velocity
                if (agent.velocity.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(agent.desiredVelocity);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
                
                // Update the animation based on actual movement
                if (animator != null)
                {
                    // Make sure the zombie is walking
                    if (hasIsWalkingParameter)
                        animator.SetBool("IsWalking", true);
                    
                    // Update animation speed
                    if (hasSpeedParameter)
                    {
                        float actualSpeed = agent.velocity.magnitude / agent.speed;
                        animator.SetFloat("Speed", actualSpeed * animationSpeedMultiplier);
                    }
                }
                
                // Update position for NavMeshAgent
                transform.position = targetPosition;
            }
            else
            {
                // If not moving, make sure animation reflects that
                if (animator != null && hasIsWalkingParameter)
                    animator.SetBool("IsWalking", false);
            }
            
            // Check if close enough to attack
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer < attackDistance)
            {
                currentState = ZombieState.Attacking;
                if (animator != null && hasAttackParameter)
                {
                    animator.SetTrigger("Attack");
                    if (hasIsWalkingParameter)
                        animator.SetBool("IsWalking", false);
                }
                
                // Play attack sound
                PlayZombieSound();
            }
        }
    }
    
    void AttackPlayer()
    {
        // Face the player while attacking
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * rotationSpeed
                );
            }
            
            // Check if it's time to damage the player
            if (Time.time >= nextDamageTime)
            {
                // Check if player is in range
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= damageRange)
                {
                    // Apply damage to player
                    if (playerDamageEffects != null)
                    {
                        Debug.Log("Zombie attacking player! Calling TakeDamage()");
                        playerDamageEffects.TakeDamage();
                    }
                    
                    // Set next damage time
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }
        
        // Wait for attack animation to finish
        Invoke("ReturnToChasing", 1f);
    }
    
    void ReturnToChasing()
    {
        // Only return to chasing if not dying or dead
        if (currentState != ZombieState.Dying && currentState != ZombieState.Dead)
        {
            currentState = ZombieState.Chasing;
            
            if (animator != null)
            {
                if (hasChaseParameter)
                    animator.SetTrigger("Chase");
                    
                if (hasIsWalkingParameter)
                    animator.SetBool("IsWalking", true);
            }
        }
    }
}