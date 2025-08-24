using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 게임 내 모든 캐릭터(플레이어, 몬스터)의 상태 효과(버프, 디버프)를 관리하는 클래스입니다.
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    private readonly Dictionary<GameObject, List<StatusEffect>> activeEffects = new Dictionary<GameObject, List<StatusEffect>>();
    private readonly List<StatusEffect> effectsToRemove = new List<StatusEffect>();
    private readonly List<GameObject> targetsToRemove = new List<GameObject>();

    void Awake()
    {
        ServiceLocator.Register<StatusEffectManager>(this);
    }

    void Update()
    {
        if (activeEffects.Count == 0) return;

        effectsToRemove.Clear();
        foreach (var entry in activeEffects.ToList()) // 순회 중 수정을 위해 ToList() 사용
        {
            GameObject target = entry.Key;
            if (target == null) // 대상이 파괴된 경우
            {
                targetsToRemove.Add(target);
                continue;
            }

            List<StatusEffect> effectsOnTarget = entry.Value;

            for (int i = effectsOnTarget.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = effectsOnTarget[i];

                if (effect.effectData.damageOverTime > 0)
                {
                    if (target.CompareTag(Tags.Monster))
                    {
                        var monster = target.GetComponentInChildren<MonsterController>();
                        if (monster != null)
                        {
                            float damageThisFrame = effect.effectData.damageOverTime * monster.maxHealth * Time.deltaTime;
                            monster.TakeDamage(damageThisFrame);
                        }
                    }
                }

                effect.duration -= Time.deltaTime;
                if (effect.duration <= 0)
                {
                    effectsToRemove.Add(effect);
                }
            }
        }

        if (effectsToRemove.Count > 0)
        {
            foreach (StatusEffect effect in effectsToRemove) { RemoveStatusEffect(effect); }
        }

        if (targetsToRemove.Count > 0)
        {
            foreach (GameObject t in targetsToRemove) { activeEffects.Remove(t); }
            targetsToRemove.Clear();
        }
    }

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
    }

    private void RemoveStatusEffect(StatusEffect effect)
    {
        if (effect == null || effect.target == null) return;

        if (activeEffects.TryGetValue(effect.target, out var effectList))
        {
            effect.RemoveEffect();
            effectList.Remove(effect);
        }
    }
}

/// <summary>
/// 활성화된 개별 상태 효과의 인스턴스 정보를 담는 클래스입니다.
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

    // 이 효과가 적용될 때 즉시 실행되는 로직 (주로 스탯 버프/디버프)
    public void ApplyEffect()
    {
        if (target.TryGetComponent<CharacterStats>(out var stats))
        {
            effectData.ApplyEffect(stats);
        }
    }

    // 이 효과가 제거될 때 실행되는 로직 (스탯 원상복구)
    public void RemoveEffect()
    {
        if (target.TryGetComponent<CharacterStats>(out var stats))
        {
            effectData.RemoveEffect(stats);
        }
    }
}