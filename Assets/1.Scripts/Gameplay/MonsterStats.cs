// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterStats.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;

[RequireComponent(typeof(MonsterController))]
public class MonsterStats : MonoBehaviour, IStatHolder
{
    // --- 이벤트 방송 ---
    public static event Action<float, Vector3> OnMonsterDamaged;

    private MonsterController controller;
    private MonsterDataSO monsterData;

    // --- 능력치 ---
    private float baseMaxHealth;
    private float baseMoveSpeed;
    private float baseContactDamage;
    public float CurrentHealth { get; private set; }
    private readonly Dictionary<StatType, List<StatModifier>> statModifiers = new Dictionary<StatType, List<StatModifier>>();

    public float FinalMaxHealth => CalculateFinalValue(StatType.Health, baseMaxHealth);
    public float FinalMoveSpeed => Mathf.Max(0f, CalculateFinalValue(StatType.MoveSpeed, baseMoveSpeed));
    public float FinalContactDamage => Mathf.Max(0f, CalculateFinalValue(StatType.ContactDamage, baseContactDamage));
    public float FinalDamageTakenBonus => statModifiers.ContainsKey(StatType.DamageTaken) ? statModifiers[StatType.DamageTaken].Sum(mod => mod.Value) : 0f;

    void Awake()
    {
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            statModifiers[type] = new List<StatModifier>();
        }
    }

    public void Initialize(MonsterController owner, MonsterDataSO data)
    {
        this.controller = owner;
        this.monsterData = data;
        baseMaxHealth = data.maxHealth;
        baseMoveSpeed = data.moveSpeed;
        baseContactDamage = data.contactDamage;
        CurrentHealth = FinalMaxHealth;
    }

    public void TakeDamage(float damage)
    {
        float damageMultiplier = 1 + (FinalDamageTakenBonus / 100f);
        float finalDamage = damage * damageMultiplier;
        CurrentHealth -= finalDamage;

        OnMonsterDamaged?.Invoke(finalDamage, transform.position); // 데미지 UI를 위해 방송

        if (CurrentHealth <= 0)
        {
            HandleDeath().Forget();
        }
    }

    private async UniTaskVoid HandleDeath()
    {
        // 사망 시 장판 생성 로직
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

        // 사망 시 자폭 로직
        if (monsterData.canExplodeOnDeath)
        {
            await PlayerTargetedExplosionAsync(transform.position, monsterData);
            if (this == null) return;
        }

        // 모든 사망 효과가 처리된 후, Controller에게 죽음을 최종 통보
        controller.Die();
    }

    private static async UniTask PlayerTargetedExplosionAsync(Vector3 deathPosition, MonsterDataSO monsterData)
    {
        // (이 함수 내용은 이전과 동일하여 생략)
    }

    #region IStatHolder Implementation and Helpers
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
    #endregion
}