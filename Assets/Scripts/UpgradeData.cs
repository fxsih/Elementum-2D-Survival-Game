using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public string description;
    public Sprite icon;
    public UpgradeType type;
    public float value;
}

public enum UpgradeType
{
    // Movement
    Speed,

    // Attack 1 (Slash)
    SlashDamage,

    SlashSpeed,

    // Attack 2 (Fireball)
    FireballDamage,

    FireballSpeed,

    // Dash
    DashDamage,
    DashDuration,

    // Utility
    Health,
    LifeSteal,
    GemMultiplier,
    AuraDamage
}