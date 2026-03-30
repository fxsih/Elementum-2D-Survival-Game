using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image frontFill;
    public Image backFill;
    public RectTransform barTransform;

    public float baseWidth = 200f;
    public float widthPerHP = 1.5f;

    public void UpdateHealth(float current, float max)
    {
        float target = current / max;

        frontFill.fillAmount = target;

        // smooth backfill (optional feel)
        StopAllCoroutines();
        StartCoroutine(SmoothBackFill(target));
    }

    System.Collections.IEnumerator SmoothBackFill(float target)
    {
        float start = backFill.fillAmount;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            backFill.fillAmount = Mathf.Lerp(start, target, t);
            yield return null;
        }
    }

    public void UpdateBarSize(float maxHealth)
{
    float healthRatio = maxHealth / 100f;
    float newWidth = baseWidth * healthRatio;

    barTransform.sizeDelta = new Vector2(newWidth, barTransform.sizeDelta.y);
}
}