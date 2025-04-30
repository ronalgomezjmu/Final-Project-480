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

    private static readonly int Walk = Animator.StringToHash("rootZombie_Walk");
    private static readonly int Attack = Animator.StringToHash("rootZombie_Attack");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

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
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddPoints(10);
            }

            // Trigger death animation
            if (animator != null)
            {
                animator.SetTrigger("die");
            }

            // Disable movement
            agent.enabled = false;
            isAttacking = true;

            // Optionally play a death sound
            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null && deathClip != null)
            {
                audio.PlayOneShot(deathClip);
            }

            // Delay destruction so the animation can play
            Destroy(gameObject, 3f);
        }
    }

    }
