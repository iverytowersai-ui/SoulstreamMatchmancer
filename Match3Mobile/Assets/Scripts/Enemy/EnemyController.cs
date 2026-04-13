using System;
using UnityEngine;

/// <summary>
/// Manages a single enemy's live state during battle.
/// Receives damage from CombatResolver.OnEffectResolved,
/// applies defense/armor reduction, and fires defeat events.
/// </summary>
public class EnemyController : MonoBehaviour
{
    #region Singleton
    public static EnemyController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    #endregion

    #region Inspector Fields
    [SerializeField] private CombatResolver combatResolver;
    #endregion

    #region Runtime State
    private EnemyData data;
    private float     currentHP;
    private float     maxHP;
    private float     currentArmor;
    private float     currentDefense;
    private bool      isDefeated;

    // Status effects
    private bool      isPoisoned;
    private int       poisonTurnsRemaining;
    private float     poisonDamage;
    private bool      isVulnerable;
    private float     vulnerabilityMult = 1f;
    #endregion

    #region Properties
    public EnemyData  Data           => data;
    public float      CurrentHP      => currentHP;
    public float      MaxHP          => maxHP;
    public float      CurrentArmor   => currentArmor;
    public bool       IsDefeated     => isDefeated;
    public bool       IsPoisoned     => isPoisoned;
    public bool       IsVulnerable   => isVulnerable;
    #endregion

    #region Events
    public event Action<float, float> OnHPChanged;      // current, max
    public event Action<float>        OnArmorChanged;    // current armor
    public event Action               OnEnemyDefeated;
    public event Action<string>       OnStatusApplied;   // "Poison", "Vulnerable"
    #endregion

    #region Initialisation
    /// <summary>Call at the start of each battle.</summary>
    public void Initialise(EnemyData enemyData, int stageNumber = 1)
    {
        data            = enemyData;
        isDefeated      = false;
        isPoisoned      = false;
        isVulnerable    = false;
        vulnerabilityMult = 1f;
        poisonTurnsRemaining = 0;

        // Scale stats by stage
        float hpScale  = Mathf.Pow(data.hpScalePerStage, stageNumber - 1);
        maxHP          = data.maxHP * hpScale;
        currentHP      = maxHP;
        currentArmor   = data.armor;
        currentDefense = data.defense;

        OnHPChanged?.Invoke(currentHP, maxHP);
        OnArmorChanged?.Invoke(currentArmor);
    }
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        if (combatResolver != null)
            combatResolver.OnEffectResolved += HandleCombatEffect;
    }

    private void OnDisable()
    {
        if (combatResolver != null)
            combatResolver.OnEffectResolved -= HandleCombatEffect;
    }
    #endregion

    #region Combat Effect Handling
    private void HandleCombatEffect(CombatEffect effect)
    {
        if (isDefeated) return;

        switch (effect.SourceType)
        {
            case TileType.Damage:
                TakeDamage(effect.DamageDealt, effect.IsCriticalHit);
                break;

            case TileType.Break:
                TakeArmorDamage(effect.ArmorDamageDealt);
                break;

            case TileType.Debuff:
                if (effect.AppliesPoison)
                    ApplyPoison(effect.DebuffDuration);
                if (effect.AppliesVulnerability)
                    ApplyVulnerability();
                break;
        }
    }

    private void TakeDamage(float rawDamage, bool isCrit)
    {
        // Vulnerability amplifies incoming damage
        float damage = rawDamage * vulnerabilityMult;

        // Armor absorbs first
        if (currentArmor > 0f)
        {
            float absorbed = Mathf.Min(currentArmor, damage);
            currentArmor -= absorbed;
            damage       -= absorbed;
            OnArmorChanged?.Invoke(currentArmor);
        }

        // Defense reduces remaining damage (min 1 gets through)
        if (damage > 0f)
        {
            damage = Mathf.Max(1f, damage - currentDefense);
            currentHP = Mathf.Max(0f, currentHP - damage);
            OnHPChanged?.Invoke(currentHP, maxHP);

            if (currentHP <= 0f) Defeat();
        }
    }

    private void TakeArmorDamage(float armorDamage)
    {
        if (currentArmor <= 0f) return;
        currentArmor = Mathf.Max(0f, currentArmor - armorDamage);
        OnArmorChanged?.Invoke(currentArmor);
        Debug.Log($"[Enemy] Armor reduced to {currentArmor:F1}");
    }

    private void ApplyPoison(int duration)
    {
        isPoisoned           = true;
        poisonTurnsRemaining = duration;
        poisonDamage         = 5f; // Will be read from CombatConfig once wired
        OnStatusApplied?.Invoke("Poison");
        Debug.Log($"[Enemy] Poisoned for {duration} turns");
    }

    private void ApplyVulnerability()
    {
        isVulnerable      = true;
        vulnerabilityMult = 1.25f; // Will be read from CombatConfig once wired
        OnStatusApplied?.Invoke("Vulnerable");
        Debug.Log("[Enemy] Vulnerable — taking 25% more damage");
    }
    #endregion

    #region Poison Tick
    /// <summary>Call at the start of each player turn to tick poison.</summary>
    public void TickStatusEffects()
    {
        if (isPoisoned && poisonTurnsRemaining > 0)
        {
            currentHP = Mathf.Max(0f, currentHP - poisonDamage);
            poisonTurnsRemaining--;
            OnHPChanged?.Invoke(currentHP, maxHP);
            Debug.Log($"[Enemy] Poison tick: {poisonDamage:F1} damage, {poisonTurnsRemaining} turns left");

            if (poisonTurnsRemaining <= 0) isPoisoned = false;
            if (currentHP <= 0f) Defeat();
        }
    }
    #endregion

    #region Defeat
    private void Defeat()
    {
        isDefeated = true;
        Debug.Log($"[Enemy] {data?.enemyName ?? "Enemy"} defeated!");
        OnEnemyDefeated?.Invoke();
    }
    #endregion
}
