using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCard : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    UpgradeData upgrade;

    public void Setup(UpgradeData newUpgrade)
    {
        upgrade = newUpgrade;

        if (upgrade == null)
        {
            Debug.LogError("❌ Upgrade is NULL");
            return;
        }

        if (iconImage != null && upgrade.icon != null)
            iconImage.sprite = upgrade.icon;

        if (titleText != null)
            titleText.text = upgrade.upgradeName;

        if (descriptionText != null)
            descriptionText.text = upgrade.description;
    }

    // 🔥 REQUIRED (fixes your error)
    public UpgradeData GetUpgrade()
    {
        return upgrade;
    }

    // 🔥 Button click
    public void OnClick()
    {
        UpgradeManager.Instance.SelectUpgrade(upgrade);
    }
}