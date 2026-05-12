using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public enum PlayerState
    {
        Grounded,
        Moving,
        Jumping,
        Dashing
    }

    [Header("State")]
    public bool inputEnabled = false;


    [Header("INPUT (Rebindable)")]
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode dashKey = KeyCode.S;
    public KeyCode shootKey = KeyCode.Q;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float airControl = 0.7f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 0.6f;

    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public LayerMask groundLayer;

    private bool isDashing;
    private float dashTimer;
    private float dashDirection;
    private float dashCooldownTimer;

    [Header("Jump Settings")]
    public float jumpForce = 14f;
    public int maxJumps = 2;

    private int jumpCount;

    public float jumpHoldBoost = 25f;

    [Range(0.1f, 1f)]
    public float shortHopMultiplier = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;

    [Header("Physics")]
    public Rigidbody2D rb;

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public PlayerDebugText debugText;
    public Animator animator;
    public PlayerMovementAudio movementAudio;  // ADD THIS

    [Header("Weapon")]
    public Transform firePoint;
    public Transform weaponHolder;

    private Vector3 firePointDefaultLocalPos;
    private Vector3 weaponHolderDefaultLocalPos;

    private SpriteRenderer[] childSprites;


    [HideInInspector]
    public bool facingRight = true;

    private float moveInput;
    private bool isGrounded;

    private PlayerCombat combat;

    public PlayerState currentState;

    void Start()
    {
        combat = GetComponent<PlayerCombat>();

        // ADD THIS - Auto-find if not assigned
        if (movementAudio == null)
            movementAudio = GetComponent<PlayerMovementAudio>();

        // Cache all sprite renderers
        childSprites =
            GetComponentsInChildren<SpriteRenderer>(
                includeInactive: false
            );

        // Cache original positions
        if (firePoint != null)
        {
            firePointDefaultLocalPos =
                firePoint.localPosition;
        }

        if (weaponHolder != null)
        {
            weaponHolderDefaultLocalPos =
                weaponHolder.localPosition;
        }
    }

    void Update()
    {
        if (!inputEnabled)
            return;

        HandleInput();
        HandleState();
        Flip();
    }

    void FixedUpdate()
    {
        if (!inputEnabled)
            return;

        ApplyMovement();

        HandleDash();

        HandleJumpHold();

        ReduceStickiness();

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.fixedDeltaTime;
    }

    void HandleInput()
    {
        moveInput = 0;

        if (Input.GetKey(moveLeft))
            moveInput = -1;

        if (Input.GetKey(moveRight))
            moveInput = 1;

        isGrounded = Physics2D.Raycast(
            groundCheck.position,
            Vector2.down,
            0.15f,
            groundLayer
        );

        if (isGrounded)
            jumpCount = 0;

        // Jump
        if (Input.GetKeyDown(jumpKey))
        {
            if (isGrounded || jumpCount < maxJumps)
            {
                rb.linearVelocity =
                    new Vector2(
                        rb.linearVelocity.x,
                        jumpForce
                    );

                jumpCount++;

                if (animator != null)
                {
                    animator.ResetTrigger("Jump");
                    animator.SetTrigger("Jump");
                }

                // ADD THIS - Trigger jump sound
                if (movementAudio != null)
                {
                    movementAudio.PlayJumpSound();
                }
            }
        }

        // Short hop
        if (Input.GetKeyUp(jumpKey) &&
            rb.linearVelocity.y > 0)
        {
            rb.linearVelocity =
                new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y *
                    shortHopMultiplier
                );
        }

        // Dash
        if (Input.GetKeyDown(dashKey) &&
            !isDashing &&
            dashCooldownTimer <= 0f)
        {
            StartDash();

            dashCooldownTimer = dashCooldown;
        }

        // Shoot
        if (combat != null)
        {
            if (Input.GetKey(shootKey))
                combat.TryShoot(held: true);

            if (Input.GetKeyDown(shootKey))
                combat.TryShoot(held: false);
        }
    }

    void StartDash()
    {
        isDashing = true;

        dashTimer = dashDuration;

        dashDirection =
            facingRight
            ? 1f
            : -1f;
    }

    void HandleDash()
    {
        if (!isDashing)
            return;

        dashTimer -= Time.fixedDeltaTime;

        RaycastHit2D hit =
            Physics2D.Raycast(
                wallCheck.position,
                Vector2.right * dashDirection,
                wallCheckDistance,
                groundLayer
            );

        if (hit.collider != null)
        {
            EndDash();
            return;
        }

        rb.linearVelocity =
            new Vector2(
                dashDirection * dashSpeed,
                0
            );

        if (dashTimer <= 0f)
            EndDash();
    }

    void EndDash()
    {
        isDashing = false;
    }

    void ApplyMovement()
    {
        if (isDashing)
            return;

        float control =
            isGrounded
            ? 1f
            : airControl;

        float targetSpeed =
            moveInput * moveSpeed * control;

        rb.linearVelocity = new Vector2(
            Mathf.Lerp(
                rb.linearVelocity.x,
                targetSpeed,
                12f * Time.fixedDeltaTime
            ),
            rb.linearVelocity.y
        );
    }

    void HandleJumpHold()
    {
        if (Input.GetKey(jumpKey) &&
            rb.linearVelocity.y > 0)
        {
            rb.linearVelocity +=
                Vector2.up *
                jumpHoldBoost *
                Time.fixedDeltaTime;
        }
    }

    void ReduceStickiness()
    {
        if (isGrounded)
            return;

        if (Mathf.Abs(rb.linearVelocity.x) < 0.01f)
            return;

        RaycastHit2D leftHit =
            Physics2D.Raycast(
                transform.position,
                Vector2.left,
                0.4f,
                groundLayer
            );

        RaycastHit2D rightHit =
            Physics2D.Raycast(
                transform.position,
                Vector2.right,
                0.4f,
                groundLayer
            );

        if (leftHit.collider != null &&
            moveInput < 0)
        {
            rb.AddForce(
                Vector2.right * 2f,
                ForceMode2D.Force
            );
        }

        if (rightHit.collider != null &&
            moveInput > 0)
        {
            rb.AddForce(
                Vector2.left * 2f,
                ForceMode2D.Force
            );
        }
    }

    void HandleState()
    {
        if (isDashing)
        {
            currentState =
                PlayerState.Dashing;
        }
        else if (!isGrounded)
        {
            currentState =
                PlayerState.Jumping;
        }
        else if (Mathf.Abs(moveInput) > 0.1f)
        {
            currentState =
                PlayerState.Moving;
        }
        else
        {
            currentState =
                PlayerState.Grounded;
        }

        if (debugText != null)
            debugText.SetState(currentState);
    }

    void Flip()
    {
        // FACE RIGHT
        if (moveInput > 0)
        {
            facingRight = true;

            FlipAllSprites(false);

            // Fire point
            if (firePoint != null)
            {
                Vector3 firePos =
                    firePointDefaultLocalPos;

                firePos.x =
                    Mathf.Abs(firePos.x);

                firePoint.localPosition =
                    firePos;
            }

            // Weapon holder
            if (weaponHolder != null)
            {
                Vector3 weaponPos =
                    weaponHolderDefaultLocalPos;

                weaponPos.x =
                    Mathf.Abs(weaponPos.x);

                weaponHolder.localPosition =
                    weaponPos;
            }
        }

        // FACE LEFT
        else if (moveInput < 0)
        {
            facingRight = false;

            FlipAllSprites(true);

            // Fire point
            if (firePoint != null)
            {
                Vector3 firePos =
                    firePointDefaultLocalPos;

                firePos.x =
                    -Mathf.Abs(firePos.x);

                firePoint.localPosition =
                    firePos;
            }

            // Weapon holder
            if (weaponHolder != null)
            {
                Vector3 weaponPos =
                    weaponHolderDefaultLocalPos;

                weaponPos.x =
                    -Mathf.Abs(weaponPos.x);

                weaponHolder.localPosition =
                    weaponPos;
            }
        }
    }

    void FlipAllSprites(bool flipX)
    {
        // Main sprite
        if (spriteRenderer != null)
            spriteRenderer.flipX = flipX;

        // Child sprites
        FlipChildSprites(transform, flipX);
    }

    void FlipChildSprites(
        Transform parent,
        bool flipX
    )
    {
        foreach (Transform child in parent)
        {
            SpriteRenderer sr =
                child.GetComponent<SpriteRenderer>();

            if (sr != null)
                sr.flipX = flipX;

            // Recursive
            FlipChildSprites(child, flipX);
        }
    }
}