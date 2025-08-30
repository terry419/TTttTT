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
    // activeEffects: 현재 어떤 게임오브젝트(Key)에 어떤 상태 효과들이(Value) 걸려있는지 저장하는 목록입니다.
    private readonly Dictionary<GameObject, List<StatusEffect>> activeEffects = new Dictionary<GameObject, List<StatusEffect>>();

    // effectsToRemove: Update 루프 안에서 duration이 다 된 효과들을 임시로 담아두는 리스트입니다.
    // (루프를 도는 중에 목록을 직접 수정하면 오류가 나기 때문에 임시 리스트를 사용합니다.)
    private readonly List<StatusEffect> effectsToRemove = new List<StatusEffect>();

    // targetsToRemove: 효과가 하나도 남지 않은 게임오브젝트를 임시로 담아두는 리스트입니다.
    private readonly List<GameObject> targetsToRemove = new List<GameObject>();

    /// <summary>
    /// 스크립트가 처음 깨어날 때 호출됩니다.
    /// 자기 자신을 서비스 로케이터(안내 데스크)에 등록하여 다른 스크립트들이 찾아올 수 있게 합니다.
    /// </summary>
    void Awake()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        // 자기 자신을 ServiceLocator에 'StatusEffectManager' 타입으로 등록합니다.
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
        foreach (var entry in activeEffects)
        {
            GameObject target = entry.Key;
            List<StatusEffect> effectsOnTarget = entry.Value;

            for (int i = effectsOnTarget.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = effectsOnTarget[i];

                // [디버그 3-2] 특정 대상의 특정 효과를 검사하고 있음을 기록합니다.
                // Debug.Log($"[디버그 3-2] 대상 '{target.name}'의 '{effect.effectData.name}' 효과 검사 중...");

                if (effect.effectData.damageOverTime > 0)
                {
                    // [디버그 3-3] 이 효과가 지속 데미지 타입임을 확인했습니다.
                    Debug.Log($"[디버그 3-3] '{effect.effectData.name}' 효과는 지속 데미지 타입임 (damageOverTime: {effect.effectData.damageOverTime})");

                    if (target.CompareTag("Monster"))
                    {
                        // [디버그 3-4] 대상이 'Monster' 태그를 가지고 있음을 확인했습니다.
                        Debug.Log("[디버그 3-4] 대상은 'Monster' 태그를 가지고 있음.");

                        var monster = target.GetComponentInChildren<MonsterController>();
                        if (monster != null)
                        {
                            // [디버그 3-5] 대상에게서 MonsterController를 성공적으로 찾았습니다.
                            Debug.Log("[디버그 3-5] 대상에게서 MonsterController를 성공적으로 찾음.");

                            float damageThisFrame = effect.effectData.damageOverTime * monster.maxHealth * Time.deltaTime;

                            // [디버그 3-6] 최종 데미지 계산 결과를 기록합니다.
                            Debug.Log($"[디버그 3-6] 지속 데미지 계산: {effect.effectData.damageOverTime}(비율) * {monster.maxHealth}(최대체력) * {Time.deltaTime}(deltaTime) = {damageThisFrame}");

                            // [디버그 3-7] TakeDamage를 호출하기 직전임을 알립니다.
                            Debug.Log("[디버그 3-7] TakeDamage 호출 직전.");
                            monster.TakeDamage(damageThisFrame);
                        }
                        else
                        {
                            // [디버그 3-5 실패] 몬스터 컨트롤러를 찾지 못했습니다.
                            Debug.LogError($"[디버그 3-5 실패] 대상 '{target.name}'은 'Monster' 태그가 있지만 MonsterController 컴포넌트가 없습니다!");
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

        // --- 이하 만료된 이펙트 제거 로직 (기존과 동일) ---
        if (effectsToRemove.Count > 0)
        {
            foreach (StatusEffect effect in effectsToRemove)
            {
                RemoveStatusEffect(effect);
            }
        }
        if (activeEffects.Count > 0)
        {
            targetsToRemove.Clear();
            foreach (var entry in activeEffects) { if (entry.Value.Count == 0) targetsToRemove.Add(entry.Key); }
            if (targetsToRemove.Count > 0) { foreach (GameObject t in targetsToRemove) activeEffects.Remove(t); }
        }
    }

    /// <summary>
    /// 특정 대상에게 상태 효과를 적용합니다.
    /// </summary>
    public void ApplyStatusEffect(GameObject target, StatusEffectDataSO effectData)
    {
        // [디버그 2-1] 효과 적용 요청을 받았음을 기록합니다.
        Debug.Log($"[디버그 2-1] ApplyStatusEffect 요청 받음. 대상: {target.name}, 효과: {effectData.name}");

        if (target == null || effectData == null) return;

        StatusEffect newEffect = new StatusEffect(target, effectData);

        if (!activeEffects.ContainsKey(target))
        {
            activeEffects[target] = new List<StatusEffect>();
        }

        activeEffects[target].Add(newEffect);
        newEffect.ApplyEffect();

        // [디버그 2-2] 상태 효과 목록에 정상적으로 추가되었음을 기록합니다.
        Debug.Log($"[디버그 2-2] 상태 효과 목록에 '{effectData.name}' 추가 완료. 현재 대상의 효과 수: {activeEffects[target].Count}개");
    }
    /// <summary>
    /// 특정 상태 효과 인스턴스를 제거합니다.
    /// </summary>
    private void RemoveStatusEffect(StatusEffect effect)
    {
        if (effect == null || effect.target == null) return;

        if (activeEffects.TryGetValue(effect.target, out var effectList))
        {
            effect.RemoveEffect(); // 스탯 즉시 변경 효과 원상복구
            effectList.Remove(effect);

            // [디버그] 어떤 효과가 종료되었는지 콘솔에 출력합니다.
            Debug.Log($"[상태 효과 종료] 대상: {effect.target.name}, 효과: {effect.effectData.name}");
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - OnDestroy() 시작. (프레임: {Time.frameCount})");
    }
}

/// <summary>
/// 활성화된 개별 상태 효과의 인스턴스 정보를 담는 클래스입니다.
/// </summary>
public class StatusEffect
{
    public GameObject target;           // 효과 대상
    public StatusEffectDataSO effectData; // 효과 원본 데이터
    public float duration;              // 남은 지속 시간

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