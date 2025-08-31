// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/StatusEffectManager.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// [v2.2] 게임 내 모든 캐릭터의 상태 효과를 관리하는 중앙 관리자입니다.
/// SO기반의 정적 효과와, 카드 모듈로부터 실시간으로 생성되는 동적 효과를 모두 처리하며 하위 호환성을 보장합니다.
/// </summary>

public class StatusEffectManager : MonoBehaviour
{
    // 현재 활성화된 모든 효과를 타겟별로 그룹화하여 저장합니다.
    private readonly Dictionary<GameObject, List<StatusEffectInstance>> activeEffects = new Dictionary<GameObject, List<StatusEffectInstance>>();
    // 매 프레임 제거할 효과를 임시 저장하는 리스트 (GC 최적화)
    private readonly List<StatusEffectInstance> effectsToRemove = new List<StatusEffectInstance>();
    // 매 프레임 정리할 타겟을 임시 저장하는 리스트 (GC 최적화)
    private readonly List<GameObject> targetsToRemove = new List<GameObject>();
    void Awake()
    {
        // ServiceLocator에 자기 자신을 등록하여 다른 곳에서 쉽게 접근할 수 있도록 합니다.
        ServiceLocator.Register<StatusEffectManager>(this);
    }
    /// <summary>
    /// [호환성 보장] 기존 StatusEffectDataSO 기반의 효과를 적용합니다.
    /// 내부적으로 SO를 새로운 StatusEffectInstance로 변환하여 신규 시스템에 전달합니다.
    /// </summary>
    public void ApplyStatusEffect(GameObject target, StatusEffectDataSO effectData)
    {
        Debug.Log($"[StatusEffectManager] (Legacy) SO기반 효과 '{effectData.name}' 적용 요청. 신규 인스턴스로 변환합니다.");
        var instance = new StatusEffectInstance(target, effectData);
        ApplyStatusEffect(target, instance); // 최종적으로는 신규 메서드를 호출
    }
    /// <summary>
    /// [핵심] 신규 카드 모듈에서 직접 생성한 동적 효과 인스턴스를 적용합니다.
    /// </summary>
    public void ApplyStatusEffect(GameObject target, StatusEffectInstance effectInstance)
    {
        if (target == null || effectInstance == null)
        {
            Debug.LogWarning($"[StatusEffectManager] Target 또는 EffectInstance가 null이므로 효과 적용을 중단합니다.");
            return;
        }
        // 이 타겟에 처음 효과가 적용되는 경우, 리스트를 새로 생성합니다.
        if (!activeEffects.ContainsKey(target))
        {
            activeEffects[target] = new List<StatusEffectInstance>();
        }
        // 동일한 ID의 효과가 이미 있는지 확인합니다.
        var existingEffect = activeEffects[target].FirstOrDefault(e => e.EffectId == effectInstance.EffectId);
        if (existingEffect != null)
        {
            // 중첩 정책에 따라 다르게 처리합니다.
            switch (effectInstance.StackingBehavior)
            {
                case StackingBehavior.RefreshDuration:
                    existingEffect.RefreshDuration();
                    Debug.Log($"[StatusEffect] '{target.name}'의 '{effectInstance.EffectId}' 효과 지속시간을 갱신했습니다.");
                    return; // 갱신만 하고 종료
                case StackingBehavior.NoStack:
                    Debug.Log($"[StatusEffect] '{target.name}'에 '{effectInstance.EffectId}' 효과가 이미 존재하며 중첩이 불가능하여 무시합니다.");
                    return; // 적용하지 않고 종료
                case StackingBehavior.StackEffect:
                    // 그냥 아래로 넘어가서 새 효과를 추가합니다.
                    break;
            }
        }
        // 새 효과를 리스트에 추가하고 적용 로직을 실행합니다.
        activeEffects[target].Add(effectInstance);
        effectInstance.ApplyEffect();
        Debug.Log($"<color=cyan>[StatusEffect]</color> '{target.name}'에게 '{effectInstance.EffectId}' 효과 적용 완료. 현재 효과 수: {activeEffects[target].Count}개");
    }
    /// <summary>
    /// 매 프레임 모든 활성 효과를 업데이트하고, 만료된 효과나 사라진 타겟을 정리합니다.
    /// </summary>
    void Update()
    {
        if (activeEffects.Count == 0) return;
        // 루프 시작 전, 임시 리스트들을 비웁니다.
        effectsToRemove.Clear();
        targetsToRemove.Clear();
        // 1. 모든 타겟과 그에 속한 효과들을 순회합니다.
        foreach (var entry in activeEffects)
        {
            GameObject target = entry.Key;
            // 타겟이 파괴되었거나 비활성화된 경우, 정리 목록에 추가합니다.
            if (target == null || !target.activeInHierarchy)
            {
                targetsToRemove.Add(target);
                continue;
            }
            var effectsOnTarget = entry.Value;
            // 리스트를 역순으로 순회해야 중간에 아이템이 제거되어도 안전합니다.
            for (int i = effectsOnTarget.Count - 1; i >= 0; i--)
            {
                var effect = effectsOnTarget[i];
                effect.Tick(Time.deltaTime); // 효과의 시간 흐름 처리
                // 효과가 만료되었다면, 제거 목록에 추가합니다.
                if (effect.IsExpired)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }
        // 2. 만료된 것으로 표시된 모든 효과를 실제로 제거합니다.
        foreach (var effect in effectsToRemove)
        {
            Debug.Log($"<color=yellow>[StatusEffect]</color> '{effect.Target.name}'의 '{effect.EffectId}' 효과가 만료되어 제거합니다.");
            RemoveStatusEffect(effect);
        }
        // 3. 파괴되거나 비활성화된 타겟과 관련된 모든 데이터를 정리합니다.
        foreach (var target in targetsToRemove)
        {
            if (activeEffects.TryGetValue(target, out var effects))
            {
                Debug.LogWarning($"[StatusEffectManager] 타겟 '{target.name}'이 비활성화/파괴되어, 남은 효과 {effects.Count}개를 모두 정리합니다.");
                // 타겟이 사라졌으므로, 각 효과의 제거 로직(스탯 복구 등)을 호출합니다.
                foreach (var effect in effects.ToList()) // ToList()로 사본을 만들어 안전하게 순회
                {
                    effect.RemoveEffect();
                }
                // 최종적으로 딕셔너리에서 해당 타겟 항목을 삭제합니다.
                activeEffects.Remove(target);
            }
        }
    }
    /// <summary>
    /// 특정 상태 효과 인스턴스를 대상의 목록에서 제거합니다.
    /// </summary>
    private void RemoveStatusEffect(StatusEffectInstance effect)
    {
        if (effect?.Target == null) return;
        if (activeEffects.TryGetValue(effect.Target, out var effectList))
        {
            effect.RemoveEffect(); // 스탯 복구 등 효과 제거 로직 실행
            effectList.Remove(effect);
        }
    }
    // --- Public API ---
    public List<StatusEffectInstance> GetActiveEffectsOn(GameObject target) => activeEffects.TryGetValue(target, out var e) ? new List<StatusEffectInstance>(e) : new List<StatusEffectInstance>();
    public bool HasStatusEffect(GameObject target, string effectId) => !string.IsNullOrEmpty(effectId) && activeEffects.TryGetValue(target, out var e) && e.Any(inst => inst.EffectId == effectId);
    public void ConsumeEffects(List<StatusEffectInstance> effectsToConsume) => effectsToConsume.ForEach(RemoveStatusEffect);
}

