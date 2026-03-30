using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NexusProgressUI : MonoBehaviour
{
    public static NexusProgressUI Instance;

    public Image fillImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI progressText;

    [Header("Speeds")]
    public float fillSpeed = 2f;
    public float numberSpeed = 100f;
    public float levelSpeed = 5f;

    int displayedValue = 0;
    int displayedLevel = 1;

    bool isDestroyed = false;

    Coroutine fillRoutine;
    Coroutine numberRoutine;
    Coroutine levelRoutine;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        isDestroyed = true;
        StopAllCoroutines();
    }

    // ---------------- MAIN ----------------

    public IEnumerator AnimateStep(int current, int required, int level)
    {
        float targetFill = (float)current / required;

        StartFill(targetFill);
        StartNumber(current, required);
        StartLevel(level);

        yield return null;
    }

    public IEnumerator AnimateLevelUp(int newLevel)
{
    // 🔥 Step 1: Fill completely
    yield return StartCoroutine(SmoothFill(1f));

    if (isDestroyed || fillImage == null) yield break;

    // 🔥 Step 2: Reset bar
    fillImage.fillAmount = 0f;

    // 🔥 Step 3: RESET NUMBER 🔥
    displayedValue = 0;
    if (progressText != null)
        progressText.text = "0/" + GameManager.Instance.GetRequiredGemsPublic();

    // 🔥 Step 4: Update level
    StartLevel(newLevel);
}

    // ---------------- STARTERS ----------------

    void StartFill(float target)
    {
        if (fillRoutine != null) StopCoroutine(fillRoutine);
        fillRoutine = StartCoroutine(SmoothFill(target));
    }

    void StartNumber(int target, int required)
    {
        if (numberRoutine != null) StopCoroutine(numberRoutine);
        numberRoutine = StartCoroutine(SmoothNumber(target, required));
    }

    void StartLevel(int target)
    {
        if (levelRoutine != null) StopCoroutine(levelRoutine);
        levelRoutine = StartCoroutine(SmoothLevel(target));
    }

    // ---------------- ANIMATIONS ----------------

    IEnumerator SmoothFill(float target)
    {
        if (fillImage == null) yield break;

        while (Mathf.Abs(fillImage.fillAmount - target) > 0.001f)
        {
            if (isDestroyed || fillImage == null) yield break;

            fillImage.fillAmount = Mathf.MoveTowards(
                fillImage.fillAmount,
                target,
                fillSpeed * Time.deltaTime
            );

            yield return null;
        }

        fillImage.fillAmount = target;
    }

   IEnumerator SmoothNumber(int targetValue, int required)
{
    if (progressText == null) yield break;

    // 🔥 SAFETY RESET (critical)
    if (displayedValue > targetValue)
        displayedValue = targetValue;

    while (displayedValue != targetValue)
    {
        if (isDestroyed || progressText == null) yield break;

        displayedValue = (int)Mathf.MoveTowards(
    displayedValue,
    targetValue,
    Time.deltaTime * numberSpeed * 100f
);

        progressText.text = displayedValue + "/" + required;

        yield return null;
    }

    displayedValue = targetValue;
    progressText.text = targetValue + "/" + required;
}

    IEnumerator SmoothLevel(int targetLevel)
    {
        if (levelText == null) yield break;

        float currentLevelValue = displayedLevel; // 🔥 float internally

        while (Mathf.Abs(currentLevelValue - targetLevel) > 0.01f)
        {
            if (isDestroyed || levelText == null) yield break;

            currentLevelValue = Mathf.MoveTowards(
                currentLevelValue,
                targetLevel,
                levelSpeed * Time.deltaTime
            );

            displayedLevel = Mathf.RoundToInt(currentLevelValue); // ✅ FIX

            levelText.text = "" + displayedLevel;

            yield return null;
        }

        displayedLevel = targetLevel;
        levelText.text = "" + targetLevel;
    }

   public void ForceResetUI(int current, int required, int level)
{
    // 🔥 STOP ALL animations FIRST
    if (fillRoutine != null) StopCoroutine(fillRoutine);
    if (numberRoutine != null) StopCoroutine(numberRoutine);
    if (levelRoutine != null) StopCoroutine(levelRoutine);

    // 🔥 RESET INTERNAL STATE (VERY IMPORTANT)
    displayedValue = current;
    displayedLevel = level;

    // 🔥 FORCE UI VALUES
    if (fillImage != null)
        fillImage.fillAmount = 0f;

    if (progressText != null)
        progressText.text = current + "/" + required;

    if (levelText != null)
        levelText.text = "" + level;
}
}