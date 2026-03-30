using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public GameObject panel;
    public List<UpgradeData> allUpgrades = new List<UpgradeData>();
    public UpgradeCard[] cards;

    public PlayerController player;

    void Awake()
    {
        Instance = this;
    }

    public void ShowUpgrades()
{
    if (panel != null)
        panel.SetActive(true);

    Time.timeScale = 0f; // ⭐ PAUSE GAME

    List<UpgradeData> selected = GetRandomUpgrades(cards.Length);

    int count = Mathf.Min(cards.Length, selected.Count);

    for (int i = 0; i < count; i++)
    {
        cards[i].Setup(selected[i]);
    }
}

    public void HideUpgrades()
{
    if (panel != null)
        panel.SetActive(false);

    Time.timeScale = 1f; // ⭐ RESUME GAME
}

    public void SelectUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogError("❌ Upgrade is NULL");
            return;
        }

        ApplyUpgrade(upgrade);

        HideUpgrades();

        // 🔥 Resume level processing
        GameManager.Instance.AddGems(0);
    }

    void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            Debug.LogError("❌ UpgradeData missing!");
            return;
        }

        player.ApplyUpgrade(upgrade);
        Debug.Log("Applied Upgrade: " + upgrade.upgradeName);
    }

    List<UpgradeData> GetRandomUpgrades(int count)
{
    List<UpgradeData> valid = new List<UpgradeData>();

    // ✅ Remove nulls
    foreach (var u in allUpgrades)
    {
        if (u != null)
            valid.Add(u);
    }

    // ❌ No upgrades available
    if (valid.Count == 0)
    {
        Debug.LogError("❌ No upgrades available!");
        return new List<UpgradeData>();
    }

    // 🔥 Shuffle list (better randomness)
    for (int i = 0; i < valid.Count; i++)
    {
        int rand = Random.Range(i, valid.Count);
        (valid[i], valid[rand]) = (valid[rand], valid[i]);
    }

    // ✅ Pick first N (no duplicates guaranteed)
    List<UpgradeData> result = new List<UpgradeData>();

    for (int i = 0; i < count && i < valid.Count; i++)
    {
        result.Add(valid[i]);
    }

    return result;
}
}