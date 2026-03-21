using UnityEngine;

public class ProjectileGlow : MonoBehaviour
{
    public Color glowColor = new Color(1f, 0.5f, 0f);
    public float glowIntensity = 3f;

    Material mat;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            mat = sr.material;
        }
    }

    void Update()
    {
        if (mat == null) return;

        Color final = glowColor * glowIntensity;

        // 🔥 Try ALL common properties (one will work)
        mat.SetColor("_EmissionColor", final);
        mat.SetColor("_GlowColor", final);
        mat.SetColor("_Color", final);
    }
}