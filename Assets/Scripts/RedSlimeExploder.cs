using UnityEngine;
using System.Collections;

public class RedSlimeExploder : MonoBehaviour
{
    public float explodeRange = 1.2f;
    public float explodeDamage = 25f;
    public float explodeRadius = 2f;
    public LayerMask explosionLayers;
    public GameObject explosionFx;
        bool isExploding = false;

        bool hasExploded = false;

    [Header("Flash")]
    public int flashCount = 4;
    public float flashSpeed = 0.08f;

    Transform player;
    EnemyController enemy;
    SpriteRenderer sr;
    Animator anim;

    [Header("Death Audio")]
public AudioClip[] deathSounds;

[Range(0f,1f)]
public float deathVolume = 1f;

[Header("Pitch Variation")]
public float deathMinPitch = 0.85f;
public float deathMaxPitch = 1.05f;

int lastDeathIndex = -1;
bool hasPlayedDeath = false;

   void Start()
{
    enemy = GetComponent<EnemyController>();
    sr = GetComponent<SpriteRenderer>();
    anim = GetComponent<Animator>();

    if (enemy != null)
        enemy.OnDeath += HandleDeathExplosion;

    GameObject p = GameObject.FindGameObjectWithTag("Player");
    if (p != null)
        player = p.transform;
}
    void Update()
    {
        if (enemy == null || enemy.IsDead) return;
        if (player == null) return;
        if (isExploding) return;

        float dist = Vector2.Distance(transform.position, player.position);

       if (dist <= explodeRange)
{
    isExploding = true;

    // 🔥 LOCK enemy so it cannot die early
    enemy.isInvulnerable = true;

    StartCoroutine(FlashThenExplode());
}
    }

 IEnumerator FlashThenExplode()
{
    if (anim == null)
    {
        Explode();
        yield break;
    }

    for (int i = 0; i < flashCount; i++)
    {
        anim.ResetTrigger("Hurt");
        anim.SetTrigger("Hurt");

        yield return new WaitForSeconds(flashSpeed);
    }

    // 🔥 ALWAYS explode (no conditions)
    Explode();
}
    void Explode()
    {   
        PlayDeathSound();
        if (enemy == null) return;
        if (hasExploded) return;
        hasExploded = true;

        // 🔥 DAMAGE FIRST
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explodeRadius,
            explosionLayers
        );

        foreach (Collider2D hit in hits)
        {
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null)
            {
                Vector2 dir = (player.transform.position - transform.position).normalized;
                player.TryTakeDamageFromEnemy(enemy, explodeDamage, dir);
            }

            EnemyController other = hit.GetComponent<EnemyController>();
            if (other != null && other != enemy && !other.IsDead)
            {
                other.TakeDamage(explodeDamage, false);
            }
        }

        // 🔥 SPAWN FX
        if (explosionFx != null)
        {
            Instantiate(explosionFx, transform.position, Quaternion.identity);
        }

        // 🔥 INSTANT CLEAN REMOVE (FIXES LAST FRAME)
        gameObject.SetActive(false); // 🔥 KEY FIX
        enemy.DropGems();
        Destroy(gameObject, 0.05f); // slight delay for safety
    }

    void HandleDeathExplosion(EnemyController e)
{
    if (hasExploded) return;

    Explode();
}

void OnDestroy()
{
    if (enemy != null)
        enemy.OnDeath -= HandleDeathExplosion;
}

void PlayDeathSound()
{
    if (hasPlayedDeath) return;
    if (deathSounds == null || deathSounds.Length == 0) return;
    if (AudioManager.Instance == null) return;

    hasPlayedDeath = true;

    int index;

    do
    {
        index = Random.Range(0, deathSounds.Length);
    }
    while (index == lastDeathIndex && deathSounds.Length > 1);

    lastDeathIndex = index;

    float pitch = Random.Range(deathMinPitch, deathMaxPitch);

    AudioSource source = AudioManager.Instance.sfxSource;

    float originalPitch = source.pitch;
    source.pitch = pitch;

    source.PlayOneShot(deathSounds[index], deathVolume);

    source.pitch = originalPitch;
}
}