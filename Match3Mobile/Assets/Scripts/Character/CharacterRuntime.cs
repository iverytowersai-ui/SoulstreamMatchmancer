using System;
using UnityEngine;

/// <summary>
/// Live player state during a battle.
/// - Receives enemy attacks from EnemyTurnController and reduces HP/shield
/// - Feeds current attack + luck into CombatResolver each wave
/// - Tracks XP gained this battle
/// - Fires OnPlayerDefeated when HP reaches zero
/// </summary>
public class CharacterRuntime : MonoBehaviour
{
    #region Singleton
    public static CharacterRuntime Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    #endregion

    #region Inspector Fields
    [SerializeField] private CombatResolver      combatResolver;
    [SerializeField] private EnemyTurnController enemyTurnController;
    #endregion

    #region Runtime State
    private CharacterData data;
    private int           currentLevel;
    private float         currentHP;
    private float         maxHP;
    private float         currentShield;
    private float         currentEnergy;
    private float         currentAttack;
    private float         currentDefense;
    private float         currentLuck;
    private int           currentXP;
    private bool          isDefeated;

    // Ultimate state
    private bool          ultimateReady;
    private bool          ultimateActiveNextWave; // queued by player, fires on next ResolveWave
    #endregion

    #region Properties
    public CharacterData Data          => data;
    public int           Level         => currentLevel;
    public float         CurrentHP     => currentHP;
    public float         MaxHP         => maxHP;
    public float         CurrentShield => currentShield;
    public float         CurrentEnergy => currentEnergy;
    public float         MaxEnergy     => data != null ? data.ultimateEnergyCost : 100f;
    public float         Attack        => currentAttack;
    public float         Defense       => currentDefense;
    public float         Luck          => currentLuck;
    public bool          UltimateReady => ultimateReady;
    public bool          IsDefeated    => isDefeated;
    public int           CurrentXP     => currentXP;
    #endregion

    #region Events
    public event Action<float, float> OnHPChanged;       // current, max
    public event Action<float, float> OnEnergyChanged;   // current, max
    public event Action<float>        OnShieldChanged;   // current shield amount
    public event Action<bool>         OnUltimateReady;   // true = ready, false = used
    public event Action<float>        OnDamageTaken;     // damage amount after reduction
    public event Action               OnPlayerDefeated;
    public event Action<int, int>     OnXPChanged;       // current xp, xp to next level
    public event Action<int>          OnLevelUp;         // new level
    #endregion

    #region Initialisation
    /// <summary>Call at the start of each battle.</summary>
    public void InitialiseForBattle(CharacterData characterData, int level)
    {
        if (characterData == null)
        {
            Debug.LogError(
                "[CharacterRuntime] InitialiseForBattle called with null CharacterData — " +
                "aborting. Check the BattleInitializer inspector wiring.", this);
            return;
        }

        data           = characterData;
        currentLevel   = Mathf.Max(1, level);
        isDefeated     = false;
        currentShield  = 0f;
        currentEnergy  = 0f;
        currentXP      = 0;
        ultimateReady  = false;
        ultimateActiveNextWave = false;

        // Calculate stats for this level
        maxHP          = data.baseMaxHP      + data.hpPerLevel      * (currentLevel - 1);
        currentHP      = maxHP;
        currentAttack  = data.baseAttack     + data.attackPerLevel  * (currentLevel - 1);
        currentDefense = data.baseDefense    + data.defensePerLevel * (currentLevel - 1);
        currentLuck    = data.baseLuck       + data.luckPerLevel    * (currentLevel - 1);

        OnHPChanged?.Invoke(currentHP, maxHP);
        OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy);
        OnShieldChanged?.Invoke(currentShield);
    }
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        if (combatResolver      != null) combatResolver.OnWaveResolved              += FeedStatsToResolver;
        if (enemyTurnController != null) enemyTurnController.OnEnemyActionExecuted  += ReceiveEnemyAction;
    }

    private void OnDisable()
    {
        if (combatResolver      != null) combatResolver.OnWaveResolved              -= FeedStatsToResolver;
        if (enemyTurnController != null) enemyTurnController.OnEnemyActionExecuted  -= ReceiveEnemyAction;
    }
    #endregion

    #region Receive Enemy Attack
    private void ReceiveEnemyAction(EnemyActionData action, float rawDamage)
    {
        if (isDefeated || data == null) return;

        switch (action.actionType)
        {
            case EnemyActionType.Attack:
                TakeEnemyDamage(rawDamage);
                break;

            case EnemyActionType.Debuff:
                // Reduce attack by 20% for 1 move
                currentAttack = Mathf.Max(1f, currentAttack * 0.8f);
                Debug.Log($"[Character] Debuffed — attack reduced to {currentAttack:F1}");
                break;
        }
    }

    private void TakeEnemyDamage(float rawDamage)
    {
        // Shield absorbs first
        float remaining = rawDamage;
        if (currentShield > 0f)
        {
            float absorbed = Mathf.Min(currentShield, remaining);
            currentShield -= absorbed;
            remaining     -= absorbed;
            OnShieldChanged?.Invoke(currentShield);
        }

        // Flat defense reduces remaining damage — min 1 always gets through
        if (remaining > 0f)
        {
            float reduced = Mathf.Max(1f, remaining - currentDefense);
            currentHP     = Mathf.Max(0f, currentHP - reduced);
            OnHPChanged?.Invoke(currentHP, maxHP);
            OnDamageTaken?.Invoke(reduced);

            // Clutch move tracking
            if (currentHP <= maxHP * 0.1f)
                CombatStats.Instance?.RecordClutchMove();

            if (currentHP <= 0f) Defeat();
        }
    }
    #endregion

    #region Receive Combat Effects (Shield + Energy from tile matches)
    /// <summary>Call this from CombatResolver.OnEffectResolved for shield and energy effects.</summary>
    public void ApplyCombatEffect(CombatEffect effect)
    {
        switch (effect.SourceType)
        {
            case TileType.Defense:
                currentShield += effect.ShieldGenerated;
                OnShieldChanged?.Invoke(currentShield);
                break;

            case TileType.Energy:
                AddEnergy(effect.EnergyGenerated);
                break;
        }
    }
    #endregion

    #region Energy & Ultimate
    private void AddEnergy(float amount)
    {
        if (ultimateReady) return; // already full
        currentEnergy = Mathf.Min(currentEnergy + amount, MaxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy);

        if (currentEnergy >= MaxEnergy && !ultimateReady)
        {
            ultimateReady = true;
            OnUltimateReady?.Invoke(true);
            Debug.Log("[Character] Ultimate ready.");
        }
    }

    /// <summary>
    /// Player presses the ultimate button. Queues it to fire on the next match wave.
    /// </summary>
    public void QueueUltimate()
    {
        if (!ultimateReady || isDefeated) return;
        ultimateActiveNextWave = true;
        Debug.Log($"[Character] {data.ultimateName} queued.");
    }

    /// <summary>
    /// Called by UltimateSystem before damage wave to check if ultimate multiplier applies.
    /// Consumes the ultimate if queued.
    /// </summary>
    public float ConsumeUltimateMultiplier()
    {
        if (!ultimateActiveNextWave) return 1f;
        ultimateActiveNextWave = false;
        ultimateReady          = false;
        currentEnergy          = 0f;
        OnEnergyChanged?.Invoke(currentEnergy, MaxEnergy);
        OnUltimateReady?.Invoke(false);
        CombatStats.Instance?.RecordUltimate();
        Debug.Log($"[Character] {data?.ultimateName} activated!");
        return data != null ? data.ultimateDamageMultiplier : 1f;
    }
    #endregion

    #region Stats Feed
    /// <summary>
    /// After each wave resolves, restore debuffed attack toward base.
    /// Simple recovery — 10% of the difference per wave.
    /// </summary>
    private void FeedStatsToResolver()
    {
        float baseAtk = data != null
            ? data.baseAttack + data.attackPerLevel * (currentLevel - 1)
            : 10f;
        currentAttack = Mathf.Lerp(currentAttack, baseAtk, 0.1f);
    }
    #endregion

    #region XP & Level Up
    /// <summary>Call at battle end with XP reward from defeated enemy.</summary>
    public void AwardXP(int amount)
    {
        if (data == null) return;
        currentXP += amount;

        int xpNeeded = XPToNextLevel();
        while (xpNeeded > 0 && currentXP >= xpNeeded && currentLevel < MaxLevel())
        {
            currentXP    -= xpNeeded;
            currentLevel++;
            OnLevelUp?.Invoke(currentLevel);
            Debug.Log($"[Character] Level up → {currentLevel}");

            // Recalculate stats
            maxHP          = data.baseMaxHP      + data.hpPerLevel      * (currentLevel - 1);
            currentHP      = Mathf.Min(currentHP + data.hpPerLevel, maxHP); // partial heal
            currentAttack  = data.baseAttack     + data.attackPerLevel  * (currentLevel - 1);
            currentDefense = data.baseDefense    + data.defensePerLevel * (currentLevel - 1);
            currentLuck    = data.baseLuck       + data.luckPerLevel    * (currentLevel - 1);

            OnHPChanged?.Invoke(currentHP, maxHP);
            xpNeeded = XPToNextLevel();
        }

        OnXPChanged?.Invoke(currentXP, xpNeeded);
    }

    public int XPToNextLevel()
    {
        if (data == null || currentLevel - 1 >= data.xpPerLevel.Length) return 0;
        return data.xpPerLevel[currentLevel - 1];
    }

    private int MaxLevel() =>
        data != null ? data.xpPerLevel.Length + 1 : 10;
    #endregion

    #region Healing (for boosters, Skill 16)
    /// <summary>Restore HP up to max. Used by heal boosters.</summary>
    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    /// <summary>Add shield directly. Used by external systems.</summary>
    public void AddShield(float amount)
    {
        currentShield += amount;
        OnShieldChanged?.Invoke(currentShield);
    }
    #endregion

    #region Defeat
    private void Defeat()
    {
        isDefeated = true;
        Debug.Log("[Character] Player defeated.");
        OnPlayerDefeated?.Invoke();
    }
    #endregion
}
