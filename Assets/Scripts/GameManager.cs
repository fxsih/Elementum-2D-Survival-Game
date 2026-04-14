using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int KillCount = 0;
    public float levelStartTime = 0f;

   [Header("Audio")]
public AudioClip gameMusic;

[Range(0f,1f)] public float gameMusicBaseVolume = 0.8f;

    public Texture2D cursorTexture;

    [Header("Level System")]
    public int currentLevel = 1;
    public int storedGems = 0;
    public int baseRequirement = 10;

    
    
    public int altarUses = 1;

    [Header("Game Timer")]
public float gameTime = 0f;

    int pendingGems = 0;
    bool isProcessing = false;

    [Header("Level Requirements")]
    public int[] levelRequirements;
void Start()
{
    ScoreManager.Instance.ResetScore();
    Cursor.SetCursor(cursorTexture, new Vector2(23, 23), CursorMode.ForceSoftware);
    Cursor.lockState = CursorLockMode.Confined;
    if (AudioManager.Instance != null)
    {
        AudioManager.Instance.PlayMusicWithFade(gameMusic, 0.5f, 1f, 0.5f, gameMusicBaseVolume);
    }
}
    void Update()
{
    gameTime += Time.deltaTime;
}

    void Awake()
    {
        Application.targetFrameRate = 120;

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    // ---------------- KILLS ----------------
    public void AddKill()
    {
        KillCount++;
        UIManager.Instance.UpdateKillText(KillCount);
    }

    public int GetKills()
    {
        return KillCount;
    }

    // ---------------- GEMS ----------------
    public void AddGems(int amount)
    {
        pendingGems += amount;

        if (!isProcessing)
        {
            StartCoroutine(ProcessDeposit());
        }
    }

    IEnumerator ProcessDeposit()
    {
        isProcessing = true;

        while (pendingGems > 0)
        {
            int required = GetRequiredGems();
            int needed = required - storedGems;

            int toAdd = Mathf.Min(pendingGems, needed);

            storedGems += toAdd;
            pendingGems -= toAdd;

            // 🔥 Animate progress
            if (NexusProgressUI.Instance != null)
            {
                yield return NexusProgressUI.Instance.AnimateStep(
                    storedGems,
                    required,
                    currentLevel
                );
            }

            // 🔥 LEVEL UP CHECK
            if (storedGems >= required)
            {
                altarUses = 1; // reset to 1 use per level
                if (NexusProgressUI.Instance != null)
                {
                    yield return NexusProgressUI.Instance.AnimateLevelUp(currentLevel + 1);
                }

                // 🔥 KEEP EXTRA GEMS (CRITICAL FIX)
                storedGems -= required;

                currentLevel++;
                levelStartTime = gameTime;

                // 🔥 Update UI after level up
                if (NexusProgressUI.Instance != null)
                {
                    NexusProgressUI.Instance.ForceResetUI(
                        storedGems,
                        GetRequiredGems(),
                        currentLevel
                    );
                }

                // 🔥 PAUSE processing for upgrade selection
                isProcessing = false;

                UpgradeManager.Instance.ShowUpgrades();

                yield break; // wait until player selects upgrade
            }
        }

        isProcessing = false;
    }

    // 🔥 CALL THIS AFTER PLAYER PICKS UPGRADE
    public void ResumeProcessing()
    {
        if (!isProcessing && pendingGems > 0)
        {
            StartCoroutine(ProcessDeposit());
        }
    }

    int GetRequiredGems()
    {
        if (levelRequirements != null && currentLevel - 1 < levelRequirements.Length)
        {
            return levelRequirements[currentLevel - 1];
        }

        return baseRequirement + (currentLevel * 5);
    }

    public int GetStoredGems()
    {
        return storedGems;
    }

    public int GetRequiredGemsPublic()
    {
        return GetRequiredGems();
    }


}