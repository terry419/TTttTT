using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임 내 모든 캐릭터(플레이어, 몬스터)의 상태 효과(버프, 디버프)를 관리하는 싱글톤 클래스입니다.
/// 특정 대상에게 상태 효과를 적용하고, 지속 시간을 추적하며, 시간이 다 되면 자동으로 제거하는 역할을 합니다.
/// 지속 데미지(DoT)나 지속 회복(HoT) 효과도 이 스크립트의 Update 메서드에서 처리됩니다.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    // [자주 사용되는 변수들]
    private readonly Dictionary<GameObject, List<StatusEffect>> activeEffects = new Dictionary<GameObject, List<StatusEffect>>();
    private readonly List<StatusEffect> effectsToRemove = new List<StatusEffect>();
    private readonly List<GameObject> targetsToRemove = new List<GameObject>();

    // 활성화된 VFX를 중앙에서 관리. <대상, <효과ID, VFX인스턴스>>
    private readonly Dictionary<GameObject, Dictionary<string, GameObject>> activeVFX = new Dictionary<GameObject, Dictionary<string, GameObject>>();

    /// <summary>
    /// 스크립트가 처음 깨어날 때 호출됩니다.
    /// 자기 자신을 서비스 로케이터에 등록합니다.
    /// </summary>
    void Awake()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        ServiceLocator.Register<StatusEffectManager>(this);
        Debug.Log("[StatusEffectManager] 서비스 로케이터에 성공적으로 등록되었습니다.");
    }

    /// <summary>
    /// 매 프레임마다 호출되며, 모든 상태 효과의 지속시간을 감소시키고 지속 데미지 등을 처리합니다.
    /// </summary>
    void Update()
    {
        if (activeEffects.Count == 0) return;

        effectsToRemove.Clear();
        targetsToRemove.Clear();

        List<GameObject> activeTargets = activeEffects.Keys.ToList();

        foreach (GameObject target in activeTargets)
        {
            if (target == null || !activeEffects.ContainsKey(target)) continue;

            List<StatusEffect> effectsOnTarget = activeEffects[target];

            for (int i = effectsOnTarget.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = effectsOnTarget[i];

                ProcessDoT(effect); // 지속 피해 처리 로직 분리

                // 지속 시간 감소 및 만료 처리
                effect.duration -= Time.deltaTime;
                if (effect.duration <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }

        // 만료된 효과 제거
        foreach (StatusEffect effect in effectsToRemove)
        {
            RemoveStatusEffect(effect.target, effect);
        }

        // 효과가 하나도 없는 대상은 activeEffects 딕셔너리에서 제거
        foreach (GameObject target in activeTargets)
        {
            if (target != null && activeEffects.ContainsKey(target) && activeEffects[target].Count == 0)
            {
                targetsToRemove.Add(target);
            }
        }

        foreach (GameObject t in targetsToRemove)
        {
            if (t != null) activeEffects.Remove(t);
        }
    }

    /// <summary>
    /// 특정 대상에게 상태 효과를 적용합니다.
    /// </summary>
    public void ApplyStatusEffect(GameObject target, StatusEffectDataSO effectData)
    {
        if (target == null || effectData == null) return;

        if (!activeEffects.ContainsKey(target))
        {
            activeEffects[target] = new List<StatusEffect>();
            activeVFX[target] = new Dictionary<string, GameObject>();
        }

        bool isRefreshable = effectData.damageOverTime != null &&
                             effectData.damageOverTime.damageType == DamageOverTimeInfo.DamageType.PercentOfMaxHealth;

        if (isRefreshable)
        {
            StatusEffect existingEffect = activeEffects[target].FirstOrDefault(e => e.effectData.effectId == effectData.effectId);
            if (existingEffect != null)
            {
                Debug.Log($"[상태 효과 갱신] 대상: {target.name}, 효과: {effectData.effectName}, 지속시간: {effectData.duration}초로 초기화.");
                existingEffect.duration = effectData.duration;
                return;
            }
        }

        // --- 새 효과 적용 로직 ---
        Debug.Log($"[상태 효과 적용] 대상: {target.name}, 효과: {effectData.effectName} (중첩)");
        StatusEffect newEffect = new StatusEffect(target, effectData);

        newEffect.ApplyStatChanges(); // 스탯 변경 적용
        activeEffects[target].Add(newEffect);

        // VFX 생성 (해당 효과의 VFX가 아직 없는 경우에만)
        if (effectData.statusVFX != null && !activeVFX[target].ContainsKey(effectData.effectId))
        {
            GameObject vfxInstance = Instantiate(effectData.statusVFX, target.transform);
            activeVFX[target][effectData.effectId] = vfxInstance;
            Debug.Log($"[VFX 생성] 대상: {target.name}, 효과: {effectData.effectName}, VFX: {vfxInstance.name}");
        }
    }

    /// <summary>
    /// 특정 상태 효과 인스턴스를 제거하고 관련 리소스를 정리합니다.
    /// </summary>
    private void RemoveStatusEffect(GameObject target, StatusEffect effect)
    {
        if (effect == null || target == null) return;

        if (activeEffects.TryGetValue(target, out var effectList))
        {
            Debug.Log($"[상태 효과 종료] 대상: {target.name}, 효과: {effect.effectData.effectName}");
            effect.RemoveStatChanges(); // 스탯 원상복구
            effectList.Remove(effect);

            // 제거 후 동일한 효과의 다른 스택이 남아있는지 확인
            bool isLastStack = !effectList.Any(e => e.effectData.effectId == effect.effectData.effectId);

            // 마지막 스택이었다면 VFX도 제거
            if (isLastStack && activeVFX.ContainsKey(target) && activeVFX[target].ContainsKey(effect.effectData.effectId))
            {
                Debug.Log($"[VFX 파괴] 대상: {target.name}, 효과: {effect.effectData.effectName} (마지막 스택)");
                Destroy(activeVFX[target][effect.effectData.effectId]);
                activeVFX[target].Remove(effect.effectData.effectId);
            }
        }
    }

    /// <summary>
    /// 상태 효과의 지속 피해(DoT) 로직을 처리합니다.
    /// </summary>
    private void ProcessDoT(StatusEffect effect)
    {
        if (effect.effectData.damageOverTime == null) return;

        var dotInfo = effect.effectData.damageOverTime;
        if (effect.target.CompareTag("Monster"))
        {
            var monster = effect.target.GetComponentInChildren<MonsterController>();
            if (monster == null) return;

            float damageThisFrame = 0f;
            switch (dotInfo.damageType)
            {
                case DamageOverTimeInfo.DamageType.Fixed:
                    damageThisFrame = dotInfo.damageAmount * Time.deltaTime;
                    break;
                case DamageOverTimeInfo.DamageType.PercentOfMaxHealth:
                    damageThisFrame = monster.maxHealth * (dotInfo.percentOfMaxHealth / 100f) * Time.deltaTime;
                    break;
            }

            if (damageThisFrame > 0)
            {
                monster.TakeDamage(damageThisFrame);
            }
        }
    }
}

/// <summary>
/// 활성화된 개별 상태 효과의 인스턴스 정보를 담는 클래스입니다. (VFX 인스턴스 참조는 Manager가 중앙 관리)
/// </summary>
public class StatusEffect
{
    public GameObject target;
    public StatusEffectDataSO effectData;
    public float duration;

    public StatusEffect(GameObject target, StatusEffectDataSO effectData)
    {
        this.target = target;
        this.effectData = effectData;
        this.duration = effectData.duration;
    }

    public void ApplyStatChanges()
    {
        if (target.TryGetComponent<CharacterStats>(out var stats))
        {
            effectData.ApplyEffect(stats);
        }
    }

    public void RemoveStatChanges()
    {
        if (target.TryGetComponent<CharacterStats>(out var stats))
        {
            effectData.RemoveEffect(stats);
        }
    }
}