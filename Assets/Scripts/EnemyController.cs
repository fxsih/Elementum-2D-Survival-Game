using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{

   [Header("Hit Audio")]
public AudioClip[] hitSounds;

[Range(0f,1f)]
public float hitVolume = 0.8f;

[Header("Pitch Variation")]
public float minPitch = 0.9f;
public float maxPitch = 2f;

int lastHitIndex = -1;
    public float moveSpeed = 2f;
    public float maxHealth = 10f;
    bool isDead = false;
    public bool IsDead => isDead;
    public float hitStopDuration = 0.05f;
    public GameObject damagePopupPrefab;

    public System.Action<EnemyController> OnDeath;

    [Header("Combat")]
public float contactDamage = 10f;

    float currentHealth;

    Transform player;
    Rigidbody2D rb;
    Animator animator;
    public static int ActiveEnemies = 0;

public bool isInvulnerable = false;

bool hasDroppedGems = false;

    int lastHitFrame = -1;

    [Header("Loot")]
public GameObject gemPrefab;
public int gemAmount = 1;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void OnEnable()
{
    ActiveEnemies++;
}

void OnDisable()
{
    ActiveEnemies--;
}

   void FixedUpdate()
{
    if (isDead) return;

    if (player == null) return;

   Collider2D playerCol = player.GetComponent<Collider2D>();

Vector2 targetPos = playerCol != null 
    ? playerCol.bounds.center 
    : (Vector2)player.position;

Vector2 dir = (targetPos - (Vector2)transform.position).normalized;



    if (animator != null)
        animator.SetBool("IsMoving", dir.sqrMagnitude > 0.01f);
}

public void TakeDamage(float damage, bool applyHitstop = true)
{
    if (isDead) return;
    if (isInvulnerable) return;
    PlayHitSound();

    // 🔥 BLOCK SAME FRAME MULTI HIT
    if (lastHitFrame == Time.frameCount)
        return;
    lastHitFrame = Time.frameCount;

    // 🔥 HITSTOP
    if (applyHitstop)
    {
        HitStop.Instance?.DoHitStop(hitStopDuration);
    }

    currentHealth -= damage;

    // 🔥 DAMAGE POPUP
    if (damagePopupPrefab != null)
    {
        Vector3 spawnPos = transform.position + new Vector3(0, 1f, 0);
        spawnPos.z = 0;
        GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
        popup.GetComponent<DamagePopup>().Setup((int)damage);
    }

    // 🔥 GET WITCH COMPONENT (IMPORTANT)
    WitchCombat wc = GetComponent<WitchCombat>();

    // 🔥 WITCH HIT EFFECT (flash + knockback)
    if (wc != null)
    {
        Vector2 hitDir = (transform.position - PlayerController.Instance.transform.position).normalized;
        wc.OnHit(hitDir);
    }

    // 🔥 DEATH
    if (currentHealth <= 0f)
    {
        Die();
        return;
    }

    // 🔥 HURT ANIMATION ONLY FOR NON-WITCH ENEMIES
    if (animator != null && wc == null)
    {
        animator.SetTrigger("Hurt");
    }
}

void Die()
{
    if (isDead) return;
    isDead = true;

    Debug.Log("Enemy died");

    animator.ResetTrigger("Hurt");

    // 🔥 CHECK FOR WITCH
    WitchCombat wc = GetComponent<WitchCombat>();

    if (wc != null)
    {
        // 👉 Witch handles its own death
        wc.OnDeathEffects();

        animator.Play("Death", 0, 0f);

        // ❌ DO NOT destroy here
    }
    else
    {
        // 👉 Normal enemies (slimes)
        animator.SetTrigger("Die");
        PlayerStatsManager.AddKill();
        Destroy(gameObject, 1.5f);
    }

    GameManager.Instance.AddKill();

    PlayerController player = PlayerController.Instance;
    if (player != null)
    {
        if (player.hasLifeSteal)
        {
            player.Heal(player.lifeStealAmount);
        }
        player.OnEnemyKilled();
    }

    // 🔥 STOP PATHFINDING
    AIPath ai = GetComponent<AIPath>();
    if (ai != null)
    {
        ai.canMove = false;
        ai.canSearch = false;
    }

    rb.linearVelocity = Vector2.zero;
    rb.bodyType = RigidbodyType2D.Kinematic;

    GetComponent<Collider2D>().enabled = false;

    DropGems();
    OnDeath?.Invoke(this);
}

    Vector2 GetPlayerPosition()
    {
        return player != null ? (Vector2)player.position : rb.position;
    }

void OnTriggerStay2D(Collider2D other)
{
    if (isDead) return;

    PlayerController player = other.GetComponent<PlayerController>();

    if (player != null)
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;
        player.TryTakeDamageFromEnemy(this, contactDamage, direction);
    }
}

public void DropGems()
{

    if (hasDroppedGems) return;
    hasDroppedGems = true;
    PlayerController player = PlayerController.Instance;

    int totalGems = gemAmount;

    // 🔥 multiply number of gems instead of value
    if (player != null && player.hasGemMultiplier)
    {
        totalGems = Mathf.RoundToInt(gemAmount * player.gemMultiplier);
    }

    for (int i = 0; i < totalGems; i++)
    {
        Vector2 offset = Random.insideUnitCircle * 0.2f;

        GameObject gem = Instantiate(
            gemPrefab,
            transform.position + (Vector3)offset,
            Quaternion.identity
        );

        GemPickup pickup = gem.GetComponent<GemPickup>();

        if (pickup != null)
        {
            pickup.amount = 1; // each gem still 1
            pickup.StartBounce();
        }
    }
}

public void ForceKill()
{
    if (isDead) return;

    isDead = true;

    // 🔥 disable animator completely
    if (animator != null)
        animator.enabled = false;

    // 🔥 hide sprite instantly
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    if (sr != null)
        sr.enabled = false;

    // 🔥 stop movement
    rb.linearVelocity = Vector2.zero;
    rb.bodyType = RigidbodyType2D.Kinematic;

    // 🔥 disable collider
    Collider2D col = GetComponent<Collider2D>();
    if (col != null)
        col.enabled = false;

    DropGems();

    Destroy(gameObject);
}

void PlayHitSound()
{
    if (hitSounds == null || hitSounds.Length == 0) return;
    if (AudioManager.Instance == null) return;
    if (AudioManager.Instance.sfxSource == null) return;

    int index;

    // 🔁 avoid same sound twice
    do
    {
        index = Random.Range(0, hitSounds.Length);
    }
    while (index == lastHitIndex && hitSounds.Length > 1);

    lastHitIndex = index;

    // 🎚 random pitch
    float pitch = Random.Range(minPitch, maxPitch);

    // 💡 TEMP pitch change (safe enough for short SFX)
    AudioSource source = AudioManager.Instance.sfxSource;

    float originalPitch = source.pitch;
    source.pitch = pitch;

    source.PlayOneShot(hitSounds[index], hitVolume);

    source.pitch = originalPitch;
}
}
