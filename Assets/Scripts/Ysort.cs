using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public PlayerController player; // assign ONLY on player
    public SpriteRenderer baseRenderer; // used for tree tops
    public int offset = 0;

    SpriteRenderer sr;
    int lockedOrder;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        lockedOrder = sr.sortingOrder;
    }

    void LateUpdate()
    {
        // TREE TOP → follow base
        if (baseRenderer != null)
        {
            sr.sortingOrder = baseRenderer.sortingOrder + offset;
            return;
        }

        // PLAYER → freeze sorting while jumping
        if (player != null && player.IsJumping)
        {
            sr.sortingOrder = lockedOrder;
            return;
        }

        // NORMAL Y-SORT
        lockedOrder = Mathf.RoundToInt(-transform.position.y * 100f) + offset;
        sr.sortingOrder = lockedOrder;
    }
}