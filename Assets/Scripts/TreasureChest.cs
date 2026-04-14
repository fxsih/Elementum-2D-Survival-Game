using UnityEngine;
using System.Collections;
using TMPro;

public class TreasureChest : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 12;
    int currentHealth;

    [Header("Timer")]
    public float refillTime = 120f;
    float timer;
    bool isReady = true;

    [Header("Rewards")]
    public int minGems = 50;
    public int maxGems = 100;
    int gemsToSpawn;

    [Header("Gem Prefab")]
    public GameObject gemPrefab;

    [Header("UI")]
    public GameObject timerUI;
    public TMP_Text timerValueText;

    CanvasGroup timerCanvasGroup;
    Coroutine fadeRoutine;

    [Header("References")]
    public Animator animator;

    SpriteRenderer sr;
    Color originalColor;

    bool isOpening = false;

    [SerializeField] AudioClip[] hitSounds;
[SerializeField] float volume = 1f;

int lastHitIndex = -1;

    void Start()
    {
        currentHealth = maxHealth;
        timer = refillTime;

        // Sprite setup
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;

        // 🔥 UI Setup (AUTO FIX)
        if (timerUI != null)
        {
            timerCanvasGroup = timerUI.GetComponent<CanvasGroup>();

            if (timerCanvasGroup == null)
                timerCanvasGroup = timerUI.AddComponent<CanvasGroup>();

            timerCanvasGroup.alpha = 0f;
            timerUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!isReady)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                timer = 0f;
                isReady = true;
                Debug.Log("🟢 Chest Ready");
            }
        }
    }

    // ================= HIT =================

    public void HitChest()
    {   
        PlayHitSound();
        Debug.Log("💥 Chest Hit");

        if (isOpening) return;

        animator.SetTrigger("Hit");
        StartCoroutine(HitEffect());

        if (!isReady)
        {
            ShowTimerUI();
            return;
        }

        currentHealth--;

        Debug.Log("❤️ HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpening = true;

        animator.SetTrigger("Open");

        gemsToSpawn = Random.Range(minGems, maxGems + 1);

        Debug.Log("💎 Will spawn: " + gemsToSpawn);

        StartCoroutine(CloseChest());
    }

    // ================= GEM SPAWN =================

    public void SpawnGemsFromAnimation()
    {
        Debug.Log("🎯 Animation Spawn");

        for (int i = 0; i < gemsToSpawn; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.6f;

            GameObject gem = Instantiate(
                gemPrefab,
                transform.position + (Vector3)offset,
                Quaternion.identity
            );

            GemPickup g = gem.GetComponent<GemPickup>();

            if (g != null)
            {
                g.amount = 1;

                // 🔥 chest-only feel
                g.scatterDistance = 1.2f;
                g.bounceHeight = 0.4f;
                g.bounceDuration = 0.35f;

                g.StartBounce();
            }
        }
    }

    IEnumerator CloseChest()
    {
        yield return new WaitForSeconds(5f);

        animator.SetTrigger("Close");

        currentHealth = maxHealth;
        isReady = false;
        timer = refillTime;
        isOpening = false;

        Debug.Log("🔵 Chest Reset");

        if (timerUI != null)
            timerUI.SetActive(false);
    }

    // ================= UI (FADE TEXT) =================

    void ShowTimerUI()
{
    Debug.Log("📢 ShowTimerUI called");

    if (timerUI == null || timerValueText == null)
    {
        Debug.LogError("❌ UI NOT ASSIGNED");
        return;
    }

    int m = Mathf.FloorToInt(timer / 60f);
    int s = Mathf.FloorToInt(timer % 60f);

    string timeText = $"{m:00}:{s:00}";

    // 🔥 FIX: add correct unit
    if (m > 0)
        timerValueText.text = timeText + " MINUTES";
    else
        timerValueText.text = timeText + " SECONDS";

    if (fadeRoutine != null)
        StopCoroutine(fadeRoutine);

    fadeRoutine = StartCoroutine(FadeTimerText());
}
    IEnumerator FadeTimerText()
    {
        timerUI.SetActive(true);

        float fadeInTime = 0.15f;
        float stayTime = 0.8f;
        float fadeOutTime = 0.4f;

        float t = 0f;

        // 🔥 RESET ALPHA
        timerCanvasGroup.alpha = 0f;

        // FADE IN
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            timerCanvasGroup.alpha = Mathf.Clamp01(t / fadeInTime);
            yield return null;
        }

        timerCanvasGroup.alpha = 1f;

        yield return new WaitForSeconds(stayTime);

        // FADE OUT
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            timerCanvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOutTime);
            yield return null;
        }

        timerCanvasGroup.alpha = 0f;
        timerUI.SetActive(false);
    }

    // ================= HIT EFFECT =================

    IEnumerator HitEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.15f;

        if (sr != null)
            sr.color = new Color(0.6f, 0.6f, 0.6f);

        yield return new WaitForSeconds(0.08f);

        transform.localScale = originalScale;

        if (sr != null)
            sr.color = originalColor;
    }

    void PlayHitSound()
{
    if (hitSounds == null || hitSounds.Length == 0) return;
    if (AudioManager.Instance == null) return;

    int index;
    do
    {
        index = Random.Range(0, hitSounds.Length);
    }
    while (index == lastHitIndex && hitSounds.Length > 1);

    lastHitIndex = index;

    float pitch = Random.Range(0.95f, 1.1f);

    AudioSource source = AudioManager.Instance.sfxSource;
    float originalPitch = source.pitch;

    source.pitch = pitch;
    source.PlayOneShot(hitSounds[index], volume);
    source.pitch = originalPitch;
}
}