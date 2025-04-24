using UnityEngine;
using UnityEngine.AI;

public class DamageBehaviour : StateMachineBehaviour
{
    private Transform player;
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find references if needed
        if (player == null)
            player = GameObject.Find("XR Origin (XR Rig)").transform;
            
        if (player == null) return;
        
        // Check distance when damage animation ends
        float distanceToPlayer = Vector3.Distance(animator.transform.position, player.position);
        animator.SetFloat("distanceToPlayer", distanceToPlayer);
        
        // If still in attack range, trigger another attack
        if (distanceToPlayer < 4f)
        {
            Debug.Log("Still in range after damage animation, attacking again!");
            animator.Play("rootZombie_Attack");
        }
        else
        {
            Debug.Log("Out of range after damage, resuming walk");
            animator.Play("rootZombie_Walk");
            
            // Resume navigation
            NavMeshAgent agent = animator.GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.isStopped = false;
        }
    }
}