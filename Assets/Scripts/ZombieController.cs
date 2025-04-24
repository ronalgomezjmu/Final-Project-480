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
    
    // Animation state names - make sure these match your controller
    private static readonly int Walk = Animator.StringToHash("rootZombie_Walk");
    private static readonly int Attack = Animator.StringToHash("rootZombie_Attack");
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Find the XR Origin as the player
        player = GameObject.Find("XR Origin (XR Rig)").transform;
        
        // Immediately start walking - important change!
        if (animator != null)
        {
            animator.Play(Walk);
        }
        
        Debug.Log("Zombie initialized and set to walking state");
    }
    
    void Update()
    {
        if (player == null || agent == null || animator == null) return;
        
        // Skip if already attacking
        if (isAttacking) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Debug the distance
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Distance to player: " + distanceToPlayer);
        }
        
        // Always try to move toward player when not attacking
        agent.SetDestination(player.position);
        
        // Only attack when close enough AND cooldown passed
        if (distanceToPlayer <= attackDistance && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformAttack());
        }
    }
    
    System.Collections.IEnumerator PerformAttack()
    {
        // Stop moving
        agent.isStopped = true;
        isAttacking = true;
        
        // Face the player
        FaceTarget();
        
        Debug.Log("Starting attack animation");
        
        // Play attack animation
        animator.Play(Attack);
        
        // Set cooldown
        nextAttackTime = Time.time + attackCooldown;
        
        // Wait for attack animation to finish
        yield return new WaitForSeconds(2.0f); // Adjust based on animation length
        
        // Resume walking
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
}