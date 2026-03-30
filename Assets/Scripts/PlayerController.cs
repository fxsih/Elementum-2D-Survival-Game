using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Health")]
public float maxHealth = 100f;
float currentHealth;
bool isDead = false;

[Header("Hurt Settings")]
public float knockbackForce = 8f;
public float knockbackDuration = 0.2f;


float knockbackTimer;
Vector2 knockbackDirection;

[Header("Damage Settings")]
public float invulnTime = 0.4f;

float invulnTimer = 0f;

public bool IsDead => isDead;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.5f;

    [Header("Dash Damage")]
public float dashDamage = 4f;
public float dashRadius = 0.6f;
public LayerMask dashHitLayers;
// 🔥 DASH OPTIMIZATION
Collider2D[] dashHitsBuffer = new Collider2D[50];
float dashDamageInterval = 0.05f;
float dashDamageTimer;

HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

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

[Header("Attack 1 VFX")]
public GameObject attack1VfxPrefab;
public Vector2 attack1VfxOffsetRight = new Vector2(0.6f, 0.1f);
public Vector2 attack1VfxOffsetLeft = new Vector2(-0.6f, 0.1f);
public Vector3 attack1VfxScale = Vector3.one;
public float attack1Radius = 0.8f;
public float attack1Force = 6f;
public LayerMask attack1AffectLayers;
public float attack1Damage = 5f;
bool attack1Held;


[Header("Attack 1 Arc Settings")]
public float attack1Range = 1.2f;
public float attack1Angle = 110f; // arc width


[Header("Attack 2 VFX")]
public GameObject attack2VfxPrefab;
public Vector2 attack2LeftOffset = new Vector2(-0.2f, 0.15f);
public Vector2 attack2RightOffset = new Vector2(0.2f, 0.15f);
public float attack2ProjectileSpeed = 8f;
public float attack2Radius = 0.8f;
public Vector3 attack2ProjectileScale = Vector3.one;
bool attack2Held;

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
    bool dashHitstopTriggered;

    float dashLockTimer;
    float jumpTimer;
    float dashTimer;
    int playerLayer;
int enemyLayer;
    

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
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        currentHealth = maxHealth;
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

       input.Gameplay.Attack1.performed += _ => attack1Held = true;
input.Gameplay.Attack1.canceled += _ => attack1Held = false;

input.Gameplay.Attack2.performed += _ => attack2Held = true;
input.Gameplay.Attack2.canceled += _ => attack2Held = false;
    }

    void OnDisable()
    {
        input.Gameplay.Disable();
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }

    void FixedUpdate()
    {
        if (knockbackTimer > 0f)
{
    rb.linearVelocity = knockbackDirection * knockbackForce;
    return;
}
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
        if (invulnTimer > 0f)
            {
                invulnTimer -= Time.deltaTime;
                // flashing effect
                sprite.enabled = Mathf.FloorToInt(Time.time * 20f) % 2 == 0;
            }
        else
            {
                sprite.enabled = true;
            }

            if (knockbackTimer > 0f)
{
    knockbackTimer -= Time.deltaTime;

    if (knockbackTimer <= 0f)
    {
        // 🔥 RESTORE COLLISION
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }
}
        UpdateFacing();
        UpdateAnimation();
        UpdateJump();
        UpdateShadowOffset();
        UpdateRunDust();
        UpdateDash();
        UpdateDashCooldown();
        UpdateAttack();
        HandleAttackHold();
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
        dashHitstopTriggered = false;
        dashDamageTimer = 0f;
        dashTimer = 0f;
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        hitEnemies.Clear();

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
    dashDamageTimer -= Time.deltaTime;

if (dashDamageTimer <= 0f)
{
    DealDashDamage();
    dashDamageTimer = dashDamageInterval;
}
    if (dashTimer >= dashDuration)
    {
        EndDash();
    }
}

void EndDash()
{
    isDashing = false;

    // 🔥 RESTORE COLLISION
    if (playerLayer != -1 && enemyLayer != -1)
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

    // 🔥 RESTORE VISUAL
    visual.gameObject.SetActive(true);

    if (!isJumping)
        rb.linearVelocity = Vector2.zero;

    dashLocked = true;
    dashLockTimer = 0f;

    if (jumpQueued)
    {
        jumpQueued = false;
        StartJump();
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

    float attackRange = attack1Range;
    float attackAngle = attack1Angle;

    float minDistanceAlwaysHit = 0.2f; // 🔥 CLOSE RANGE FIX

    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, attack1AffectLayers);

    foreach (Collider2D hit in hits)
    {
        Rigidbody2D hitRb = hit.attachedRigidbody;
        EnemyController enemy = hit.GetComponent<EnemyController>();
if (enemy != null && enemy.IsDead) continue;
        if (hitRb == null) continue;

        Vector2 toTarget = (hitRb.position - (Vector2)transform.position);
        float distance = toTarget.magnitude;

        if (distance > attackRange) continue;
        EnemyController e = hit.GetComponent<EnemyController>();
if (e != null && e.IsDead) continue;
    if (enemy != null)
    {
        enemy.TakeDamage(attack1Damage);
    }

        // 🔥 FIX: always hit very close targets
        if (distance < minDistanceAlwaysHit)
        {
            Vector2 pushDir = (toTarget + dir).normalized;
            hitRb.AddForce(pushDir * attack1Force, ForceMode2D.Impulse);
            continue;
        }

        toTarget.Normalize();

        float hitAngle = Vector2.Angle(dir, toTarget);

        if (hitAngle <= attackAngle * 0.5f)
        {
            Vector2 pushDir = (toTarget + dir * 0.5f).normalized;
            hitRb.AddForce(pushDir * attack1Force, ForceMode2D.Impulse);
        }
    }

    // --- VFX (unchanged) ---
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

void OnDrawGizmosSelected()
{

    Gizmos.color = Color.red;

    Vector2 dir;

if (Application.isPlaying)
    dir = GetMouseDirection();
else
    dir = Vector2.right; // fallback in editor
    Vector3 dir3 = (Vector3)dir;

    Vector3 origin = transform.position;

    float halfAngle = attack1Angle * 0.5f;
    int segments = 20;

    Vector3 prevPoint = origin;

    for (int i = 0; i <= segments; i++)
    {
        float t = (float)i / segments;
        float angle = -halfAngle + (attack1Angle * t);

        Vector3 rotatedDir = Quaternion.Euler(0, 0, angle) * dir3;
        Vector3 point = origin + rotatedDir * attack1Range;

        Gizmos.DrawLine(prevPoint, point);
        prevPoint = point;
    }

    // center line
    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(origin, origin + dir3 * attack1Range);
}
void HandleAttackHold()
{
    if (attack2Held)
    {
        TryAttack2();
    }
    else if (attack1Held)
    {
        TryAttack1();
    }
}

void DealDashDamage()
{
    // 🔥 slightly forward hitbox (feels better)
    Vector2 center = (Vector2)transform.position + dashDirection * 0.4f;

    int hitCount = Physics2D.OverlapCircleNonAlloc(
        center,
        dashRadius,
        dashHitsBuffer,
        dashHitLayers
    );

    for (int i = 0; i < hitCount; i++)
    {
        Collider2D hit = dashHitsBuffer[i];

        EnemyController enemy = hit.GetComponent<EnemyController>();

        if (enemy == null) continue;
        if (enemy.IsDead) continue;
        if (hitEnemies.Contains(enemy)) continue;

       enemy.TakeDamage(dashDamage, false);

        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
if (enemyRb != null)
{
    Vector2 pushDir = (enemy.transform.position - transform.position).normalized;
    enemyRb.AddForce(pushDir * 2f, ForceMode2D.Impulse);
}

        hitEnemies.Add(enemy);
    }
}

public void TakeDamage(float damage, Vector2 hitDirection)
{
    if (isDead) return;
    if (invulnTimer > 0f) return;

    currentHealth -= damage;

    invulnTimer = invulnTime;

    // 🔥 CALCULATE KNOCKBACK DIRECTION
    knockbackDirection = hitDirection.normalized;
    knockbackTimer = knockbackDuration;

    // 🔥 PASS THROUGH ENEMIES
    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);

    if (animator != null)
        animator.SetTrigger("Hurt");

    HitStop.Instance?.DoHitStop(0.03f);

    if (currentHealth <= 0f)
    {
        Die();
    }
}
void Die()
{
    if (isDead) return;

    isDead = true;

    Debug.Log("Player died");

    rb.linearVelocity = Vector2.zero;
    input.Gameplay.Disable();

    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

    if (animator != null)
    {
        animator.ResetTrigger("Hurt");
        animator.SetTrigger("Die");
    }

    // 🔥 ADD THIS
    if (shadow != null)
        shadow.gameObject.SetActive(false);

    playerCollider.enabled = false;

    Invoke(nameof(HandleDeathEnd), 1f);
    StartCoroutine(HideShadowDelayed());
}

void HandleDeathEnd()
{
    GameOverManager.Instance.GameOver();

    gameObject.SetActive(false); // instead of Destroy
}
IEnumerator HideShadowDelayed()
{
    yield return new WaitForSeconds(0.2f); // tweak timing
    if (shadow != null)
        shadow.gameObject.SetActive(false);
}

public float GetCurrentHealth()
{
    return currentHealth;
}

public float GetMaxHealth()
{
    return maxHealth;
}

public void Heal(float amount)
{
    if (isDead) return;

    currentHealth += amount;

    // Clamp so it doesn’t exceed max
    currentHealth = Mathf.Min(currentHealth, maxHealth);
}
}