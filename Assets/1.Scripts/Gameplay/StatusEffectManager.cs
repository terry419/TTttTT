using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 내 모든 캐릭터(플레이어, 몬스터)의 상태 효과(버프, 디버프)를 관리하는 싱글톤 클래스입니다.
/// 특정 대상에게 상태 효과를 적용하고, 지속 시간을 추적하며, 시간이 다 되면 자동으로 제거하는 역할을 합니다.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }

    // 현재 활성화된 모든 상태 효과를 추적하는 딕셔너리입니다.
    // Key: 효과가 적용된 대상 GameObject
    // Value: 해당 대상에게 적용된 상태 효과 목록
    private readonly Dictionary<GameObject, List<StatusEffect>> activeEffects = new Dictionary<GameObject, List<StatusEffect>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // 매 프레임 모든 활성 효과의 지속 시간을 갱신합니다.
        // 성능 최적화를 위해 코루틴 기반으로 변경할 수도 있습니다.
        if (activeEffects.Count == 0) return;

        // 순회 중 컬렉션 변경을 피하기 위해 제거할 효과 목록을 따로 관리합니다.
        List<StatusEffect> effectsToRemove = new List<StatusEffect>();

        foreach (var entry in activeEffects)
        {
            List<StatusEffect> effects = entry.Value;
            // 리스트를 역순으로 순회하여 제거 시 인덱스 문제를 방지합니다.
            for (int i = effects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = effects[i];
                effect.duration -= Time.deltaTime;
                if (effect.duration <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }

        // 시간이 다 된 효과들을 제거합니다.
        foreach (var effect in effectsToRemove)
        {
            RemoveStatusEffect(effect);
        }
    }

    /// <summary>
    /// 특정 대상에게 상태 효과를 적용합니다.
    /// </summary>
    /// <param name="target">효과를 적용할 대상</param>
    /// <param name="effectData">적용할 효과의 데이터(SO)</param>
    public void ApplyStatusEffect(GameObject target, StatusEffectDataSO effectData)
    {
        if (target == null || effectData == null) return;

        StatusEffect newEffect = new StatusEffect(target, effectData);

        if (!activeEffects.ContainsKey(target))
        {
            activeEffects[target] = new List<StatusEffect>();
        }

        activeEffects[target].Add(newEffect);
        newEffect.ApplyEffect(); // 효과의 시작 로직 실행
        Debug.Log($"[StatusEffect] {target.name}에게 {effectData.name} 효과 적용 (지속시간: {effectData.duration}초)");
    }

    /// <summary>
    /// 특정 상태 효과를 대상으로부터 제거합니다.
    /// </summary>
    private void RemoveStatusEffect(StatusEffect effect)
    {
        if (effect == null || !activeEffects.ContainsKey(effect.target)) return;

        effect.RemoveEffect(); // 효과의 종료 로직 실행
        activeEffects[effect.target].Remove(effect);

        // 대상에게 더 이상 적용중인 효과가 없다면 딕셔너리에서 키를 제거합니다.
        if (activeEffects[effect.target].Count == 0)
        {
            activeEffects.Remove(effect.target);
        }
        Debug.Log($"[StatusEffect] {effect.target.name}의 {effect.effectData.name} 효과 종료");
    }
}

/// <summary>
/// 활성화된 개별 상태 효과의 인스턴스 정보를 담는 클래스입니다.
/// </summary>
public class StatusEffect
{
    public GameObject target; // 효과 대상
    public StatusEffectDataSO effectData; // 효과 원본 데이터
    public float duration; // 남은 지속 시간

    public StatusEffect(GameObject target, StatusEffectDataSO effectData)
    {
        this.target = target;
        this.effectData = effectData;
        this.duration = effectData.duration;
    }

    public void ApplyEffect()
    {
        // TODO: 효과 적용 시점의 로직 (예: CharacterStats의 비율 값 변경)
        // CharacterStats stats = target.GetComponent<CharacterStats>();
        // if (stats != null) { stats.buffDamageRatio += effectData.damageRatioBonus; }
    }

    public void RemoveEffect()
    {
        // TODO: 효과 종료 시점의 로직 (예: 변경했던 스탯 원상복구)
        // CharacterStats stats = target.GetComponent<CharacterStats>();
        // if (stats != null) { stats.buffDamageRatio -= effectData.damageRatioBonus; }
    }
}

// StatusEffectDataSO는 ScriptableObject로 별도 정의되어야 합니다.
// 예: public class StatusEffectDataSO : ScriptableObject { ... }
