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
        StartCoroutine(ProcessDeposit(amount));
    }

    IEnumerator ProcessDeposit(int amount)
    {
        while (amount > 0)
        {
            int required = GetRequiredGems();
            int needed = required - storedGems;

            int toAdd = Mathf.Min(amount, needed);

            storedGems += toAdd;
            amount -= toAdd;

            if (NexusProgressUI.Instance != null)
            {
                yield return NexusProgressUI.Instance.AnimateStep(
                    storedGems,
                    required,
                    currentLevel
                );
            }

            if (storedGems >= required)
            {
                storedGems = 0;
                currentLevel++;

                if (NexusProgressUI.Instance != null)
                {
                    yield return NexusProgressUI.Instance.AnimateLevelUp(currentLevel);
                }

                Debug.Log("LEVEL UP! Level: " + currentLevel);
            }
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