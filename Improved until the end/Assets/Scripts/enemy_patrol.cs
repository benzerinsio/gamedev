using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy_patrol : MonoBehaviour
{
    //Movement section
    private bool isWaiting;
    private bool isFacingRight;
    private float speed;
    private float nonFollowing = 5f; //default speed
    private float followSpeed = 8f; // increase speed when following the player
    private float waitTime = 1f; //to go to another direction

    //Follow player Section
    [SerializeField] GameObject player;
    [SerializeField] Transform attackPoint; //from where the attack begins
    [SerializeField] Transform patrolPoint; // from where the enemy sees
    private bool inDashAnimation;
    private bool canDash;
    private float preDashTimer = 0.5f;
    private float dashTime = 0.5f;
    private float dashCooldown = 1f;
    private float dashPower = 28f; 
    private float playerinRange = 10f; //the minimum distance to dash towards the player
    private float distance; //distance between the enemy and the player
    private float viewRange = 20f; //distance of the vision of the enemy
    private bool isFollowing;

    //Dash Section
    private bool charging;
    private bool dashing;
    private SpriteRenderer sprite;

    //new tutorial section
    [SerializeField] GameObject pointA;
    [SerializeField] GameObject pointB;
    private Rigidbody2D rb;
    private Animator anim;
    private Transform currentPoint;

    //Attack Section
    private float attackRange = 2f;
    private bool canAttack = true;
    private float attackCooldown = 0.5f;

    //Animation Section
    private Animator animator;
    private enum MovementState { walk, run, charge, dash, attack };
    private MovementState state;


    void Start()
    {
        charging = false;
        dashing = false;
        state = MovementState.walk;
        canDash = true;
        inDashAnimation = false;
        isWaiting = false;
        isFollowing = false;
        speed = nonFollowing;
        isFacingRight = true;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        currentPoint = pointB.transform;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(patrolPoint.position, transform.TransformDirection(Vector2.right), viewRange);//get anything to get hit by the ray
        Debug.DrawRay(patrolPoint.position, patrolPoint.TransformDirection(Vector2.right) * viewRange, Color.red);//better to visualization
        if (hit)
        {
            if (hit.transform.name == "Player")
            {
                isFollowing = true;
                speed = followSpeed;
                distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance <= playerinRange && distance > attackRange && canDash)
                {
                    StartCoroutine(dashAttack());
                }
                else if(distance <= attackRange)
                {
                    speed = 0;
                    if (canAttack)
                    {
                        //Debug.Log("a");
                        StartCoroutine(attack());
                    }
                }
            }
        } else
        {
            isFollowing = false;
            speed = nonFollowing;
        }

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.1f && currentPoint == pointB.transform && speed > 0f && !isFollowing)
        {
            //StartCoroutine(changePoint(pointA));
            changePointNoWait(pointA);
        } else if (Vector2.Distance(transform.position, currentPoint.position) < 0.1f && currentPoint == pointA.transform && speed > 0f && !isFollowing)
        {
            //StartCoroutine(changePoint(pointB));
            changePointNoWait(pointB);
        }

        animationHandler();
    }

    private void FixedUpdate()
    {
        //Debug.Log(rb.velocity.x);
        if (inDashAnimation || !canAttack)//atualmente cancela o rotate e a velocidade
        {
            return;
        }
        
        if (rb.velocity.x > 0f && !isFacingRight)
        {
            Rotate();
        } else if (rb.velocity.x < 0f && isFacingRight)
        {
            Rotate();
        }


        if (!isFollowing)
        {
            if (currentPoint.position.x > transform.position.x)
            {
                rb.velocity = new Vector2(speed, 0f);
            }
            else if (currentPoint.position.x < transform.position.x)
            {
                rb.velocity = new Vector2(-speed, 0f);
            }
        } else
        {
            if (isFacingRight)
            {
                rb.velocity = new Vector2(speed, 0f);
            } else
            {
                rb.velocity = new Vector2(-speed, 0f);
            }
        }

    }

    private void Rotate()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    //private IEnumerator jumpCut()
    private IEnumerator dashAttack()
    {
        charging = true;
        inDashAnimation = true;
        canDash = false;
        rb.velocity = new Vector2(0f, 0f);
        //change animation to pre attack
        yield return new WaitForSeconds(preDashTimer);
        charging = false;
        dashing = true;
        //dashing
        //change animation to attack
        if (isFacingRight)
        {
            rb.velocity = new Vector2(dashPower, 0f);
        } else
        {
            rb.velocity = new Vector2(-dashPower, 0f);
        }
        yield return new WaitForSeconds(dashTime);
        inDashAnimation = false;
        dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;

        // rb.velocity = new Vector2(horizontalDirection * dashingPower, verticalDirection * dashingPower);
        //normal dash since is using 
    }

    private IEnumerator attack()
    {
        rb.velocity = new Vector2(0f, 0f);
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void changePointNoWait(GameObject point)
    {
        currentPoint = point.transform;
    }

    private void animationHandler()
    {
        Debug.Log(state);
        if (isFollowing && !charging && canAttack)
        {
            state = MovementState.run;
            animator.SetInteger("State", (int)state);
        }
        else if (charging)//verify how to change de sprite color
        {
            state = MovementState.charge;
            animator.SetInteger("State", (int)state);
        } else if (dashing || isFollowing && !canAttack) 
        {
            state = MovementState.dash;
            animator.SetInteger("State", (int)state);
        }
        else if (isFollowing && !canAttack)
        {
            state = MovementState.attack;
            animator.SetInteger("State", (int)state);
        } else
        {
            state = MovementState.walk;
            animator.SetInteger("State", (int)state);
        }
        
    }
}
