using UnityEngine;
using TMPro;
using System.Collections;

public class GemCounter : MonoBehaviour
{
    public static GemCounter Instance;

    public int currentGems = 0;
    public TextMeshProUGUI gemText;

    void Awake()
{
    if (Instance == null)
        Instance = this;
    else
        Destroy(gameObject);
}

    void Start()
    {
        UpdateUI(); // safe here
    }

    public void AddGems(int amount)
    {
        currentGems += amount;
        UpdateUI();
    }

    public void ResetGems()
    {
        currentGems = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        gemText.text = "" + currentGems;

        // 🔥 SAME POP AS KILL TEXT
        gemText.transform.localScale = Vector3.one * 1f;
        CancelInvoke(nameof(ResetScale));
        Invoke(nameof(ResetScale), 0.2f);

        StopAllCoroutines();
        StartCoroutine(FlashEffect());
    }

    void ResetScale()
    {
        gemText.transform.localScale = Vector3.one;
    }

    IEnumerator FlashEffect()
    {
        Color normalColor = Color.white;
        Color flashColor = new Color(1f, 0.85f, 0.2f);

        gemText.color = flashColor;

        float t = 0f;
        float duration = 0.4f;

        while (t < duration)
        {
            t += Time.deltaTime;
            gemText.color = Color.Lerp(flashColor, normalColor, t / duration);
            yield return null;
        }

        gemText.color = normalColor;
    }
}