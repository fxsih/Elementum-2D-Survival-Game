using UnityEngine;
using System.Collections;

public class GemPickup : MonoBehaviour
{
    public int amount = 1;

    [Header("Bounce")]
    public float bounceHeight = 0.25f;
    public float bounceDuration = 0.25f;
    public float scatterDistance = 0.4f;

    [Header("Magnet")]
    public float magnetRange = 2.5f;
    public float magnetSpeed = 12f;
    public float magnetDelay = 0.25f;

    Vector3 startPos;
    Vector3 targetOffset;
    Transform player;

    bool canMagnet = false;
    bool isMovingToPlayer = false;
    bool isBouncing = false; // 🔥 NEW (prevents conflict)

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(EnableMagnetAfterDelay());
    }

    public void StartBounce()
    {
        startPos = transform.position;
        targetOffset = (Vector3)(Random.insideUnitCircle * scatterDistance);

        transform.localScale = Vector3.zero;

        StartCoroutine(ScalePop());
        StartCoroutine(Bounce());
    }

    IEnumerator EnableMagnetAfterDelay()
    {
        yield return new WaitForSeconds(magnetDelay);
        canMagnet = true;
    }

    void Update()
    {
        if (player == null || isBouncing) return; // 🔥 wait for bounce to finish

        float distance = Vector2.Distance(transform.position, player.position);

        if (canMagnet && distance < magnetRange)
        {
            isMovingToPlayer = true;
        }

        if (isMovingToPlayer)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                magnetSpeed * Time.deltaTime
            );
        }
    }

    IEnumerator ScalePop()
    {
        float t = 0f;

        while (t < 0.15f)
        {
            t += Time.deltaTime;

            float scale = Mathf.Lerp(0.6f, 1f, t / 0.15f);
            transform.localScale = Vector3.one * scale;

            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    IEnumerator Bounce()
    {
        isBouncing = true;

        float t = 0f;

        while (t < bounceDuration)
        {
            t += Time.deltaTime;

            float progress = t / bounceDuration;
            float height = Mathf.Sin(progress * Mathf.PI) * bounceHeight;

            transform.position =
                startPos +
                targetOffset * progress +
                Vector3.up * height;

            yield return null;
        }

        transform.position = startPos + targetOffset;

        isBouncing = false; // 🔥 allow magnet now
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        PlayerGemInventory inv = other.GetComponent<PlayerGemInventory>();

        if (inv != null)
        {
            inv.AddGems(amount); // ✅ correct
        }

        Destroy(gameObject);
    }
}
}