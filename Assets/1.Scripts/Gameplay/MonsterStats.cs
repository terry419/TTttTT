// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterStats.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;

[RequireComponent(typeof(MonsterController))]
public class MonsterStats : MonoBehaviour, IStatHolder
{
    public static event Action<float, Vector3> OnMonsterDamaged;

    private MonsterController controller;
    private MonsterDataSO monsterData;

    private float baseMaxHealth;
    private float baseMoveSpeed;
    private float baseContactDamage;
    public float CurrentHealth { get; private set; }
    private readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();

    public float FinalMaxHealth => CalculateFinalValue(StatType.Health, baseMaxHealth);
    public float FinalMoveSpeed => Mathf.Max(0f, CalculateFinalValue(StatType.MoveSpeed, baseMoveSpeed));
    public float FinalContactDamage => Mathf.Max(0f, CalculateFinalValue(StatType.ContactDamage, baseContactDamage));
    public float FinalDamageTakenBonus => statModifiers.ContainsKey(StatType.DamageTaken) ? statModifiers[StatType.DamageTaken].Sum(mod => mod.Value) : 0f;

    public void Initialize(MonsterController owner, MonsterDataSO data)
    {
        this.controller = owner;
        this.monsterData = data;
        baseMaxHealth = data.maxHealth;
        baseMoveSpeed = data.moveSpeed;
        baseContactDamage = data.contactDamage;
        CurrentHealth = FinalMaxHealth;

        // 모든 효과 초기화
        foreach (var list in statModifiers.Values) { list.Clear(); }
    }

    public void TakeDamage(float damage)
    {
        float damageMultiplier = 1 + (FinalDamageTakenBonus / 100f);
        float finalDamage = damage * damageMultiplier;
        CurrentHealth -= finalDamage;

        OnMonsterDamaged?.Invoke(finalDamage, transform.position);

        if (CurrentHealth <= 0)
        {
            HandleDeath().Forget();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, FinalMaxHealth);
    }

    public async UniTaskVoid HandleDeath()
    {
        if (monsterData.onDeathZoneEffect != null)
        {
            var player = ServiceLocator.Get<PlayerController>()?.GetComponent<CharacterStats>();
            if (player != null)
            {
                var context = new EffectContext { Caster = player, SpawnPoint = this.transform, HitPosition = this.transform.position };
                monsterData.onDeathZoneEffect.Execute(context);
                Log.Info(Log.LogCategory.AI_Behavior, $"{name}이(가) 사망하며 '{monsterData.onDeathZoneEffect.name}' 장판을 생성했습니다.");
            }
        }

        if (monsterData.canExplodeOnDeath)
        {
            await PlayerTargetedExplosionAsync(transform.position, monsterData);
            if (this == null) return;
        }

        controller.Die();
    }

    private static async UniTask PlayerTargetedExplosionAsync(Vector3 deathPosition, MonsterDataSO monsterData)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(monsterData.explosionDelay));
        var poolManager = ServiceLocator.Get<PoolManager>();
        var playerController = ServiceLocator.Get<PlayerController>();
        if (poolManager == null || playerController == null) return;
        GameObject vfxGO = await poolManager.GetAsync(monsterData.explosionVfxRef.AssetGUID);
        if (vfxGO != null)
        {
            vfxGO.transform.position = deathPosition;
        }
        float sqrDistanceToPlayer = (deathPosition - playerController.transform.position).sqrMagnitude;
        float explosionSqrRadius = monsterData.explosionRadius * monsterData.explosionRadius;
        if (sqrDistanceToPlayer <= explosionSqrRadius)
        {
            if (playerController.TryGetComponent<CharacterStats>(out var playerStats))
            {
                playerStats.TakeDamage(monsterData.explosionDamage);
            }
        }
    }

    public void ApplySelfStatusEffect(MonsterStatusEffectSO effectData)
    {
        if (effectData == null) return;
        var statusEffectManager = ServiceLocator.Get<StatusEffectManager>();
        if (statusEffectManager == null) return;

        var bonuses = new Dictionary<StatType, float>();
        if (effectData.moveSpeedBonus != 0) bonuses.Add(StatType.MoveSpeed, effectData.moveSpeedBonus);
        if (effectData.contactDamageBonus != 0) bonuses.Add(StatType.ContactDamage, effectData.contactDamageBonus);
        if (effectData.damageTakenBonus != 0) bonuses.Add(StatType.DamageTaken, effectData.damageTakenBonus);

        var instance = new StatusEffectInstance(
            this.gameObject, effectData.effectId, effectData.duration, bonuses,
            effectData.damageOverTime, DamageType.Flat, false,
            effectData.healOverTime, 1f, HealType.Flat,
            StackingBehavior.RefreshDuration, null, null, null, null
        );
        statusEffectManager.ApplyStatusEffect(this.gameObject, instance);
    }

    public void RemoveSelfStatusEffect(string effectId)
    {
        if (string.IsNullOrEmpty(effectId)) return;
        ServiceLocator.Get<StatusEffectManager>()?.RemoveStatusEffect(this.gameObject, effectId);
    }

    public void AddModifier(StatType type, StatModifier modifier)
    {
        if (!statModifiers.ContainsKey(type)) statModifiers[type] = new List<StatModifier>();
        statModifiers[type].Add(modifier);
    }
    public void RemoveModifiersFromSource(object source)
    {
        foreach (var list in statModifiers.Values)
        {
            list.RemoveAll(mod => mod.Source == source);
        }
    }
    private float CalculateFinalValue(StatType type, float baseValue)
    {
        float totalBonusRatio = statModifiers.ContainsKey(type) ? statModifiers[type].Sum(mod => mod.Value) : 0f;
        return baseValue * (1 + totalBonusRatio / 100f);
    }
}