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
    Damage,
    Speed,
    AttackSpeed,
    Health
}