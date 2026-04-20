using UnityEngine;

namespace Matchmancer.Character
{
    /// <summary>
    /// Minimal GearData stub — just enough for LevelData rewards to compile.
    /// Replace or extend when Skill 15 (Gear System) is built.
    /// </summary>
    [CreateAssetMenu(fileName = "Gear_New", menuName = "Matchmancer/Gear Data")]
    public class GearData : ScriptableObject
    {
        public string gearName = "Frayed Amulet";
        public Sprite icon;
        [TextArea] public string description;

        [Header("Stat Bonuses")]
        public int attackBonus  = 0;
        public int defenseBonus = 0;
        public int luckBonus    = 0;
    }
}
