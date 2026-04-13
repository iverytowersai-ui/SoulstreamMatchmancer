using UnityEngine;

/// <summary>
/// ScriptableObject that defines one tile type's visual and base combat properties.
/// Create one asset per TileType via right-click → Create → Matchmancer → Tile Type Data.
/// Assign all six to GameBoard's tileTypeData[] array (indexed by TileType cast to int).
/// </summary>
[CreateAssetMenu(fileName = "TileTypeData_Damage", menuName = "Matchmancer/Tile Type Data")]
public class TileTypeData : ScriptableObject
{
    [Header("Identity")]
    public TileType  tileType;
    public string    displayName;    // e.g. "Soulstream Shard"
    public Sprite    icon;
    public Color     tileColor = Color.white;

    [Header("Base Combat Values")]
    [Tooltip("Base value per tile of this type in a match.")]
    public float baseDamageValue  = 10f;  // used by Damage tiles
    public float baseEnergyValue  = 15f;  // used by Energy tiles
    public float baseDefenseValue = 8f;   // used by Defense tiles

    [Header("Flavour")]
    [TextArea] public string loreDescription;
}
