using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(AIPath))]
public class EnemyUnstuckHandler : MonoBehaviour
{
    [Header("Stuck Settings")]
    public float stuckTimeThreshold = 2f;
    public float movementThreshold = 0.02f;

    [Header("Wander Settings")]
    public float wanderTime = 1f;

    [Header("Close Range Settings")]
    public float ignoreUnstuckDistance = 1.0f;

    [Header("Orbit Settings")]
    public float orbitDistance = 1.5f;
    public float orbitSpeed = 2f;
    float personalOrbitDistance;
float personalOrbitSpeed;
int orbitDirection;

    AIPath ai;

    float stuckTimer;
    float wanderTimer;
    float repathTimer;

    bool isWandering;
    Vector2 wanderDirection;

    Vector3 lastPosition;
    float orbitAngle;
    Rigidbody2D rb;

    void Awake()
    {
        ai = GetComponent<AIPath>();
        lastPosition = transform.position;

        orbitAngle = Random.Range(0f, 360f); // random start angle
        personalOrbitDistance = orbitDistance + Random.Range(-0.5f, 0.5f);
personalOrbitSpeed = orbitSpeed + Random.Range(-0.5f, 0.5f);
orbitDirection = Random.value > 0.5f ? 1 : -1;
rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector3 playerPos = GetPlayerPosition();
        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

        // ===== STUCK DETECTION (DISABLED WHEN CLOSE) =====
        if (!isWandering && distanceToPlayer > ignoreUnstuckDistance)
        {
            if (movedDistance < movementThreshold)
            {
                stuckTimer += Time.deltaTime;

                if (stuckTimer >= stuckTimeThreshold)
                {
                    StartWander();
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        // ===== WANDERING =====
        if (isWandering)
        {
            wanderTimer -= Time.deltaTime;

            ai.canMove = false;

            transform.position += (Vector3)(wanderDirection * ai.maxSpeed * Time.deltaTime);

            if (wanderTimer <= 0f)
            {
                StopWander();
            }
        }

        // ===== CONTINUOUS REPATH + ORBIT =====
        if (!isWandering)
        {
            repathTimer -= Time.deltaTime;

            if (repathTimer <= 0f)
            {
                if (distanceToPlayer < orbitDistance * 1.5f)
                {
                    ai.destination = GetOrbitPosition(); // 🔥 orbit when close
                }
                else
                {
                    ai.destination = playerPos; // normal chase
                }

                ai.SearchPath();
                repathTimer = Random.Range(0.8f, 1.2f);
            }
        }

        lastPosition = transform.position;
        ApplySeparation();
    }

    // ===== START WANDER =====
    void StartWander()
    {
        isWandering = true;
        wanderTimer = wanderTime;

        Vector2 awayFromPlayer = (transform.position - GetPlayerPosition()).normalized;
        wanderDirection = (awayFromPlayer + Random.insideUnitCircle * 0.5f).normalized;

        ai.canMove = false;
    }

    // ===== STOP WANDER =====
    void StopWander()
    {
        isWandering = false;
        stuckTimer = 0f;

        SnapToNearestNode();

        ai.canMove = true;

        ai.destination = GetPlayerPosition();
        ai.SearchPath();
    }

    // ===== SNAP TO GRID =====
    void SnapToNearestNode()
    {
        var nn = AstarPath.active.GetNearest(transform.position, NNConstraint.Default);

        if (nn.node != null && nn.node.Walkable)
        {
            transform.position = (Vector3)nn.position;
        }
    }

    // ===== GET PLAYER =====
    Vector3 GetPlayerPosition()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            return p.transform.position;

        return transform.position;
    }
 Vector3 GetOrbitPosition()
{
    Vector3 playerPos = GetPlayerPosition();
    orbitAngle += personalOrbitSpeed * orbitDirection * Time.deltaTime;

    float rad = orbitAngle * Mathf.Deg2Rad;

    Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * personalOrbitDistance;

    return playerPos + offset;
}

void ApplySeparation()
{
    Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, 0.7f);

    Vector2 push = Vector2.zero;

    foreach (var col in neighbors)
    {
        if (col.gameObject == gameObject) continue;
        if (!col.CompareTag("Enemy")) continue;

        Vector2 diff = (Vector2)(transform.position - col.transform.position);

        if (diff.magnitude > 0)
        {
            push += diff.normalized / diff.magnitude;
        }
    }

    // 🔥 apply as force instead of teleport
    rb.AddForce(push * 2f, ForceMode2D.Force);
}
}