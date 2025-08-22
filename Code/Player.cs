using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Android.Types;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private bool isDead;
    private bool playerUnlocked; //private : je ne le vois pas, public : je le vois dans l'inspecteur Unity

    [Header("Knockback info")]
    [SerializeField] private Vector2 knockbackDir;
    private bool isKnocked;
    private bool canBeKnocked = true;

    [Header("Move info")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedMultiplier;
    private float defaultSpeed;
    [Space]
    [SerializeField] private float milestoneIncreaser;
    private float defaultMilestoneIncrease;
    private float speedMilestone;

    [Header("Jump info")]
    [SerializeField] public float jumpForce;
    [SerializeField] float power;
    [SerializeField] private float doubeJumpForce;
    private bool canDoubleJump;


    [Header("Slide info")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideTime;
    [SerializeField] private float slideCooldown;
    private float slideCooldownCounter;
    private float slideTimeCounter;
    private bool isSliding;

    [Header("Collision info")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float ceillingCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Vector2 wallCheckSize;
    private bool isGrounded;
    private bool wallDetected;
    private bool ceillingDetected;
    [HideInInspector] public bool ledgeDetected;

    [Header("Ledge info")]
    [SerializeField] private Vector2 offset1; //offset for position before climb 
    [SerializeField] private Vector2 offset2; //offset for position AFTER climb

    private Vector2 climbBegunPosition;
    private Vector2 climbOverPosition;

    private bool canGrabLedge = true;
    private bool canClimb;

    private void Awake()
    {

    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        speedMilestone = milestoneIncreaser;
        defaultSpeed = moveSpeed;
        defaultMilestoneIncrease = milestoneIncreaser;
    }


    private void FixedUpdate()
    {

    }
    void Update()
    {
        CheckCollision();
        AnimatorControllers();

        slideTimeCounter -= Time.deltaTime;
        slideCooldownCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.K) && !isDead)
            KnockBack();

        if (Input.GetKeyDown(KeyCode.D) && !isDead)
            StartCoroutine(Die());

        if (isKnocked)
            return;

        if (isDead)
            return;

        if (playerUnlocked)
            SetupMovement();

        if (isGrounded)
            canDoubleJump = true;

        SpeedController();

        CheckForLedge();
        CheckForSlideCancel();
        CheckInput();
    }

    public void Damage()
    {
        if (moveSpeed >= maxSpeed)
            KnockBack();
        else
            StartCoroutine(Die());
    }   

    private IEnumerator Die()
    {
        isDead = true;
        rb.linearVelocity = knockbackDir;
        anim.SetBool("isDead", true);

        yield return new WaitForSeconds(1f);
        rb.linearVelocity = new Vector2(0, 0);
        yield return new WaitForSeconds(1f);
        GameManager.instance.RestartLevel();
    }

    private IEnumerator Invicibility() //Coroutine to handle the invincibility effect after being knocked back
    {
        Color originalColor = sr.color;
        Color darkenColor = new Color(sr.color.r, sr.color.g, sr.color.b, .5f); // Set alpha to 0.5 for a semi-transparent effect

        canBeKnocked = false;
        sr.color = darkenColor;
        yield return new WaitForSeconds(.1f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.1f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.15f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.15f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.25f);

        sr.color = originalColor;
        yield return new WaitForSeconds(.25f);

        sr.color = darkenColor;
        yield return new WaitForSeconds(.3f);

        sr.color = originalColor;
        canBeKnocked = true;
    }

    #region Knockback

    private void KnockBack()
    {
        if (!canBeKnocked)
            return;

        StartCoroutine(Invicibility());
        isKnocked = true;
        rb.linearVelocity = knockbackDir;
    }

    private void CancelKnockback() => isKnocked = false;

    #endregion


    #region SpeedControll
    private void SpeedReset()
    {
        moveSpeed = defaultSpeed; ;
        milestoneIncreaser = defaultMilestoneIncrease;
    }

    private void SpeedController()
    {
        if (moveSpeed == maxSpeed)
            return;


        if (transform.position.x > speedMilestone)
        {

            speedMilestone += milestoneIncreaser;

            moveSpeed *= speedMultiplier;
            milestoneIncreaser *= speedMultiplier;

            if (moveSpeed > maxSpeed)
                moveSpeed = maxSpeed;

        }
    }

    #endregion 


    #region LedgeClimb
    private void CheckForLedge()
    {
        if (ledgeDetected && canGrabLedge)
        {
            canGrabLedge = false;
            rb.gravityScale = 0; // Disable gravity to prevent falling while climbing

            Vector2 ledgePosition = GetComponentInChildren<LedgeDetection>().transform.position;

            climbBegunPosition = ledgePosition + offset1;
            climbOverPosition = ledgePosition + offset2;

            canClimb = true;
        }

        if (canClimb)
            transform.position = climbBegunPosition;
    }

    private void LedgeClimbOver()
    {
        canClimb = false;
        rb.gravityScale = 5;
        transform.position = climbOverPosition;
        //canGrabLedge = true;
        Invoke("AllowLedgeGrab", .2f);
    }

    private void AllowLedgeGrab() => canGrabLedge = true; // "=>" veut dire "return" en une ligne, c'est une syntaxe plus concise pour les méthodes qui retournent une seule valeur ou qui n'ont pas de corps complexe.


    #endregion


    private void CheckForSlideCancel()
    {
        if (slideTimeCounter < 0 && !ceillingDetected)
            isSliding = false;
    }

    private void SetupMovement()
    {
        if (wallDetected)
        {
            SpeedReset();
            return;
        }
        if (isSliding)
            rb.linearVelocity = new Vector2(slideSpeed, rb.linearVelocity.y);
        else 
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
    }


    #region Inputs

    private void JumpButton()
    {
        if (isSliding)
            return;
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        }
        else if (canDoubleJump)
        {
            canDoubleJump = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubeJumpForce);
        }
    }
    private void SlideButton()
    {
        if (rb.linearVelocity.x != 0 && slideCooldownCounter < 0)
        {
            isSliding = true;
            slideTimeCounter = slideTime;
            slideCooldownCounter = slideCooldown;
        }        
    }   
    private void CheckInput()
    {
        if (Input.GetButtonDown("Fire2"))
            playerUnlocked = true;

        if (Input.GetButtonDown("Jump"))
            JumpButton();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            SlideButton();
    }

    #endregion Inputs


    #region Animations
    private void AnimatorControllers()
    {
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetFloat("xVelocity", rb.linearVelocity.x);

        anim.SetBool("canDoubleJump", canDoubleJump);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("canClimb", canClimb);

        if (rb.linearVelocity.y < -20)
            anim.SetBool("canRoll", true);

        anim.SetBool("isKnocked", isKnocked); 
    }

    private void RollAnimFinished() => anim.SetBool("canRoll", false); //This method is called from the animation event in the Roll animation to reset the canRoll boolean in the animator.

    #endregion Animations

    private void CheckCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.BoxCast(wallCheck.position, wallCheckSize, 0, Vector2.zero, 0, whatIsGround);
        ceillingDetected = Physics2D.Raycast(transform.position, Vector2.up, ceillingCheckDistance, whatIsGround); //check for ceiling collision
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y + ceillingCheckDistance));
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}

