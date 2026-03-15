using UnityEngine;

public class Attack2Projectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 2f;
    public GameObject explosionPrefab;
    public float explosionRadius = 1.5f;
public float explosionForce = 6f;
public LayerMask explosionAffectLayers;

    Rigidbody2D rb;
    Vector2 direction;
    bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (other.isTrigger) return;
        if (other.transform == transform.root) return;

        Hit();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        if (collision.transform == transform.root) return;

        Hit();
    }

    void Hit()
{
    hasHit = true;

    if (rb != null)
        rb.linearVelocity = Vector2.zero;

    ApplyExplosionForce();

    if (explosionPrefab != null)
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

    Destroy(gameObject);
}
    void ApplyExplosionForce()
{
    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionAffectLayers);

    foreach (Collider2D hit in hits)
    {
        Rigidbody2D hitRb = hit.attachedRigidbody;
        if (hitRb == null) continue;

        Vector2 dir = (hitRb.position - (Vector2)transform.position);
        float distance = dir.magnitude;

        if (distance < 0.01f)
            dir = Vector2.up;
        else
            dir /= distance;

        float falloff = 1f - Mathf.Clamp01(distance / explosionRadius);
        hitRb.AddForce(dir * explosionForce * falloff, ForceMode2D.Impulse);
    }
}
}