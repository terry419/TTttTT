using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ToList() 사용을 위해 추가

/// <summary>
/// 게임 내 모든 캐릭터(플레이어, 몬스터)의 상태 효과(버프, 디버프)를 관리하는 싱글톤 클래스입니다.
/// 특정 대상에게 상태 효과를 적용하고, 지속 시간을 추적하며, 시간이 다 되면 자동으로 제거하는 역할을 합니다.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }

    private readonly Dictionary<GameObject, List<StatusEffect>> activeEffects = new Dictionary<GameObject, List<StatusEffect>>();
    
    // Update에서 순회 중 변경을 피하기 위해 사용할 임시 리스트
    private readonly List<StatusEffect> effectsToRemove = new List<StatusEffect>();
    private readonly List<GameObject> targetsToRemove = new List<GameObject>();

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
        if (activeEffects.Count == 0) return;

        effectsToRemove.Clear();
        targetsToRemove.Clear();

        // 1. 만료된 이펙트들을 찾아서 effectsToRemove 리스트에 추가합니다.
        foreach (var entry in activeEffects)
        {
            List<StatusEffect> effectsOnTarget = entry.Value;
            // 리스트를 역순으로 순회해야 안전하게 중간에 아이템을 제거할 수 있습니다.
            for (int i = effectsOnTarget.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = effectsOnTarget[i];
                effect.duration -= Time.deltaTime;
                if (effect.duration <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }

        // 2. 만료된 이펙트들을 실제 activeEffects 딕셔너리에서 제거합니다.
        if (effectsToRemove.Count > 0)
        {
            foreach (StatusEffect effect in effectsToRemove)
            {
                RemoveStatusEffect(effect);
            }
        }

        // 3. 효과가 하나도 남지 않은 대상(GameObject)을 딕셔너리에서 제거합니다.
        foreach (var entry in activeEffects)
        {
            if (entry.Value.Count == 0)
            {
                targetsToRemove.Add(entry.Key);
            }
        }

        if (targetsToRemove.Count > 0)
        {
            foreach (GameObject target in targetsToRemove)
            {
                activeEffects.Remove(target);
            }
        }
    }

    /// <summary>
    /// 특정 대상에게 상태 효과를 적용합니다.
    /// </summary>
    public void ApplyStatusEffect(GameObject target, StatusEffectDataSO effectData)
    {
        if (target == null || effectData == null) return;

        StatusEffect newEffect = new StatusEffect(target, effectData);

        if (!activeEffects.ContainsKey(target))
        {
            activeEffects[target] = new List<StatusEffect>();
        }

        activeEffects[target].Add(newEffect);
        newEffect.ApplyEffect();
        Debug.Log($"[StatusEffect] {target.name}에게 {effectData.name} 효과 적용 (지속시간: {effectData.duration}초)");
    }

    /// <summary>
    /// 특정 상태 효과 인스턴스를 제거합니다.
    /// </summary>
    private void RemoveStatusEffect(StatusEffect effect)
    {
        if (effect == null || effect.target == null) return;

        // 대상의 효과 목록에서 해당 효과를 제거합니다.
        if (activeEffects.TryGetValue(effect.target, out var effectList))
        {
            effect.RemoveEffect(); // 능력치 원상복구
            effectList.Remove(effect);
            Debug.Log($"[StatusEffect] {effect.target.name}의 {effect.effectData.name} 효과 종료");
        }
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
        CharacterStats stats = target.GetComponent<CharacterStats>();
        if (stats != null)
        {
            effectData.ApplyEffect(stats); // StatusEffectDataSO의 ApplyEffect 호출
        }
    }

    public void RemoveEffect()
    {
        CharacterStats stats = target.GetComponent<CharacterStats>();
        if (stats != null)
        {
            effectData.RemoveEffect(stats); // StatusEffectDataSO의 RemoveEffect 호출
        }
    }
}