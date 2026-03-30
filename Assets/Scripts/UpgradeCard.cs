using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCard : MonoBehaviour
{
    public Image iconImage; // ⭐ ADD THIS
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

        // ✅ SET ICON (THIS WAS MISSING)
        if (iconImage != null && upgrade.icon != null)
        {
            iconImage.sprite = upgrade.icon;
        }

        if (titleText != null)
            titleText.text = upgrade.upgradeName;

        if (descriptionText != null)
            descriptionText.text = upgrade.description;
    }

    public void OnClick()
    {
        if (upgrade == null)
        {
            Debug.LogError("❌ Clicked upgrade is NULL");
            return;
        }

        UpgradeManager.Instance.SelectUpgrade(upgrade);
    }
}