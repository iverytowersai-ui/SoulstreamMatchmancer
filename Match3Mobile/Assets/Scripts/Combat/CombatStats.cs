using System;
using UnityEngine;

/// <summary>
/// Tracks all combat statistics for the current battle session.
/// Reset at the start of each level. Final values are read by
/// StarRatingSystem and AchievementTracker at level end.
/// </summary>
public class CombatStats : MonoBehaviour
{
    #region Singleton
    public static CombatStats Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    #endregion

    #region Tracked Stats
    // Damage
    public float TotalDamageDealt   { get; private set; }
    public float MaxSingleHit       { get; private set; }
    public int   TotalCriticalHits  { get; private set; }

    // Combos
    public int   CurrentCombo       { get; private set; }
    public int   MaxCombo           { get; private set; }

    // Combat actions
    public int   TotalMoves         { get; private set; }
    public int   InventoryMovesUsed { get; private set; }
    public int   ClutchMoves        { get; private set; }   // win with ≤2 HP or ≤1 move

    // Defense
    public float TotalShieldGenerated { get; private set; }

    // Energy / Ultimate
    public float TotalEnergyGenerated { get; private set; }
    public int   UltimatesActivated   { get; private set; }

    // Luck events
    public int   LuckTilesMatched   { get; private set; }

    // Completion
    public bool  IsVictory          { get; private set; }
    public float BattleDuration     { get; private set; }
    public bool  BattleEnded        { get; private set; }  // true after any RecordVictory call
    #endregion

    #region Events
    public event Action<float> OnDamageDealt;
    public event Action<int>   OnComboChanged;
    public event Action<bool>  OnCritOccurred;
    #endregion

    #region Public — Reset
    /// <summary>Call at the start of every level.</summary>
    public void ResetForNewBattle()
    {
        TotalDamageDealt    = 0f;
        MaxSingleHit        = 0f;
        TotalCriticalHits   = 0;
        CurrentCombo        = 0;
        MaxCombo            = 0;
        TotalMoves          = 0;
        InventoryMovesUsed  = 0;
        ClutchMoves         = 0;
        TotalShieldGenerated = 0f;
        TotalEnergyGenerated = 0f;
        UltimatesActivated  = 0;
        LuckTilesMatched    = 0;
        IsVictory           = false;
        BattleEnded         = false;
        BattleDuration      = 0f;
        battleStartTime     = Time.time;
    }
    #endregion

    #region Public — Record Events
    public void RecordDamage(float amount, bool isCrit)
    {
        TotalDamageDealt += amount;
        if (amount > MaxSingleHit) MaxSingleHit = amount;
        if (isCrit) { TotalCriticalHits++; OnCritOccurred?.Invoke(true); }
        OnDamageDealt?.Invoke(amount);
    }

    public void RecordCombo(int comboCount)
    {
        CurrentCombo = comboCount;
        if (comboCount > MaxCombo) MaxCombo = comboCount;
        OnComboChanged?.Invoke(comboCount);
    }

    public void ResetCombo()
    {
        CurrentCombo = 0;
        OnComboChanged?.Invoke(0);
    }

    public void RecordMove()                          => TotalMoves++;
    public void RecordInventoryMove()                 => InventoryMovesUsed++;
    public void RecordClutchMove()                    => ClutchMoves++;
    public void RecordShield(float amount)            => TotalShieldGenerated += amount;
    public void RecordEnergy(float amount)            => TotalEnergyGenerated += amount;
    public void RecordUltimate()                      => UltimatesActivated++;
    public void RecordLuckTile()                      => LuckTilesMatched++;
    public void RecordVictory(bool won, float time)
    {
        IsVictory      = won;
        BattleDuration = time;
        BattleEnded    = true;  // freezes the Update() timer regardless of win/loss
    }
    #endregion

    #region Unity Lifecycle
    private float battleStartTime;
    private void Start() => battleStartTime = Time.time;
    private void Update()
    {
        // Freeze timer once the battle ends (victory OR defeat), not just on win.
        if (!BattleEnded) BattleDuration = Time.time - battleStartTime;
    }
    #endregion
}
