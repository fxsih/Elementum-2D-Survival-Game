using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lifeTime = 1f;

    TextMeshPro text;
    Color startColor;

    void Awake()
{
    text = GetComponent<TextMeshPro>();

    if (text == null)
    {
        Debug.LogError("TextMeshPro component missing!");
        return;
    }

    startColor = text.color;
}

    public void Setup(int damage)
    {
        text.text = damage.ToString();

        // random horizontal spread
        transform.position += new Vector3(Random.Range(-0.3f, 0.3f), 0, 0);

        // scale punch
        transform.localScale = Vector3.one * 1.5f;
    }

    float timer;

void Update()
{
    timer += Time.deltaTime;

    // move up
    transform.position += Vector3.up * moveSpeed * Time.deltaTime;

    // fade out
    float alpha = 1 - (timer / lifeTime);
    text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

    if (timer >= lifeTime)
    {
        Destroy(gameObject);
    }
}
}