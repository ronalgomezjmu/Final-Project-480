using UnityEngine;
using UnityEngine.AI;

public class WalkToAttack : StateMachineBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    
    // OnStateEnter is called when the animation state starts
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find the player (XR Origin)
        player = GameObject.Find("XR Origin (XR Rig)").transform;
        
        // Get the NavMeshAgent component from the zombie
        agent = animator.GetComponent<NavMeshAgent>();
        
        if (agent == null)
        {
            Debug.LogError("No NavMeshAgent found on zombie!");
        }
    }

    // OnStateUpdate is called every frame while in this state
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null || agent == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(animator.transform.position, player.position);
        
        // Update the distance parameter in the animator
        animator.SetFloat("distanceToPlaer", distanceToPlayer);
        
        // Update destination to follow player
        agent.SetDestination(player.position);
        
        // Debug distance in console (optional)
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Distance to player: " + distanceToPlayer);
        }
    }
}