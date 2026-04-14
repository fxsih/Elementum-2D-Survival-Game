using UnityEngine;

public class WitchProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float damage = 10f;
    public float lifeTime = 4f;

    public GameObject explosionPrefab;
public float explosionRadius = 1.5f;

    Vector2 direction;

    Rigidbody2D rb;
    [Header("Explosion Audio")]
public AudioClip[] explosionSounds;

[Range(0f,1f)]
public float explosionVolume = 0.9f;

public float explosionMinPitch = 0.9f;
public float explosionMaxPitch = 1.1f;

int lastExplosionIndex = -1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        int enemyLayer = LayerMask.NameToLayer("Enemy");
int projectileLayer = gameObject.layer;

// 🔥 Ignore enemy collisions
Physics2D.IgnoreLayerCollision(projectileLayer, enemyLayer, true);
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        if (rb != null)
            rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (other.isTrigger) return;

    PlayerController player = other.GetComponent<PlayerController>();

    if (player != null)
    {
        Vector2 hitDir = (player.transform.position - transform.position).normalized;
        player.TakeDamage(damage, hitDir);
    }

    Explode();
}

public void Explode()
{
    PlayExplosionSound();
    if (explosionPrefab != null)
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

    Destroy(gameObject);
}

void PlayExplosionSound()
{
    if (explosionSounds == null || explosionSounds.Length == 0) return;
    if (AudioManager.Instance == null) return;

    int index;

    do
    {
        index = Random.Range(0, explosionSounds.Length);
    }
    while (index == lastExplosionIndex && explosionSounds.Length > 1);

    lastExplosionIndex = index;

    float pitch = Random.Range(explosionMinPitch, explosionMaxPitch);

    AudioSource source = AudioManager.Instance.sfxSource;

    float originalPitch = source.pitch;
    source.pitch = pitch;

    source.PlayOneShot(explosionSounds[index], explosionVolume);

    source.pitch = originalPitch;
}
}