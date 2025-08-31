// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/StatusEffectInstance.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Linq; // FirstOrDefault 사용을 위해 추가

/// <summary>
/// 상태 효과의 중첩 방식을 정의하는 열거형.
/// </summary>
public enum StackingBehavior
{
    [Tooltip("기존 효과의 지속시간만 초기화합니다.")]
    RefreshDuration,
    [Tooltip("별개의 효과로 새로 추가하여 중첩시킵니다.")]
    StackEffect,
    [Tooltip("이미 효과가 있다면 무시합니다.")]
    NoStack
}

/// <summary>
/// 지속 피해량의 계산 방식을 정의하는 열거형.
/// </summary>
public enum DamageType { Flat, MaxHealthPercentage }

/// <summary>
/// 회복량의 계산 방식을 정의하는 열거형.
/// </summary>
public enum HealType { Flat, MaxHealthPercentage }

/// <summary>
/// 활성화된 개별 상태 효과의 모든 데이터를 담는 인스턴스 클래스.
/// ScriptableObject가 '설계도'라면, 이 클래스는 '실제품'에 해당합니다.
/// </summary>
public class StatusEffectInstance
{
    #region 필드와 프로퍼티
    // --- 주요 속성 ---
    public GameObject Target { get; }
    public string EffectId { get; }
    public StackingBehavior StackingBehavior { get; }

    // --- 시간 관련 ---
    private readonly float initialDuration;
    private float duration;
    public bool IsExpired => duration > 0 && duration <= Time.deltaTime; // 1프레임 오차 방지
    public float RemainingDuration => duration;

    // --- 효과 내용 (스탯) ---
    private readonly Dictionary<StatType, float> statBonuses;

    // --- 효과 내용 (지속 피해) ---
    private readonly float dotAmount;
    private readonly DamageType damageType;
    public float DamagePerSecond
    {
        get
        {
            if (Target == null) return 0f;
            float baseAmount = dotAmount;

            // 최대 체력 비례 데미지 계산
            if (damageType == DamageType.MaxHealthPercentage)
            {
                if (Target.TryGetComponent<MonsterStats>(out var monster))
                    baseAmount = monster.FinalMaxHealth * (dotAmount / 100f);
                else if (Target.TryGetComponent<CharacterStats>(out var player))
                    baseAmount = player.FinalHealth * (dotAmount / 100f);
            }

            // 시전자 공격력 비례 데미지 계산
            if (scalesWithDmgBonus && casterStats != null)
                baseAmount *= (1 + casterStats.FinalDamageBonus / 100f);

            return baseAmount;
        }
    }

    // --- 효과 내용 (회복) ---
    private readonly float totalHealAmount;
    private readonly float healDuration;
    private readonly HealType healType;

    // --- 스케일링 및 시전자 정보 ---
    private readonly bool scalesWithDmgBonus;
    private readonly CharacterStats casterStats;

    // --- VFX 정보 ---
    private readonly AssetReferenceGameObject onApplyVFXRef;
    private readonly AssetReferenceGameObject loopingVFXRef;
    private readonly AssetReferenceGameObject onExpireVFXRef;
    private GameObject loopingVFXInstance;
    #endregion

    #region 생성자
    /// <summary>
    /// [v2.2] 신규 카드 모듈로부터 동적으로 인스턴스를 생성하는 주 생성자.
    /// </summary>
    public StatusEffectInstance(GameObject target, string id, float duration, Dictionary<StatType, float> bonuses,
        float dotAmount, DamageType dotType, bool scales,
        float healAmount, float healDuration, HealType healType,
        StackingBehavior stacking, CharacterStats caster,
        AssetReferenceGameObject onApplyVFX, AssetReferenceGameObject loopingVFX, AssetReferenceGameObject onExpireVFX)
    {
        this.Target = target;
        this.EffectId = id;
        this.initialDuration = duration;
        this.duration = duration;
        this.statBonuses = bonuses ?? new Dictionary<StatType, float>();

        this.dotAmount = dotAmount;
        this.damageType = dotType;
        this.scalesWithDmgBonus = scales;
        this.casterStats = caster;

        this.totalHealAmount = healAmount;
        this.healDuration = healDuration;
        this.healType = healType;

        this.StackingBehavior = stacking;

        this.onApplyVFXRef = onApplyVFX;
        this.loopingVFXRef = loopingVFX;
        this.onExpireVFXRef = onExpireVFX;
    }

    /// <summary>
    /// [호환성] StatusEffectDataSO로부터 인스턴스를 생성하는 생성자.
    /// </summary>
    public StatusEffectInstance(GameObject target, StatusEffectDataSO data, CharacterStats caster = null)
    {
        Target = target;
        casterStats = caster;
        EffectId = data.effectId;
        initialDuration = data.duration;
        duration = data.duration;

        dotAmount = data.damageOverTime;
        damageType = DamageType.Flat; // 레거시는 Flat만 지원

        totalHealAmount = data.healOverTime; // 레거시는 초당 회복이므로, 지속시간 곱셈 제거
        healDuration = 1f; // 1초마다 회복하도록 설정
        healType = HealType.Flat; // 레거시는 Flat만 지원

        StackingBehavior = StackingBehavior.RefreshDuration;
        statBonuses = new Dictionary<StatType, float>();
        if (data.damageRatioBonus != 0) statBonuses[StatType.Attack] = data.damageRatioBonus;
        if (data.attackSpeedRatioBonus != 0) statBonuses[StatType.AttackSpeed] = data.attackSpeedRatioBonus;
        if (data.moveSpeedRatioBonus != 0) statBonuses[StatType.MoveSpeed] = data.moveSpeedRatioBonus;
        if (data.healthRatioBonus != 0) statBonuses[StatType.Health] = data.healthRatioBonus;
        if (data.critRateRatioBonus != 0) statBonuses[StatType.CritRate] = data.critRateRatioBonus;
        if (data.critDamageRatioBonus != 0) statBonuses[StatType.CritMultiplier] = data.critDamageRatioBonus;
    }
    #endregion

    #region 핵심 로직 (적용, 제거, 틱)
    /// <summary>
    /// 대상에게 효과를 적용합니다. (스탯, 즉발회복, VFX)
    /// </summary>
    public void ApplyEffect()
    {
        // 1. 스탯 적용 (IStatHolder 인터페이스 사용)
        if (Target.TryGetComponent<IStatHolder>(out var statHolder))
        {
            foreach (var bonus in statBonuses)
            {
                statHolder.AddModifier(bonus.Key, new StatModifier(bonus.Value, this));
            }
        }

        // 2. 즉발 회복 처리 (지속시간이 0일 경우)
        if (totalHealAmount > 0 && healDuration <= 0)
        {
            ApplyHeal(totalHealAmount);
        }

        // 3. VFX 재생
        PlayVFX(onApplyVFXRef, Target.transform.position, false);
        PlayVFX(loopingVFXRef, Target.transform.position, true, Target.transform);
    }

    /// <summary>
    /// 대상에게서 효과를 제거합니다. (스탯 복구, VFX 정리)
    /// </summary>
    public void RemoveEffect()
    {
        // 1. 스탯 복구 (IStatHolder 인터페이스 사용)
        if (Target != null && Target.TryGetComponent<IStatHolder>(out var statHolder))
        {
            statHolder.RemoveModifiersFromSource(this);
        }

        // 2. 루핑 VFX 중지 및 소멸 VFX 재생
        if (loopingVFXInstance != null)
        {
            ServiceLocator.Get<PoolManager>()?.Release(loopingVFXInstance);
            loopingVFXInstance = null;
        }
        PlayVFX(onExpireVFXRef, Target.transform.position, false);
    }

    /// <summary>
    /// 매 프레임 호출되어 지속시간, DoT, HoT 등을 처리합니다.
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (duration > 0) duration -= deltaTime;

        // 지속 피해 처리 (1초마다)
        if (dotAmount > 0)
        {
            // DoT는 MonsterController와 CharacterStats 양쪽 모두에 적용될 수 있음
            Target.GetComponent<MonsterController>()?.TakeDamage(DamagePerSecond * deltaTime);
            Target.GetComponent<CharacterStats>()?.TakeDamage(DamagePerSecond * deltaTime);
        }

        // 지속 회복 처리
        if (totalHealAmount > 0 && healDuration > 0)
        {
            ApplyHeal(totalHealAmount * (deltaTime / healDuration));
        }
    }
    #endregion

    #region 헬퍼 메서드
    /// <summary>
    /// 효과의 지속시간을 초기값으로 되돌립니다.
    /// </summary>
    public void RefreshDuration() => duration = initialDuration;

    private void ApplyHeal(float amount)
    {
        if (Target == null) return;

        float finalHeal = amount;
        if (healType == HealType.MaxHealthPercentage)
        {
            if (Target.TryGetComponent<CharacterStats>(out var stats))
                finalHeal = stats.FinalHealth * (amount / 100f);
            // 몬스터 회복은 현재 기획에 없으므로 플레이어만 처리
        }

        Target.GetComponent<CharacterStats>()?.Heal(finalHeal);
    }

    private async void PlayVFX(AssetReferenceGameObject vfxRef, Vector3 position, bool isLoop, Transform parent = null)
    {
        if (vfxRef == null || !vfxRef.RuntimeKeyIsValid()) return;
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        GameObject vfxInstance = await poolManager.GetAsync(vfxRef.AssetGUID);
        if (vfxInstance != null)
        {
            vfxInstance.transform.position = position;
            if (parent != null) vfxInstance.transform.SetParent(parent, true);

            if (isLoop)
            {
                loopingVFXInstance = vfxInstance;
            }
        }
    }
    #endregion
}