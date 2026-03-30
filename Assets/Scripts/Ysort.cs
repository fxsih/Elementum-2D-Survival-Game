using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public PlayerController player;      // Assign ONLY on player
    public SpriteRenderer baseRenderer;  // Used for tree tops

    // ➕ NEW: player base reference (for dash FX)
    public Transform playerBase;         // Assign GroundRef here

    public int offset = 0;

    private SpriteRenderer sr;
    private int lockedOrder;
    private bool wasJumping;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // 1️⃣ TREE TOP → follow base
        if (baseRenderer != null)
        {
            sr.sortingOrder = baseRenderer.sortingOrder + offset;
            return;
        }

        // 2️⃣ DASH FX → follow player base Y
        if (playerBase != null)
        {
            float y = playerBase.position.y;
            sr.sortingOrder = Mathf.RoundToInt(-y * 100f) + offset;
            return;
        }

        // 3️⃣ PLAYER → detect jump START and lock order
        if (player != null)
        {
            if (player.IsJumping && !wasJumping)
            {
                // Lock CURRENT correct order
                lockedOrder = Mathf.RoundToInt(-transform.position.y * 100f) + offset;
            }

            if (player.IsJumping || player.IsDashing)
            {
                sr.sortingOrder = lockedOrder;
                wasJumping = true;
                return;
            }

            wasJumping = false;
        }

        // 4️⃣ NORMAL Y-SORT
        lockedOrder = Mathf.RoundToInt(-transform.position.y * 100f) + offset;
        sr.sortingOrder = lockedOrder;
    }
}