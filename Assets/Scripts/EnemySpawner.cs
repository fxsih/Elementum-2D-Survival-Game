using UnityEngine;
using Pathfinding;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnRate = 2f;
    public int maxEnemies = 10;

    [Header("Spawn Distance")]
    public float spawnDistanceFromCamera = 2f;

    [Header("Blocked Layers")]
    public LayerMask blockedLayers; // 👈 assign Ocean + Lava

    Camera cam;
    Transform player;

    float timer;

    void Start()
    {
        cam = Camera.main;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            timer = 0f;

            if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        if (player == null) return;

        float height = cam.orthographicSize;
        float width = height * cam.aspect;

        float spawnRadius = Mathf.Max(width, height) + spawnDistanceFromCamera;

        for (int i = 0; i < 15; i++) // more attempts = more reliable
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Vector2 spawnPos = (Vector2)player.position + dir * spawnRadius;

            // ✅ 1. ENSURE OUTSIDE CAMERA
            Vector3 viewport = cam.WorldToViewportPoint(spawnPos);

            if (viewport.x > 0 && viewport.x < 1 &&
                viewport.y > 0 && viewport.y < 1)
            {
                continue; // inside screen → reject
            }

            // ✅ 2. BLOCK OCEAN / LAVA USING LAYER
            if (Physics2D.OverlapCircle(spawnPos, 0.3f, blockedLayers))
            {
                continue; // hit water/lava → reject
            }

            // ✅ 3. A* WALKABLE CHECK
            var nn = AstarPath.active.GetNearest(spawnPos);

            if (nn.node != null && nn.node.Walkable)
            {
                Instantiate(enemyPrefab, (Vector3)nn.position, Quaternion.identity);
                return;
            }
        }

        Debug.Log("Spawn failed (no valid position)");
    }
}