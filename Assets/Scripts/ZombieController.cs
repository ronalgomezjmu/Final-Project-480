using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    public float attackDistance = 1.5f;
    public float detectionRange = 10f;
    public float attackCooldown = 2.0f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    public AudioClip deathClip;
    public AudioClip walkingClip; // Add walking sound clip
    public float walkingSoundVolume = 0.5f; // Volume control for walking sound
    private AudioSource audioSource; // Audio source component
    private ZombieHealth zombieHealth; // Reference to health component

    private static readonly int Walk = Animator.StringToHash("rootZombie_Walk");
    private static readonly int Attack = Animator.StringToHash("rootZombie_Attack");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        zombieHealth = GetComponent<ZombieHealth>(); // Get the health component
        audioSource = GetComponent<AudioSource>(); // Get the audio source component

        // Find the player
        GameObject playerObject = GameObject.Find("XR Origin (XR Rig)");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found in scene!");
        }

        // Start walking animation
        if (animator != null)
        {
            animator.Play(Walk);
        }

        // Start playing walking sound (looped)
        if (audioSource != null && walkingClip != null)
        {
            audioSource.clip = walkingClip;
            audioSource.loop = true;
            audioSource.volume = walkingSoundVolume;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.Play();
        }

        Debug.Log("Zombie initialized and set to walking state");
    }

    void Update()
    {
        if (player == null || agent == null || animator == null) return;
        if (isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Move toward player if on NavMesh
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);

            // Play walking sound if moving
            if (agent.velocity.magnitude > 0.1f && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            // Pause walking sound if stopped
            else if (agent.velocity.magnitude <= 0.1f && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }

        // Attack if close enough and cooldown expired
        if (distanceToPlayer <= attackDistance && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformAttack());
        }
    }

    System.Collections.IEnumerator PerformAttack()
    {
        agent.isStopped = true;
        isAttacking = true;

        // Pause walking sound during attack
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }

        FaceTarget();
        animator.Play(Attack);

        nextAttackTime = Time.time + attackCooldown;

        yield return new WaitForSeconds(2.0f); // adjust for your animation length

        // Call damage method from PlayerDamageEffects script
        PlayerDamageEffects damageEffects = player.GetComponent<PlayerDamageEffects>();
        if (damageEffects != null)
        {
            damageEffects.TakeDamage(); // uses your blood splatter + health system
        }

        animator.Play(Walk);
        agent.isStopped = false;
        isAttacking = false;

        // Resume walking sound
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        Debug.Log("Attack finished, resuming walking");
    }

    void FaceTarget()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log("Bullet hit zombie - Taking damage");

            // Add points
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoints(10);
            }

            // IMPORTANT: Call TakeDamage instead of handling death directly
            if (zombieHealth != null)
            {
                zombieHealth.TakeDamage(1);
            }

            // Remove the direct death handling - let ZombieHealth handle it
            // Comment out or remove these lines:
            /*
            // Trigger death animation
            if (animator != null)
            {
                animator.SetTrigger("die");
            }

            // Disable movement
            agent.enabled = false;
            isAttacking = true;

            // Destroy the object
            Destroy(gameObject, 3f);
            */
        }
    }
}