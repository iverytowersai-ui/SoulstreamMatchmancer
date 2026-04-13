using UnityEngine;

/// <summary>
/// Defines a playable character's base stats, level growth curve, and ultimate.
/// Create via right-click → Create → Matchmancer → Character Data.
/// One asset for MVP. Structure supports multiple characters later.
/// </summary>
[CreateAssetMenu(fileName = "CharacterData_Default", menuName = "Matchmancer/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string     characterName  = "The Arcanist";
    public Sprite     portrait;
    public Sprite     battleSprite;

    [Header("Base Stats (Level 1)")]
    public float      baseMaxHP       = 150f;
    public float      baseAttack      = 10f;
    public float      baseDefense     = 5f;    // flat damage reduction from enemy attacks
    public float      baseCritChance  = 0.05f; // 5% — added to CombatConfig base
    public float      baseLuck        = 0f;

    [Header("Stat Growth Per Level")]
    public float      hpPerLevel      = 10f;
    public float      attackPerLevel  = 1f;
    public float      defensePerLevel = 0.5f;
    public float      luckPerLevel    = 0.2f;

    [Header("Ultimate")]
    public string     ultimateName    = "Soul Surge";
    public Sprite     ultimateIcon;
    [TextArea]
    public string     ultimateDescription;
    public float      ultimateEnergyCost  = 100f;
    public float      ultimateDamageMultiplier = 3.0f; // applied to next Damage wave

    [Header("XP Curve")]
    [Tooltip("XP required to reach level N+1. Index 0 = XP to reach level 2.")]
    public int[]      xpPerLevel      = { 100, 150, 220, 310, 420, 550, 700, 870, 1060 };

    [Header("Flavour")]
    [TextArea] public string loreDescription;
}
