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
        StopAllCoroutines(); // 🔥 prevent crash during scene change
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

        // 🔥 Animate
        if (NexusProgressUI.Instance != null)
        {
            yield return NexusProgressUI.Instance.AnimateStep(
                storedGems,
                required,
                currentLevel
            );
        }

        // 🔥 LEVEL UP
        if (storedGems >= required)
        {
            if (NexusProgressUI.Instance != null)
            {
                yield return NexusProgressUI.Instance.AnimateLevelUp(currentLevel + 1);
            }

            storedGems = 0;
            currentLevel++;

            // 🔥 Update UI
            if (NexusProgressUI.Instance != null)
            {
                NexusProgressUI.Instance.ForceResetUI(
                    storedGems,
                    GetRequiredGems(),
                    currentLevel
                );
            }

            // 🔥 STOP HERE (WAIT FOR PLAYER)
            isProcessing = false;

            UpgradeManager.Instance.ShowUpgrades();

            yield break; // 🔥 CRITICAL
        }
    }

    isProcessing = false;
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