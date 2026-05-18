using UnityEngine;

public class BotBrain : MonoBehaviour
{
    [Header("Combat")]
    public float preferredDistance = 5f;
    public float shootDistance = 10f;

    [Header("Movement")]
    public float jumpHeightThreshold = 1.5f;

    [Header("Detection")]
    public LayerMask groundLayer;
    public Transform frontCheck;
    public float wallDistance = 0.5f;
    public float ledgeDistance = 1f;

    [Header("Weapons")]
    public float weaponSearchRadius = 25f;

    [Header("Roaming")]
    public float roamRadius = 20f;
    public float roamRefreshTime = 5f;
    public float roamPointReachedDistance = 1.5f;

    [Header("Platform Navigation")]
    public float platformSearchRadius = 12f;
    public float platformSearchHeight = 8f;

    [Header("Direction Lock")]
    public float directionChangeCooldown = 1f;

    [Header("Stuck Prevention")]
    public float stuckTimeThreshold = 2.5f;
    public float stuckMoveDistance = 0.6f;
    public float stuckResetRadius = 10f;

    [Header("Skill")]
    public float aggression = 1f;
    public float jumpChance = 0.08f;
    public float doubleJumpChance = 0.05f;

    private PlayerController2D controller;
    private PlayerCombat combat;
    private Rigidbody2D rb;

    private Transform targetPlayer;
    private Transform targetWeapon;

    private Vector2 roamTarget;
    private bool hasRoamTarget;

    private Vector2 platformTarget;
    private bool hasPlatformTarget;

    private float roamTimer;
    private float directionTimer;
    private float lockedMoveDirection;

    private float jumpHoldTimer;

    private Vector2 lastPos;
    private float stuckTimer;

    private bool usedDoubleJump;

    void Start()
    {
        controller = GetComponent<PlayerController2D>();
        combat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody2D>();

        lockedMoveDirection = Random.value > 0.5f ? 1f : -1f;

        lastPos = transform.position;
        PickNewRoamTarget();
    }

    void Update()
    {
        if (!controller.inputEnabled)
            return;

        ResetInputs();
        FindTargets();
        HandleStuck();

        roamTimer += Time.deltaTime;

        if (NeedsWeapon())
            WeaponBehaviour();
        else
            CombatBehaviour();

        HandleJumpHold();
        HandleDoubleJump();
    }

    // =====================================================
    // WEAPON STATE SYSTEM
    // =====================================================

    bool NeedsWeapon()
    {
        if (combat.currentWeapon == null)
            return true;

        if (combat.currentWeapon.weaponName == "Melee")
            return true;

        return false;
    }

    bool HasRangedWeapon()
    {
        return combat.currentWeapon != null &&
               combat.currentWeapon.weaponName != "Melee";
    }

    // =====================================================
    // INPUT RESET
    // =====================================================

    void ResetInputs()
    {
        controller.externalMoveInput = 0;
        controller.externalJump = false;
        controller.externalJumpHeld = false;
        controller.externalShoot = false;
        controller.externalShootHeld = false;
        controller.externalDash = false;
    }

    // =====================================================
    // TARGETING
    // =====================================================

    void FindTargets()
    {
        FindClosestPlayer();
        FindClosestWeapon();
    }

    void FindClosestPlayer()
    {
        float best = Mathf.Infinity;
        targetPlayer = null;

        foreach (PlayerHealth p in GameManager.Instance.GetActivePlayers())
        {
            if (p == null || p.gameObject == gameObject || !p.gameObject.activeSelf)
                continue;

            float d = Vector2.Distance(transform.position, p.transform.position);

            if (d < best)
            {
                best = d;
                targetPlayer = p.transform;
            }
        }
    }

    void FindClosestWeapon()
    {
        WeaponPickup[] pickups =
            FindObjectsByType<WeaponPickup>(FindObjectsSortMode.None);

        float best = Mathf.Infinity;
        targetWeapon = null;

        foreach (var w in pickups)
        {
            float d = Vector2.Distance(transform.position, w.transform.position);

            if (d < best && d <= weaponSearchRadius)
            {
                best = d;
                targetWeapon = w.transform;
            }
        }
    }

    // =====================================================
    // WEAPON MODE (HIGH PRIORITY SURVIVAL)
    // =====================================================

    void WeaponBehaviour()
    {
        if (targetWeapon == null)
        {
            Roam();
            return;
        }

        Vector2 dir = targetWeapon.position - transform.position;

        MoveSmart(dir);

        if (dir.y > jumpHeightThreshold)
            TriggerJump();
    }

    // =====================================================
    // COMBAT MODE (RISK AWARE)
    // =====================================================

    void CombatBehaviour()
    {
        if (targetPlayer == null)
        {
            Roam();
            return;
        }

        Vector2 dir = targetPlayer.position - transform.position;

        float vertical = Mathf.Abs(dir.y);

        if (vertical <= jumpHeightThreshold)
        {
            Fight(dir);
            return;
        }

        Navigate(dir);
    }

    void Fight(Vector2 dir)
    {
        float dx = Mathf.Abs(dir.x);

        bool weakWeapon = NeedsWeapon();     // melee or none
        bool strongWeapon = HasRangedWeapon();

        float move = 0;

        // =====================================================
        // DEFENSIVE LOGIC (WEAK BOT)
        // =====================================================
        if (weakWeapon)
        {
            // RUN AWAY unless very close
            if (dx < preferredDistance)
                move = -Mathf.Sign(dir.x);
            else
                move = 0;
        }
        // =====================================================
        // AGGRESSIVE LOGIC (STRONG BOT)
        // =====================================================
        else
        {
            if (dx > preferredDistance)
                move = Mathf.Sign(dir.x);
            else if (dx < preferredDistance * 0.7f)
                move = -Mathf.Sign(dir.x);
        }

        MoveSmart(new Vector2(move, 0));

        bool facing =
            (dir.x > 0 && controller.facingRight) ||
            (dir.x < 0 && !controller.facingRight);

        if (!weakWeapon && dx <= shootDistance && facing)
        {
            controller.externalShoot = true;
            controller.externalShootHeld = true;
        }

        // dash only when strong OR escaping
        if (!weakWeapon && Random.value < 0.01f * aggression)
            controller.externalDash = true;
    }

    // =====================================================
    // NAVIGATION
    // =====================================================

    void Navigate(Vector2 dir)
    {
        if (hasPlatformTarget)
        {
            Vector2 to = platformTarget - (Vector2)transform.position;

            MoveSmart(to);

            if (to.y > 0.5f)
                TriggerJump();

            if (Vector2.Distance(transform.position, platformTarget) < 2f)
                hasPlatformTarget = false;

            return;
        }

        if (FindPlatform())
            return;

        Roam();
    }

    bool FindPlatform()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(transform.position, platformSearchRadius, groundLayer);

        float best = -999f;
        bool found = false;

        foreach (var h in hits)
        {
            Bounds b = h.bounds;
            Vector2 top = new Vector2(b.center.x, b.max.y);

            if (top.y <= transform.position.y + 1f)
                continue;

            if (top.y > transform.position.y + platformSearchHeight)
                continue;

            float score = top.y;

            if (score > best)
            {
                best = score;
                platformTarget = top;
                found = true;
            }
        }

        hasPlatformTarget = found;
        return found;
    }

    // =====================================================
    // ROAM
    // =====================================================

    void Roam()
    {
        if (!hasRoamTarget || roamTimer > roamRefreshTime)
            PickNewRoamTarget();

        Vector2 dir = roamTarget - (Vector2)transform.position;

        MoveSmart(dir);

        if (Vector2.Distance(transform.position, roamTarget) < roamPointReachedDistance)
            PickNewRoamTarget();
    }

    void PickNewRoamTarget()
    {
        roamTimer = 0;
        hasRoamTarget = true;

        for (int i = 0; i < 20; i++)
        {
            Vector2 pos =
                (Vector2)transform.position + Random.insideUnitCircle * roamRadius;

            RaycastHit2D hit =
                Physics2D.Raycast(pos + Vector2.up * 6f, Vector2.down, 12f, groundLayer);

            if (hit.collider != null)
            {
                roamTarget = hit.point;
                return;
            }
        }

        roamTarget = transform.position;
    }

    // =====================================================
    // MOVEMENT CORE
    // =====================================================

    void MoveSmart(Vector2 dir)
    {
        directionTimer -= Time.deltaTime;

        float desired = Mathf.Sign(dir.x);

        if (directionTimer <= 0f && Mathf.Abs(dir.x) > 1f)
        {
            lockedMoveDirection = desired;
            directionTimer = directionChangeCooldown;
        }

        controller.externalMoveInput = lockedMoveDirection;

        if (WallAhead())
        {
            TriggerJump();
            lockedMoveDirection *= -1f;
        }

        if (LedgeAhead())
        {
            if (Random.value < 0.3f)
                TriggerJump();
        }
    }

    bool WallAhead()
    {
        return Physics2D.Raycast(frontCheck.position,
            Vector2.right * lockedMoveDirection,
            wallDistance,
            groundLayer);
    }

    bool LedgeAhead()
    {
        Vector2 origin = (Vector2)frontCheck.position +
                         Vector2.right * lockedMoveDirection * 0.5f;

        return !Physics2D.Raycast(origin, Vector2.down, ledgeDistance, groundLayer);
    }

    // =====================================================
    // JUMPING
    // =====================================================

    void TriggerJump()
    {
        controller.externalJump = true;
        controller.externalJumpHeld = true;
        jumpHoldTimer = 0.25f;
    }

    void HandleJumpHold()
    {
        if (jumpHoldTimer > 0)
        {
            jumpHoldTimer -= Time.deltaTime;
            controller.externalJumpHeld = true;
        }
    }

    void HandleDoubleJump()
    {
        if (controller.currentState == PlayerController2D.PlayerState.Grounded)
            usedDoubleJump = false;

        if (usedDoubleJump)
            return;

        if (rb.linearVelocity.y < 2f &&
            controller.currentState == PlayerController2D.PlayerState.Jumping)
        {
            if (Random.value < doubleJumpChance)
            {
                TriggerJump();
                usedDoubleJump = true;
            }
        }
    }

    // =====================================================
    // STUCK PREVENTION
    // =====================================================

    void HandleStuck()
    {
        float moved = Vector2.Distance(transform.position, lastPos);

        if (moved < stuckMoveDistance)
            stuckTimer += Time.deltaTime;
        else
        {
            stuckTimer = 0;
            lastPos = transform.position;
        }

        if (stuckTimer > stuckTimeThreshold)
        {
            stuckTimer = 0;

            hasPlatformTarget = false;
            hasRoamTarget = false;

            lockedMoveDirection = Random.value > 0.5f ? 1f : -1f;

            roamTarget =
                (Vector2)transform.position +
                Random.insideUnitCircle.normalized * stuckResetRadius;

            TriggerJump();
        }
    }
}