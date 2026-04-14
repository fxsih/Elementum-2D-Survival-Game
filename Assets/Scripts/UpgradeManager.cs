using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("UI")]
    public GameObject panel;

    [Header("Cards")]
    public UpgradeCard[] cards;

    [Header("Data")]
    public List<UpgradeData> allUpgrades = new List<UpgradeData>();
    public PlayerController player;

    [Header("Runtime")]
    public List<CardUI> activeCards = new List<CardUI>();

    int currentIndex = -1; // ❌ NO DEFAULT SELECTION
    CardUI currentCard;

    public bool usingKeyboard = false;

    Vector3 lastMousePos;

    public CanvasGroup cardCanvasGroup;

    [Header("Level Up Audio")]
public AudioClip[] levelUpSounds;

[Range(0f,1f)]
public float levelUpVolume = 1f;

public float levelUpMinPitch = 0.95f;
public float levelUpMaxPitch = 1.1f;

int lastLevelUpIndex = -1;

    void Awake()
    {
        Instance = this;
    }

  void Update()
{
    if (!panel.activeSelf) return;
    if (activeCards.Count == 0) return;

    // 🎮 KEYBOARD INPUT → TAKE CONTROL
    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
    {
        ActivateKeyboardMode();
    }

    // 🖱️ MOUSE MOVEMENT → TAKE BACK CONTROL
    if (usingKeyboard)
    {
        if ((Input.mousePosition - lastMousePos).sqrMagnitude > 0.01f)
        {
            ActivateMouseMode();
        }
    }

    lastMousePos = Input.mousePosition;

    // 🎮 KEYBOARD NAVIGATION
    if (usingKeyboard)
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveSelection(1);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveSelection(-1);

        if (Input.GetKeyDown(KeyCode.Return))
            ConfirmSelection();
    }
}

void ActivateKeyboardMode()
{
    usingKeyboard = true;

    Cursor.visible = false;

    EventSystem.current.SetSelectedGameObject(null);

    foreach (var card in activeCards)
        card.ForceDeselect();

    // 🔥 BLOCK ALL MOUSE INTERACTION
    if (cardCanvasGroup != null)
        cardCanvasGroup.blocksRaycasts = false;

    if (currentIndex == -1)
        SelectCard(0);
}

void ActivateMouseMode()
{
    usingKeyboard = false;

    Cursor.visible = true;

    currentIndex = -1;
    currentCard = null;

    EventSystem.current.SetSelectedGameObject(null);

    // 🔥 ENABLE MOUSE AGAIN
    if (cardCanvasGroup != null)
        cardCanvasGroup.blocksRaycasts = true;
}
    // =========================
    // 🎴 SHOW UPGRADES
    // =========================
    public void ShowUpgrades()
{
    PlayLevelUpSound();
    panel.SetActive(true);
    Time.timeScale = 0f;

    Cursor.visible = true;

    List<UpgradeData> upgrades = GetRandomUpgrades(cards.Length);

    activeCards.Clear();

    for (int i = 0; i < cards.Length; i++)
{
    if (i >= upgrades.Count)
    {
        cards[i].gameObject.SetActive(false);
        continue;
    }

    cards[i].gameObject.SetActive(true);
    cards[i].Setup(upgrades[i]);

    CardUI ui = cards[i].GetComponent<CardUI>();
    ui.ResetCard();
    activeCards.Add(ui);
}

    // 🔥 CLEAR ANY OLD SELECTION
    if (EventSystem.current != null)
        EventSystem.current.SetSelectedGameObject(null);

    currentIndex = -1;
    currentCard = null;
    usingKeyboard = false;

    // 🔥 ENABLE mouse interaction
    if (cardCanvasGroup != null)
        cardCanvasGroup.blocksRaycasts = true;
}
    public void HideUpgrades()
{
    panel.SetActive(false);
    Time.timeScale = 1f;

    foreach (var card in activeCards)
{
    card.ResetCard();
}

    // 🔥 RESTORE MOUSE
    Cursor.visible = true;

    // 🔥 RESET INPUT MODE
    usingKeyboard = false;

    // 🔥 CLEAR selection completely
    if (EventSystem.current != null)
        EventSystem.current.SetSelectedGameObject(null);

    // 🔥 ENABLE mouse interaction again
    if (cardCanvasGroup != null)
        cardCanvasGroup.blocksRaycasts = true;
}

    // =========================
    // 🎯 SELECTION
    // =========================
    public void SetHoveredCard(CardUI card)
    {
        if (usingKeyboard) return;

        EventSystem.current.SetSelectedGameObject(card.gameObject);
    }

    void SelectCard(int index)
    {
        if (activeCards.Count == 0) return;

        currentIndex = index;
        currentCard = activeCards[currentIndex];

        EventSystem.current.SetSelectedGameObject(currentCard.gameObject);
    }

    void MoveSelection(int dir)
    {
        if (currentIndex == -1)
        {
            SelectCard(0);
            return;
        }

        int newIndex = currentIndex + dir;

        if (newIndex < 0) newIndex = activeCards.Count - 1;
        if (newIndex >= activeCards.Count) newIndex = 0;

        SelectCard(newIndex);
    }

    void ConfirmSelection()
    {
        if (currentCard == null) return;

        UpgradeData upgrade = currentCard.GetUpgrade();

        if (upgrade == null) return;

        ApplyUpgrade(upgrade);
        HideUpgrades();

        GameManager.Instance.ResumeProcessing();
    }

    void ClearSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    // =========================
    // ⚙️ APPLY
    // =========================
    void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return;

        player.ApplyUpgrade(upgrade);
        Debug.Log("Applied Upgrade: " + upgrade.upgradeName);
    }

    // =========================
    // 🎲 RANDOM
    // =========================
    List<UpgradeData> GetRandomUpgrades(int count)
{
    List<UpgradeData> valid = new List<UpgradeData>();

    // ✅ Filter valid upgrades
    foreach (var u in allUpgrades)
    {
        if (u != null && IsUpgradeValid(u))
            valid.Add(u);
    }

    // ❌ No upgrades available
    if (valid.Count == 0)
    {
        Debug.LogWarning("No valid upgrades available!");
        return new List<UpgradeData>();
    }

    // 🔥 Shuffle
    for (int i = 0; i < valid.Count; i++)
    {
        int rand = Random.Range(i, valid.Count);
        (valid[i], valid[rand]) = (valid[rand], valid[i]);
    }

    List<UpgradeData> result = new List<UpgradeData>();
    HashSet<UpgradeType> usedTypes = new HashSet<UpgradeType>();

    foreach (var upgrade in valid)
    {
        // 🔥 Skip duplicate types
        if (usedTypes.Contains(upgrade.type))
            continue;

        result.Add(upgrade);
        usedTypes.Add(upgrade.type);

        if (result.Count >= count)
            break;
    }

    return result;
}

    public void SelectUpgrade(UpgradeData upgrade)
    {
        ApplyUpgrade(upgrade);
        HideUpgrades();

        GameManager.Instance.ResumeProcessing();
    }

   bool IsUpgradeValid(UpgradeData upgrade)
{
    PlayerController player = PlayerController.Instance;

    switch (upgrade.type)
    {
        case UpgradeType.Speed:
            return player.moveSpeed < player.maxMoveSpeed;

        case UpgradeType.SlashSpeed:
            return player.GetFinalSlashSpeed() < player.maxSlashSpeed;

        case UpgradeType.FireballSpeed:
            return player.GetFinalFireballSpeed() < player.maxFireballSpeed;

        case UpgradeType.DashDuration:
            return player.GetFinalDashDuration() < player.maxDashDuration;

            case UpgradeType.LifeSteal:
    return !player.hasLifeSteal;

case UpgradeType.GemMultiplier:
    return !player.hasGemMultiplier;

case UpgradeType.AuraDamage:
    return !player.hasAura;

    case UpgradeType.AuraDamageBoost:
    return player.hasAura;

case UpgradeType.GemMultiplierBoost:
    return player.hasGemMultiplier;

case UpgradeType.LifeStealBoost:
    return player.hasLifeSteal;

    case UpgradeType.AuraRadius:
    return player.hasAura && player.auraRadius < player.maxAuraRadius;

        default:
            return true;
    }
}

void PlayLevelUpSound()
{
    if (levelUpSounds == null || levelUpSounds.Length == 0) return;
    if (AudioManager.Instance == null) return;

    int index;

    do
    {
        index = Random.Range(0, levelUpSounds.Length);
    }
    while (index == lastLevelUpIndex && levelUpSounds.Length > 1);

    lastLevelUpIndex = index;

    float pitch = Random.Range(levelUpMinPitch, levelUpMaxPitch);

    AudioSource source = AudioManager.Instance.sfxSource;

    float originalPitch = source.pitch;
    source.pitch = pitch;

    source.PlayOneShot(levelUpSounds[index], levelUpVolume);

    source.pitch = originalPitch;
}
}