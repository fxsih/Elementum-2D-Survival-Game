using UnityEngine;

public class FadeWhenBehind : MonoBehaviour
{
    private SpriteRenderer sr;

    public float fadedAlpha = 0.3f;
    public float normalAlpha = 1f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetAlpha(fadedAlpha);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetAlpha(normalAlpha);
        }
    }

    void SetAlpha(float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}