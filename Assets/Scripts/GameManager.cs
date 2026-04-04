using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    int KillCount = 0;

    [Header("Level System")]
    public int currentLevel = 1;
    public int storedGems = 0;
    public int baseRequirement = 10;

    int pendingGems = 0;
    bool isProcessing = false;

    [Header("Level Requirements")]
    public int[] levelRequirements;

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
                // 🎉 Level animation
                if (NexusProgressUI.Instance != null)
                {
                    yield return NexusProgressUI.Instance.AnimateLevelUp(currentLevel + 1);
                }

                // 🔥 KEEP EXTRA GEMS (CRITICAL FIX)
                storedGems -= required;

                currentLevel++;

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