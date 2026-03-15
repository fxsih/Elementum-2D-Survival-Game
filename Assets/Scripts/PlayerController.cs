using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.5f;

    [Header("Dash FX")]
    public GameObject dashFxPrefab;

    [Header("Jump (Visual Only)")]
    public float jumpHeight = 0.55f;
    public float jumpDuration = 0.22f;
    public AnimationCurve jumpCurve;


    [Header("Attack")]
public float attack1Duration = 0.35f;
public float attack2Duration = 0.55f;
public bool lockMovementDuringAttack = true;

public float attack1Delay = 0.15f;
public float attack2Delay = 0.25f;
float attackCooldownTimer;

[Header("Attack VFX")]
public GameObject attack1VfxPrefab;
public Vector2 attack1VfxOffsetRight = new Vector2(0.6f, 0.1f);
public Vector2 attack1VfxOffsetLeft = new Vector2(-0.6f, 0.1f);
public Vector3 attack1VfxScale = Vector3.one;
public float attack1Radius = 0.8f;


[Header("Attack 2 VFX")]
public GameObject attack2VfxPrefab;
public Vector2 attack2LeftOffset = new Vector2(-0.2f, 0.15f);
public Vector2 attack2RightOffset = new Vector2(0.2f, 0.15f);
public float attack2ProjectileSpeed = 8f;
public float attack2Radius = 0.8f;
public Vector3 attack2ProjectileScale = Vector3.one;

    [Header("Dash Jump Combo")]
    public float dashJumpForwardBoost = 7f;
    public float dashJumpHeightMultiplier = 1.4f;

    [Header("Air Control")]
    public float airControl = 0.6f;

    [Header("Shadow")]
    public Vector2 shadowMinScale = new Vector2(0.25f, 0.15f);
    public float shadowLeftOffsetX = -0.01f;

    [Header("Run Dust FX")]
    public GameObject runDustPrefab;
    public Vector2 runDustOffsetRight = Vector2.zero;
    public Vector2 runDustOffsetLeft = new Vector2(-0.5f, 0f);

    Rigidbody2D rb;
    Collider2D playerCollider;
    SpriteRenderer sprite;
    Animator animator;

    Transform visual;
    Transform shadow;

    GameObject activeRunDust;

    bool wasRunning;

    PlayerInputActions input;

    Vector2 moveInput;
    Vector2 mouseScreenPos;
    Vector2 dashDirection;

    bool isJumping;
    bool isDashing;
    bool dashLocked;

    float dashLockTimer;
    float jumpTimer;
    float dashTimer;

    bool isAttacking;
float attackTimer;
public bool IsAttacking => isAttacking;

    Vector3 visualBasePos;
    Vector3 shadowBasePos;
    Vector3 shadowBaseScale;

    Vector2 dashJumpMomentum;

    bool jumpQueued;
    float jumpBufferTimer;
    float jumpBufferTime = 0.55f;

    public bool IsJumping => isJumping;
    public bool IsDashing => isDashing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();
        sprite = animator.GetComponent<SpriteRenderer>();

        visual = animator.transform.parent;
        shadow = transform.Find("Shadow");

        visualBasePos = visual.localPosition;
        shadowBasePos = shadow.localPosition;
        shadowBaseScale = shadow.localScale;
    }

    void OnEnable()
    {
        input = new PlayerInputActions();
        input.Gameplay.Enable();

        input.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Gameplay.Move.canceled += _ => moveInput = Vector2.zero;

        input.Gameplay.MousePosition.performed += ctx => mouseScreenPos = ctx.ReadValue<Vector2>();

        input.Gameplay.Jump.performed += _ => QueueJump();
        input.Gameplay.Dash.performed += _ => TryStartDash();

        input.Gameplay.Attack1.performed += _ => TryAttack1();
input.Gameplay.Attack2.performed += _ => TryAttack2();
    }

    void OnDisable()
    {
        input.Gameplay.Disable();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else if (isJumping)
        {
            Vector2 airMove = moveInput * moveSpeed * airControl;

            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity,
                dashJumpMomentum + airMove,
                0.2f
            );
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }

        if (isAttacking && lockMovementDuringAttack)
{
    rb.linearVelocity = Vector2.zero;
    return;
}
    }

    void Update()
    {
        if (jumpQueued)
        {
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer <= 0f)
                jumpQueued = false;
        }
        if (attackCooldownTimer > 0f)
    attackCooldownTimer -= Time.deltaTime;
        UpdateFacing();
        UpdateAnimation();
        UpdateJump();
        UpdateShadowOffset();
        UpdateRunDust();
        UpdateDash();
        UpdateDashCooldown();
        UpdateAttack();
    }

    void TryStartDash()
    {
        if (isDashing) return;
        if (dashLocked) return;
        if (isAttacking) CancelAttack();

        StartDash();
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = 0f;

        if (moveInput.sqrMagnitude > 0.01f)
            dashDirection = moveInput.normalized;
        else
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
            );

            dashDirection = (mouseWorld - transform.position).normalized;
        }

        visual.gameObject.SetActive(false);
        DestroyRunDust();

        SpawnDashFX();
    }

    void UpdateDash()
    {
        if (!isDashing) return;

        dashTimer += Time.deltaTime;

        if (dashTimer >= dashDuration)
        {
            isDashing = false;

            if (!isJumping)
                rb.linearVelocity = Vector2.zero;

            visual.gameObject.SetActive(true);

            dashLocked = true;
            dashLockTimer = 0f;

            if (jumpQueued)
            {
                jumpQueued = false;
                StartJump();
            }
        }
    }

    void UpdateDashCooldown()
    {
        if (!dashLocked) return;

        dashLockTimer += Time.deltaTime;

        if (dashLockTimer >= dashCooldown)
            dashLocked = false;
    }

    void SpawnDashFX()
    {
        if (!dashFxPrefab) return;

        float angle =
            Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg + 180f;

        GameObject fx = Instantiate(
            dashFxPrefab,
            transform.position,
            Quaternion.Euler(0f, 0f, angle),
            transform
        );

        StartCoroutine(UpdateFxSorting(fx));

        Destroy(fx, dashDuration);
    }

    IEnumerator UpdateFxSorting(GameObject fx)
    {
        SpriteRenderer playerRenderer = sprite;

        SpriteRenderer[] srs = fx.GetComponentsInChildren<SpriteRenderer>();
        ParticleSystemRenderer[] ps = fx.GetComponentsInChildren<ParticleSystemRenderer>();

        while (fx != null)
        {
            int order = playerRenderer.sortingOrder - 1;

            foreach (SpriteRenderer r in srs)
            {
                r.sortingLayerName = playerRenderer.sortingLayerName;
                r.sortingOrder = order;
            }

            foreach (ParticleSystemRenderer p in ps)
            {
                p.sortingLayerName = playerRenderer.sortingLayerName;
                p.sortingOrder = order;
            }

            yield return null;
        }
    }

    void QueueJump()
    {
        
        if (!isDashing)
        {
            StartJump();
            return;
        }
        if (isAttacking)
        CancelAttack();

        jumpQueued = true;
        jumpBufferTimer = jumpBufferTime;
    }

    void UpdateFacing()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
        );

        sprite.flipX = mouseWorld.x < transform.position.x;
    }

    void UpdateAnimation()
    {
        animator.SetFloat("Speed", rb.linearVelocity.sqrMagnitude);
    }

    void StartJump()
    {
        
        if (isJumping) return;

        isJumping = true;
        jumpTimer = 0f;

        dashJumpMomentum = rb.linearVelocity;

        if (isDashing)
        {
            dashJumpMomentum = Vector2.Lerp(
                rb.linearVelocity,
                dashDirection * dashJumpForwardBoost,
                0.5f
            );
        }

        animator.SetTrigger("Jump");
    }

    void UpdateJump()
    {
        if (!isJumping) return;

        jumpTimer += Time.deltaTime;

        float t = Mathf.Clamp01(jumpTimer / jumpDuration);

        float heightMultiplier = dashJumpMomentum != Vector2.zero ? dashJumpHeightMultiplier : 1f;

        float h = jumpCurve.Evaluate(t) * heightMultiplier;

        visual.localPosition = visualBasePos + Vector3.up * (h * jumpHeight);

        shadow.localScale = Vector3.Lerp(shadowBaseScale, shadowMinScale, h);

        if (t >= 1f)
        {
            isJumping = false;

            visual.localPosition = visualBasePos;
            shadow.localScale = shadowBaseScale;
        }
    }

    void UpdateShadowOffset()
    {
        shadow.localPosition = shadowBasePos;

        if (sprite.flipX)
        {
            shadow.localPosition = new Vector3(
                shadowBasePos.x + shadowLeftOffsetX,
                shadowBasePos.y,
                shadowBasePos.z
            );
        }
    }

    void UpdateRunDust()
    {
        bool isActuallyMoving =
            rb.linearVelocity.sqrMagnitude > 0.01f &&
            !isJumping &&
            !isDashing;

        if (isActuallyMoving && !wasRunning)
            SpawnRunDust();

        if (!isActuallyMoving && wasRunning)
            DestroyRunDust();

        wasRunning = isActuallyMoving;

        if (!activeRunDust) return;

        Vector2 offset = sprite.flipX ? runDustOffsetLeft : runDustOffsetRight;

        activeRunDust.transform.localPosition = offset;

        SpriteRenderer sr = activeRunDust.GetComponentInChildren<SpriteRenderer>();

        if (sr) sr.flipX = sprite.flipX;
    }

    void SpawnRunDust()
    {
        if (!runDustPrefab || activeRunDust) return;

        activeRunDust = Instantiate(runDustPrefab, transform);

        StartCoroutine(UpdateFxSorting(activeRunDust));
    }

    void DestroyRunDust()
    {
        if (!activeRunDust) return;

        Destroy(activeRunDust);

        activeRunDust = null;
    }

  void TryAttack1()
{
    if (isDashing) return;
    if (attackCooldownTimer > 0f) return;

    if (isAttacking)
        CancelAttack();

    StartAttack1();
}

void TryAttack2()
{
    if (isDashing) return;
    if (attackCooldownTimer > 0f) return;

    if (isAttacking)
        CancelAttack();

    StartAttack2();
}

void StartAttack1()
{
    isAttacking = true;
    attackTimer = attack1Duration;
    attackCooldownTimer = attack1Delay;
    animator.SetTrigger("Attack1");
}

void StartAttack2()
{
    isAttacking = true;
    attackTimer = attack2Duration;
    attackCooldownTimer = attack2Delay;
    animator.SetTrigger("Attack2");
}
void UpdateAttack()
{
    if (!isAttacking) return;

    attackTimer -= Time.deltaTime;

    if (attackTimer <= 0f)
    {
        isAttacking = false;
    }
}

public void SpawnAttack1VFX()
{
    if (attack1VfxPrefab == null) return;

    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
        new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
    );

    Vector2 dir = (Vector2)(mouseWorld - transform.position);

    if (dir.sqrMagnitude < 0.001f)
        dir = Vector2.right;
    else
        dir.Normalize();

    GameObject vfx = Instantiate(attack1VfxPrefab, transform);

    vfx.transform.localPosition = new Vector3(dir.x * attack1Radius, dir.y * attack1Radius, 0f);

    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    vfx.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

    Vector3 finalScale = attack1VfxScale;

    if (dir.x < 0f)
        finalScale.y = -Mathf.Abs(finalScale.y);
    else
        finalScale.y = Mathf.Abs(finalScale.y);

    vfx.transform.localScale = finalScale;
    StartCoroutine(UpdateAttackVfxSorting(vfx));
}
IEnumerator UpdateAttackVfxSorting(GameObject fx)
{
    SpriteRenderer playerRenderer = sprite;
    SpriteRenderer[] srs = fx.GetComponentsInChildren<SpriteRenderer>();
    ParticleSystemRenderer[] ps = fx.GetComponentsInChildren<ParticleSystemRenderer>();

    while (fx != null)
    {
        int order = playerRenderer.sortingOrder + 1;

        foreach (SpriteRenderer r in srs)
        {
            r.sortingLayerName = playerRenderer.sortingLayerName;
            r.sortingOrder = order;
        }

        foreach (ParticleSystemRenderer p in ps)
        {
            p.sortingLayerName = playerRenderer.sortingLayerName;
            p.sortingOrder = order;
        }

        yield return null;
    }
}


public void SpawnAttack2VFX_Left()
{
    SpawnAttack2Projectile(attack2LeftOffset);
}

public void SpawnAttack2VFX_Right()
{
    SpawnAttack2Projectile(attack2RightOffset);
}
void SpawnAttack2Projectile(Vector2 localOffset)
{
    if (attack2VfxPrefab == null) return;

    Vector2 dir = GetMouseDirection();
    Vector3 spawnPosition = transform.position + (Vector3)(dir * attack2Radius);

    GameObject vfx = Instantiate(attack2VfxPrefab, spawnPosition, Quaternion.identity);

Collider2D projectileCol = vfx.GetComponent<Collider2D>();
if (projectileCol != null && playerCollider != null)
{
    Physics2D.IgnoreCollision(projectileCol, playerCollider);
}

    Vector3 finalScale = attack2ProjectileScale;
    vfx.transform.localScale = finalScale;

    Attack2Projectile projectile = vfx.GetComponent<Attack2Projectile>();
    if (projectile != null)
    {
        projectile.speed = attack2ProjectileSpeed;
        projectile.SetDirection(dir);
    }
}
Vector2 GetMouseDirection()
{
    Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
        new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
    );

    Vector2 dir = (mouseWorld - transform.position);
    return dir.normalized;
}
void CancelAttack()
{
    if (!isAttacking) return;

    isAttacking = false;
    attackTimer = 0f;

    animator.ResetTrigger("Attack1");
    animator.ResetTrigger("Attack2");

    animator.Play("Pyra_Idle");
}
}