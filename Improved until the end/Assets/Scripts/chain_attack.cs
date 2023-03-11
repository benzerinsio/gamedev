using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chain_attack : StateMachineBehaviour
{
    private Rigidbody2D rb;
    private Transform transform;
    private float littleMove = 2f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        rb = animator.GetComponent<Rigidbody2D>();
        transform = animator.GetComponent<Transform>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //animator.GetComponent<Player>()."FUNCTIONNAME"(); //use a function in the player character to flip the sprite
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            animator.SetTrigger("Second_Attack");
            if (transform.rotation.y != 0f)//perform a little action towards the direction the player is facing
            {
                rb.velocity = new Vector2(-littleMove, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(littleMove, rb.velocity.y);
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("Second_Attack");
    }
}
