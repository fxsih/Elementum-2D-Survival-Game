using UnityEngine;
using Pathfinding;

public class WitchShooter : MonoBehaviour
{
    public GameObject projectilePrefab;

    public float stopDistance = 4f;
    public float runDistance = 7f;
    public float shootCooldown = 2f;
    public Transform firePoint;

    [Header("Witch Projectile Audio")]
public AudioClip[] projectileSounds;

[Range(0f,1f)]
public float projectileVolume = 0.7f;

public float projectileMinPitch = 1.0f;
public float projectileMaxPitch = 1.2f;

int lastProjectileIndex = -1;

    bool isAttacking = false;
    bool hasFiredThisAttack = false;

    [Header("Respawn Settings")]
    public float despawnDistance = 20f;
    public float respawnDistance = 12f;

    float shootTimer;

    Transform player;
    EnemyController enemy;
    AIPath ai;
    Animator animator;

    void Start()
    {
        enemy = GetComponent<EnemyController>();
        ai = GetComponent<AIPath>();
        animator = GetComponent<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 🔥 DESPAWN ONLY IF OUTSIDE SCREEN
        if (dist > despawnDistance && IsOutsideScreen())
        {
            RespawnNearPlayer();
            return;
        }

        if (enemy == null || enemy.IsDead) return;

        if (animator != null && animator.GetBool("IsDead")) return;

        shootTimer -= Time.deltaTime;

        Vector2 dirToPlayer = (player.position - transform.position).normalized;

        float buffer = 0.5f;

        // 🔥 TOO CLOSE → RUN AWAY
        if (dist < stopDistance - buffer)
        {
            Vector2 retreatPos = (Vector2)transform.position - dirToPlayer * stopDistance;

            ai.destination = retreatPos;
            ai.maxSpeed = 5f;
            ai.canMove = true;
        }
        // 🔥 TOO FAR → COME CLOSER
        else if (dist > runDistance + buffer)
        {
            ai.destination = player.position;
            ai.maxSpeed = 4f;
            ai.canMove = true;
        }
        // 🔥 PERFECT RANGE → HOLD DISTANCE
        else
        {
            Vector2 holdPosition = (Vector2)player.position - dirToPlayer * stopDistance;

            ai.destination = holdPosition;
            ai.maxSpeed = 2f;
            ai.canMove = true;
        }

        // 🔥 SHOOT
        if (dist <= stopDistance && shootTimer <= 0f)
        {
            Shoot();
            shootTimer = shootCooldown;
        }

        // 🔥 ANIMATION
        float speed = ai.velocity.magnitude;
        animator.SetFloat("Speed", speed);

        FacePlayer();
    }

    // 🔥 CHECK IF OFF SCREEN
    bool IsOutsideScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;

        Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);

        return (viewportPos.x < 0 || viewportPos.x > 1 ||
                viewportPos.y < 0 || viewportPos.y > 1);
    }

    void Shoot()
    {
        if (isAttacking) return;

        isAttacking = true;
        hasFiredThisAttack = false;

        animator.SetTrigger("Attack");
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void SpawnProjectile()
    {
        if (hasFiredThisAttack) return;
        PlayProjectileSound();

        FireProjectile();
        hasFiredThisAttack = true;
    }

    void FacePlayer()
    {
        if (player == null) return;

        Vector3 scale = transform.localScale;

        if (player.position.x < transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    void FireProjectile()
    {
        if (player == null) return;

        Collider2D playerCol = player.GetComponent<Collider2D>();

        Vector2 targetPos = playerCol != null
            ? playerCol.bounds.center
            : (Vector2)player.position;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        Vector2 dir = (targetPos - (Vector2)spawnPos).normalized;

        spawnPos += (Vector3)(dir * 0.5f);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.GetComponent<WitchProjectile>()?.SetDirection(dir);
    }

    void RespawnNearPlayer()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        int side = Random.Range(0, 4);
        Vector2 offset = Vector2.zero;

        switch (side)
        {
            case 0: // left
                offset = new Vector2(-camWidth - 2f, Random.Range(-camHeight, camHeight));
                break;
            case 1: // right
                offset = new Vector2(camWidth + 2f, Random.Range(-camHeight, camHeight));
                break;
            case 2: // top
                offset = new Vector2(Random.Range(-camWidth, camWidth), camHeight + 2f);
                break;
            case 3: // bottom
                offset = new Vector2(Random.Range(-camWidth, camWidth), -camHeight - 2f);
                break;
        }

        Vector2 desiredPos = (Vector2)player.position + offset;

        Vector2 validPos = GetSafeSpawnPosition(desiredPos);

        transform.position = validPos;

        if (ai != null)
            ai.Teleport(validPos, true);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    Vector2 GetSafeSpawnPosition(Vector2 basePos)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            Vector2 testPos = basePos + randomOffset;

            NNInfo nearest = AstarPath.active.GetNearest(testPos, NNConstraint.Default);

            if (nearest.node != null && nearest.node.Walkable)
                return (Vector2)nearest.position;
        }

        return basePos;
    }

    void PlayProjectileSound()
{
    if (projectileSounds == null || projectileSounds.Length == 0) return;
    if (AudioManager.Instance == null) return;

    int index;

    do
    {
        index = Random.Range(0, projectileSounds.Length);
    }
    while (index == lastProjectileIndex && projectileSounds.Length > 1);

    lastProjectileIndex = index;

    float pitch = Random.Range(projectileMinPitch, projectileMaxPitch);

    AudioSource source = AudioManager.Instance.sfxSource;

    float originalPitch = source.pitch;
    source.pitch = pitch;

    source.PlayOneShot(projectileSounds[index], projectileVolume);

    source.pitch = originalPitch;
}
}