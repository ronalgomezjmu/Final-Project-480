using UnityEngine;
using UnityEngine.AI;

public class AttackBehaviour : StateMachineBehaviour
{
    private Transform player;
    private NavMeshAgent agent;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find references if needed
        if (player == null)
            player = GameObject.Find("XR Origin (XR Rig)").transform;
            
        if (agent == null)
            agent = animator.GetComponent<NavMeshAgent>();
            
        // Stop moving while attacking
        if (agent != null)
            agent.isStopped = true;
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Check distance when attack animation ends
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(animator.transform.position, player.position);
        animator.SetFloat("distanceToPlayer", distanceToPlayer);
        
        // Debug info
        Debug.Log("Attack finished, distance: " + distanceToPlayer);
    }
}