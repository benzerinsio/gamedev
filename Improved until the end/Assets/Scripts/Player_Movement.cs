using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Movement : MonoBehaviour
{
    //Components
    private Rigidbody2D rb;
    private BoxCollider2D bc;

    //Horizontal Move
    private bool isFacingRight = true;
    private float horizontalDirection;
    private float horizontalSpeed = 10f;

    //Vertical Move
    private float gravityScale;
    private bool inMaxHeight = false;
    private bool inJumpCut = false;
    private bool isFalling = false;
    private float verticalDirection;
    private float coyoteTime = 0.1f;
    private float coyoteCounter;
    private float jumpBuffer = 0.1f;
    private float jumpBufferCounter;
    private float jumpPower = 16f;
    private float fallIncreaseSpeed = -70f;
    private float maxFallSpeed = -25f;

    //Layers

    //Ground layer
    [SerializeField] private LayerMask jumpableGround;

    //Enemy
    [SerializeField] private LayerMask enemyLayers;

    //Dash
    private TrailRenderer tr;
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    //Animations
    private Animator animator;
    private enum MovementState { idle, running, jumping, maxHeight, falling, attacking };
    private MovementState state;

    //Attack
    [SerializeField] private Transform attackPoint;
    private float attackBuffer = 0.1f;//if the player press the button starts the buffer, to set 
    public float attackRange = 1.09f;
    private float attackCounter;
    private bool isAttacking = false;
    private bool inSecondAttack = false;
    private bool canAttack = true;

    //Debug Tools
    private int bugCounter = 0;
    void Start()
    {
        tr = GetComponent<TrailRenderer>();
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        gravityScale = rb.gravityScale;
        attackCounter = -0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalDirection = Input.GetAxisRaw("Horizontal");
        verticalDirection = Input.GetAxisRaw("Vertical");

        jumpBufferCounter -= Time.deltaTime;
        attackCounter -= Time.deltaTime;

        if(isGrounded() && !isAttacking)
        {
            canAttack = true;
        }

        if (isGrounded())  //setting coyote time
        {
            isFalling = false;
            inJumpCut = false;
            inMaxHeight = false;
            coyoteCounter = coyoteTime;
        }
        else
        {
            //canAttack = false;
            coyoteCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump")) //jump buffer
        {
            jumpBufferCounter = jumpBuffer;
        }
        if (Input.GetButtonDown("Attack"))//do certain verification -> if the player is in a state that cannot attack, start the counter, if not, do the attack
        {
            attackCounter = attackBuffer;
        }

        if(attackCounter >=0f && canAttack && !inSecondAttack)//verificar se ta funcionando o segundo attack
        {
            canAttack = false;

            if (isAttacking || !isGrounded())
            {
                Attack(2);//first attack animation (or dash attack)
            }
            else
            {
                Attack(1);//second attack animation
            }
        }
        animationHandler();

        if (isDashing || isAttacking)//if is dashing, can't perform another action (do da same with the attack)
        {
            return;
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f )//&& !isDashing) // jump cut
        {
            StartCoroutine(jumpCut());
        }

        if(!isGrounded() && rb.velocity.y == 0f && !inJumpCut && !inMaxHeight) // make the max height smooth
        {
            StartCoroutine(maxHeightJump());
        }

        if (Input.GetButtonDown("Dash") && canDash)
        {
            StartCoroutine(Dash());
        }
        if (Input.GetButtonDown("Crouch"))
        {
            //Debug.Log("Crouching");
        }
    }

    private void FixedUpdate()
    {
        if (isDashing || isAttacking)//if is dashing, can't perform another action (do the same for the attack)
        {
            return;
        }

        if (horizontalDirection < 0f && isFacingRight)
        {
            Rotate();
        }
        else if (horizontalDirection > 0f && !isFacingRight)
        {
            Rotate();
        }

        rb.velocity = new Vector2(horizontalDirection * horizontalSpeed, rb.velocity.y);

        if (jumpBufferCounter > 0f && coyoteCounter > 0f) //perform the jump action
        {
            jump();
        }

        if (rb.velocity.y < 0f && !isFalling || isFalling && rb.velocity.y > maxFallSpeed)//increases the speed when falling
        {
            falling();
        }
        if (isFalling && rb.velocity.y <= maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
        }
    }

    private void falling()
    {
        isFalling = true;
        rb.AddForce(new Vector2(0f, fallIncreaseSpeed), ForceMode2D.Force);
    }
    private bool isGrounded()
    {
        return Physics2D.BoxCast(bc.bounds.center/*the actual position*/, bc.bounds.size/*size of each axis*/, 0f, Vector2.down/*or Vector2(0, -1)*/, .1f, jumpableGround);
    }

    private void Attack(int which)
    {
        animator.SetBool("isDashing", false);
        if(which == 1)
        {
            StartCoroutine(firstAttackRoutine());
        } else if (which == 2)
        {
            StartCoroutine(secondAttackRoutine());
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    private void jump()
    {
        isFalling = false;
        coyoteCounter = 0; //makes sure that the jump will get always the same height
        rb.velocity = new Vector2(rb.velocity.x, jumpPower);
    }

    private IEnumerator jumpCut()//makes the cut more smooth
    {
        float aux = rb.gravityScale / 2;
        inJumpCut = true;
        rb.gravityScale = aux;
        rb.velocity = new Vector2(rb.velocity.x, 1f);
        yield return new WaitForSeconds(0.2f);
        if(rb.gravityScale == aux)//makes sure to not mess up the other possible gravity change
        {
            rb.gravityScale = gravityScale;
        }
    }

    private IEnumerator maxHeightJump()//makes max height transition more smooth
    {
        inMaxHeight = true;
        rb.gravityScale /= 2;
        yield return new WaitForSeconds(0.1f);
        rb.gravityScale = gravityScale;
    }

    private IEnumerator firstAttackRoutine()
    {
        isAttacking = true;//
        rb.velocity = new Vector2(0f, 0f);
        yield return new WaitForSeconds(0.4f);
        canAttack = true;
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("dano no troxa: " + enemy.name);
        }
        yield return new WaitForSeconds(0.2f);
        if (!inSecondAttack)
        {
            isAttacking = false;
        }
    }

    private IEnumerator secondAttackRoutine()
    {
        canAttack = false;
        inSecondAttack = true;
        yield return new WaitForSeconds(0.1f);
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("dano no troxa: " + enemy.name);
        }
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
        inSecondAttack = false;
    }

    private void Rotate()
    {
        isFacingRight = !isFacingRight;

        transform.Rotate(0f,180f,0f);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(horizontalDirection * dashingPower, verticalDirection * dashingPower);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(0f, 0f);
        tr.emitting = false;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void animationHandler()
    {
        //running / idle section
        if(rb.velocity.x != 0f && isGrounded())
        {
            state = MovementState.running;
            animator.SetInteger("State", (int)state);
        }
        else if (rb.velocity.x == 0f && isGrounded())
        {
            state = MovementState.idle;
            animator.SetInteger("State", (int)state);
        }

        //jumping / maxheight / fall section
        if(rb.velocity.y > 0f && !isGrounded())
        {
            state = MovementState.jumping;
            animator.SetInteger("State", (int)state);
        }
        else if (rb.velocity.y == 0f && !isGrounded())
        {
            state = MovementState.maxHeight;
            animator.SetInteger("State", (int)state);
        }
        else if (rb.velocity.y < 0f && !isGrounded())
        {
            state = MovementState.falling;
            animator.SetInteger("State", (int)state);
        }

        //dash section
        if (isDashing && isAttacking)
        {
            animator.SetBool("dashAttack", true);
        }
        else if (isDashing && rb.velocity.x == 0f && rb.velocity.y > 0f)
        {
            state = MovementState.jumping;
            animator.SetInteger("State", (int)state);
        }
        else if (isDashing && rb.velocity.x == 0f && rb.velocity.y < 0f)
        {
            state = MovementState.falling;
            animator.SetInteger("State", (int)state);
        }
        else if (isDashing && rb.velocity.x != 0f)
        {
            animator.SetBool("isDashing", true);
        }
        else if (!isDashing)
        {
            animator.SetBool("isDashing", false);
            animator.SetBool("dashAttack", false);
        }
        //attack section
        if (isAttacking && !inSecondAttack)
        {
            state = MovementState.attacking;
            animator.SetInteger("State", (int)state);
        }
        else if (isAttacking && inSecondAttack)
        {
            animator.SetTrigger("Second_Attack");
        }
        else if(!isAttacking && !inSecondAttack)
        {
            animator.ResetTrigger("Second_Attack");
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy")//tag enemy or trap its dead
        {
          Die(collision.gameObject);
        }
    }

    public void Die(GameObject killer)//get which game object killed the player
    {
        Debug.Log("I'm dead by: "+killer.name);
        //load a new scene for each gameobject (unless it's the boss)
        //myRigidBody.bodyType = RigidbodyType2D.Static; -> don't let the player move (it's dead)
        //animation.SetTrigger("Death"); -> go to death animation
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name
    }
}
