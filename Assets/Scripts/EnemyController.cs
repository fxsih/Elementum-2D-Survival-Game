using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float maxHealth = 10f;
    bool isDead = false;
    public bool IsDead => isDead;
    public float hitStopDuration = 0.05f;
    public GameObject damagePopupPrefab;

    [Header("Combat")]
public float contactDamage = 10f;

    float currentHealth;

    Transform player;
    Rigidbody2D rb;
    Animator animator;

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

   void FixedUpdate()
{
    if (isDead) return;

    if (player == null) return;

    Vector2 dir = (player.position - transform.position).normalized;



    if (animator != null)
        animator.SetBool("IsMoving", dir.sqrMagnitude > 0.01f);
}

public void TakeDamage(float damage, bool applyHitstop = true)
{
    if (isDead) return;

    // 🔥 BLOCK SAME FRAME MULTI HIT
    if (lastHitFrame == Time.frameCount)
        return;

    lastHitFrame = Time.frameCount;

    // ✅ ONLY apply hitstop when explicitly allowed
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

    // 🔥 DEATH
    if (currentHealth <= 0f)
    {
        Die();
        return;
    }

    // 🔥 HURT ANIMATION (optional: disable for dash if you want)
    if (animator != null)
        animator.SetTrigger("Hurt");
}

void Die()
{
    if (isDead) return;

    isDead = true;
    Debug.Log("Enemy died");

    animator.ResetTrigger("Hurt");
    animator.SetTrigger("Die");
    GameManager.Instance.AddKill();

   PlayerController player = FindObjectOfType<PlayerController>();

   if (player.hasLifeSteal)
{
    player.Heal(player.lifeStealAmount);
}

if (player != null)
{
    player.OnEnemyKilled(maxHealth);
}

    // 🔥 STOP PATHFINDING
    AIPath ai = GetComponent<AIPath>();
    if (ai != null)
    {
        ai.canMove = false;
        ai.canSearch = false;
    }

    // OPTIONAL: disable rigidbody movement
    rb.linearVelocity = Vector2.zero;
    rb.bodyType = RigidbodyType2D.Kinematic;

    // disable collider
    GetComponent<Collider2D>().enabled = false;

    // destroy after animation
    DropGems();
    Destroy(gameObject, 1.5f);
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

void DropGems()
{
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
}
