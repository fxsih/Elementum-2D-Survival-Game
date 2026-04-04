using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{

    [Header("Upgrade Limits")]
public float maxMoveSpeed = 8f;
public float maxSlashSpeed = 0.6f;
public float maxFireballSpeed = 0.8f;

public float maxDashDuration = 0.45f;

    [Header("Movement")]
    public float moveSpeed = 6f;

    [Header("Health")]
public float maxHealth = 100f;
float currentHealth;
bool isDead = false;

[Header("Hurt Settings")]
public float knockbackForce = 3f;
public float knockbackDuration = 0.2f;

public HealthBarUI healthUI;


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
public LayerMask dashHitLayers;
public float dashRadius = 0.6f;
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

    [Header("Life steal")]
    public bool hasLifeSteal = false;
    public float lifeStealAmount = 0f;

    [Header("Gem Multiplier")]
    public bool hasGemMultiplier = false;
    public float gemMultiplier = 1f;

[Header("Base Stats")]

// Attack 1 (Slash)
public float baseSlashDamage = 5f;
public float baseSlashSpeed = 1f;


// Attack 2 (Fireball)
public float baseFireballDamage = 8f;
public float baseFireballSpeed = 1f;

// Dash
public float baseDashDamage = 4f;
public float baseDashDuration = 0.18f;


[Header("Upgrade Bonuses")]

// Attack 1
float bonusSlashDamage = 0f;
float bonusSlashSpeed = 0f;

// Attack 2
float bonusFireballDamage = 0f;
float bonusFireballSpeed = 0f;


// Dash
float bonusDashDamage = 0f;
float bonusDashDuration = 0f;


// Utility (these can stay direct)
public float baseLifeSteal = 2f; // heal per kill
public bool hasAura = false;

public float auraDamage = 1f;     // damage per tick
public float auraRadius = 1.5f;   // range
public float auraTickRate = 1f;   // per second
float auraTimer = 0f;

    Rigidbody2D rb;
    Collider2D playerCollider;
    SpriteRenderer sprite;
    Animator animator;

    public static PlayerController Instance;

    bool lastUsedSlash = true;
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

    Dictionary<EnemyController, float> enemyHitCooldown = new Dictionary<EnemyController, float>();
public float enemyDamageCooldown = 0.5f;

public TMP_Text healthText; // drag in inspector

    void Awake()
    {
        if (Instance == null)
        Instance = this;
    else
        Destroy(gameObject);
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
        UpdateHealthUI();
    }

    void OnEnable()
    {
        input = new PlayerInputActions();
        input.Gameplay.Enable();

        input.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Gameplay.Move.canceled += _ => moveInput = Vector2.zero;

        input.Gameplay.MousePosition.performed += ctx => mouseScreenPos = ctx.ReadValue<Vector2>();

        input.Gameplay.Jump.performed += _ =>
{
    QueueJump();
};
        input.Gameplay.Dash.performed += _ => TryStartDash();

       input.Gameplay.Attack1.performed += _ =>
{
    if (isDashing)
        EndDash(); // 🔥 cancel dash

    attack1Held = true;
};
input.Gameplay.Attack1.canceled += _ => attack1Held = false;

input.Gameplay.Attack2.performed += _ =>
{
    if (isDashing)
        EndDash(); // 🔥 cancel dash

    attack2Held = true;
};
input.Gameplay.Attack2.canceled += _ => attack2Held = false;
    }

    void OnDisable()
    {
        input.Gameplay.Disable();
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    }

  void FixedUpdate()
{
    // 🔥 KNOCKBACK
    if (knockbackTimer > 0f)
    {
        Vector2 move = knockbackDirection * knockbackForce * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            knockbackDirection,
            move.magnitude,
            LayerMask.GetMask("Wall")
        );

        if (!hit)
        {
            rb.MovePosition(rb.position + move);
        }

        return;
    }

    // 🔥 DASH (FULLY CONTROLLED HERE)
    if (isDashing)
    {
        dashTimer += Time.fixedDeltaTime;
        dashDamageTimer -= Time.fixedDeltaTime;

        if (dashDamageTimer <= 0f)
        {
            DealDashDamage();
            dashDamageTimer = dashDamageInterval;
        }

        float finalDashDuration = Mathf.Min(baseDashDuration + bonusDashDuration, 0.45f);

        if (dashTimer >= finalDashDuration)
        {
            EndDash();
            return;
        }

        rb.linearVelocity = dashDirection * dashSpeed;
        return; // 🔥 BLOCK EVERYTHING ELSE
    }

    // 🔥 NORMAL MOVEMENT
    if (isJumping)
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
        rb.linearVelocity = moveInput * GetMoveSpeed();
    }

    // 🔥 ATTACK LOCK
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
{
    float slashSpeed = Mathf.Min(baseSlashSpeed + bonusSlashSpeed, 0.6f);
    float fireballSpeed = Mathf.Min(baseFireballSpeed + bonusFireballSpeed, 0.8f);

    slashSpeed = Mathf.Min(slashSpeed, 3f);
    fireballSpeed = Mathf.Min(fireballSpeed, 3f);

    // 🔥 IGNORE INPUT WHILE DASHING
    if (isDashing)
    {
        attackCooldownTimer -= Time.deltaTime;
        return;
    }

    if (lastUsedSlash)
        attackCooldownTimer -= Time.deltaTime * GetFinalSlashSpeed();
    else
        attackCooldownTimer -= Time.deltaTime * GetFinalFireballSpeed();
}
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
    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
    rb.linearVelocity = Vector2.zero; // ✅ ADD THIS
}
}
        UpdateFacing();
        UpdateAnimation();
        UpdateJump();
        UpdateShadowOffset();
        UpdateRunDust();
        UpdateDashCooldown();
        UpdateAttack();
        HandleAttackHold();
        HandleAuraDamage();
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


void EndDash()
{
    isDashing = false;

    // 🔥 HARD STOP
    rb.linearVelocity = Vector2.zero;

    if (playerLayer != -1 && enemyLayer != -1)
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);

    visual.gameObject.SetActive(true);

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

        float finalDashDuration = Mathf.Min(baseDashDuration + bonusDashDuration, 0.45f);
Destroy(fx, finalDashDuration);
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
    lastUsedSlash = true;
    isAttacking = true;
    attackTimer = attack1Duration;
    attackCooldownTimer = attack1Delay;
    animator.SetTrigger("Attack1");
}

void StartAttack2()
{
    lastUsedSlash = false;
    isAttacking = true;
    attackTimer = attack2Duration;
    attackCooldownTimer = attack2Delay;
    animator.SetTrigger("Attack2");
}
void UpdateAttack()
{
    if (!isAttacking) return;

    float slashSpeed = baseSlashSpeed + bonusSlashSpeed;
float fireballSpeed = baseFireballSpeed + bonusFireballSpeed;

// Decide which attack is active
if (attack1Held)
    attackCooldownTimer -= Time.deltaTime * slashSpeed;
else if (attack2Held)
    attackCooldownTimer -= Time.deltaTime * fireballSpeed;
else
    attackCooldownTimer -= Time.deltaTime;

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
    // 🔥 CLOSE RANGE ALWAYS HIT
    if (distance < minDistanceAlwaysHit)
    {
       enemy.TakeDamage(baseSlashDamage + bonusSlashDamage);

        Vector2 pushDir = (toTarget + dir).normalized;
        hitRb.AddForce(pushDir * attack1Force, ForceMode2D.Impulse);
        continue;
    }

    toTarget.Normalize();
    float hitAngle = Vector2.Angle(dir, toTarget);

    if (hitAngle <= attackAngle * 0.5f)
    {
       enemy.TakeDamage(baseSlashDamage + bonusSlashDamage);// ✅ MOVED HERE

        Vector2 pushDir = (toTarget + dir * 0.5f).normalized;
        hitRb.AddForce(pushDir * attack1Force, ForceMode2D.Impulse);
    }
}}

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
projectile.SetDamage(baseFireballDamage + bonusFireballDamage);// 🔥 THIS LINE FIXES IT
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
    if (isDashing) return;
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

       enemy.TakeDamage(baseDashDamage + bonusDashDamage, false);
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

    UpdateHealthUI(); // ✅ UPDATED

    invulnTimer = invulnTime;  

    knockbackDirection = hitDirection.normalized;  
    knockbackTimer = knockbackDuration;  

    Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);  

    if (animator != null)  
        animator.SetTrigger("Hurt");  

    HitStop.Instance?.DoHitStop(0.03f);  

    if (currentHealth <= 0f)  
    {  
        Die();  
    }  
}

public void TryTakeDamageFromEnemy(EnemyController enemy, float damage, Vector2 hitDir)
{
    if (isDead) return;

    if (enemyHitCooldown.ContainsKey(enemy))
    {
        if (Time.time < enemyHitCooldown[enemy])
            return;
    }

    enemyHitCooldown[enemy] = Time.time + enemyDamageCooldown;

    TakeDamage(damage, hitDir);
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
    currentHealth = Mathf.Min(currentHealth, maxHealth);  

    UpdateHealthUI(); // ✅ UPDATED
}
public void ApplyUpgrade(UpgradeData upgrade)
{
    switch (upgrade.type)
    {
        case UpgradeType.Speed:
            moveSpeed += upgrade.value;
            moveSpeed = Mathf.Min(moveSpeed, maxMoveSpeed);
            break;

        case UpgradeType.SlashDamage:
            bonusSlashDamage += upgrade.value;
            break;

        case UpgradeType.FireballDamage:
            bonusFireballDamage += upgrade.value;
            break;

        case UpgradeType.DashDamage:
            bonusDashDamage += upgrade.value;
            break;

        case UpgradeType.DashDuration:
            bonusDashDuration += upgrade.value;
            break;

        case UpgradeType.Health:
            maxHealth += upgrade.value;
            currentHealth = Mathf.Min(currentHealth + upgrade.value, maxHealth);
            UpdateHealthUI(); // ✅ ADD THIS
            break;

        case UpgradeType.LifeSteal:
    hasLifeSteal = true;
    lifeStealAmount = baseLifeSteal;
    break;

       case UpgradeType.GemMultiplier:
    hasGemMultiplier = true;
    gemMultiplier = 2f;
    break;

        case UpgradeType.AuraDamage:
            auraDamage += upgrade.value;
            break;

        case UpgradeType.SlashSpeed:
            bonusSlashSpeed += upgrade.value;
            break;

        case UpgradeType.FireballSpeed:
            bonusFireballSpeed += upgrade.value;
            break;
    }

    Debug.Log("Upgrade Applied: " + upgrade.upgradeName);
}

void UpdateHealthUI()
{
    if (healthUI != null)
        healthUI.UpdateHealth(currentHealth, maxHealth);

    if (healthText != null)
        healthText.text = Mathf.CeilToInt(currentHealth) + "/" + Mathf.CeilToInt(maxHealth);
}
public void OnEnemyKilled(float enemyMaxHealth)
{
    if (hasLifeSteal) return;

    float healAmount = enemyMaxHealth * lifeStealAmount;

    Heal(healAmount);
}

void HandleAuraDamage()
{
    if (!hasAura) return;

    auraTimer += Time.deltaTime;

    if (auraTimer < auraTickRate) return; // 🔥 use your variable

    auraTimer = 0f;

    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, auraRadius);

    foreach (Collider2D hit in hits)
    {
        EnemyController enemy = hit.GetComponent<EnemyController>();

        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(auraDamage, false); // ❌ no hitstop
        }
    }
}

public float GetFinalSlashSpeed()
{
    return Mathf.Min(baseSlashSpeed + bonusSlashSpeed, maxSlashSpeed);
}

public float GetFinalFireballSpeed()
{
    return Mathf.Min(baseFireballSpeed + bonusFireballSpeed, maxFireballSpeed);
}

public float GetFinalDashDuration()
{
    return Mathf.Min(baseDashDuration + bonusDashDuration, maxDashDuration);
}

public float GetMoveSpeed()
{
    return Mathf.Min(moveSpeed, maxMoveSpeed);
}
}