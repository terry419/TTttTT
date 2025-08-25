// [리팩토링 완료]
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EffectExecutor : MonoBehaviour
{
    private Dictionary<CardEffectType, ICardEffectHandler> effectHandlers;

    void Awake()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        if (!ServiceLocator.IsRegistered<EffectExecutor>())
        {
            ServiceLocator.Register<EffectExecutor>(this);
            DontDestroyOnLoad(gameObject);
            InitializeHandlers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeHandlers()
    {
        effectHandlers = new Dictionary<CardEffectType, ICardEffectHandler>
        {
            { CardEffectType.SplitShot, new SplitShotHandler() },
            { CardEffectType.Wave, new WaveHandler() },
            { CardEffectType.Lightning, new LightningHandler() }
        };
    }

    // [리팩토링] 시전자(Caster)의 정보를 직접 매개변수로 받습니다.
    public void Execute(CardDataSO cardData, CharacterStats casterStats, Transform spawnPoint, float actualDamageDealt = 0f)
    {
        if (cardData == null || casterStats == null || spawnPoint == null)
        {
            Debug.LogError("[EffectExecutor] 필수 인자(CardData, CasterStats, SpawnPoint) 중 하나가 null입니다!");
            return;
        }

        

        // 카드 효과 타입에 따른 핸들러 실행
        if (effectHandlers.TryGetValue(cardData.effectType, out ICardEffectHandler handler))
        { 
            // 핸들러에게도 시전자 정보를 넘겨주어야 할 수 있습니다. (지금은 EffectExecutor만 넘김)
            handler.Execute(cardData, this, casterStats, spawnPoint);
        }
        else
        {
            Debug.LogError($"[EffectExecutor] '{cardData.effectType}' 타입에 대한 핸들러가 등록되어 있지 않습니다!");
        }
    }

    // [리팩토링] 데미지 계산 시에도 시전자의 스탯을 직접 받습니다.
    public float CalculateTotalDamage(CardDataSO cardData, CharacterStats casterStats)
    {
        if (casterStats == null || cardData == null)
        {
            return cardData != null ? cardData.baseDamage : 0f;
        }

        if (cardData.baseDamage <= 0)
        {
            return 0f;
        }

        // CharacterStats의 FinalDamage가 이제 '총 공격력 보너스'를 의미합니다.
        float totalAttackBonus = casterStats.FinalDamage;

        // 최종 대미지 = 카드 기본 대미지 * (1 + 총 공격력 보너스 / 100)
        float finalDamage = cardData.baseDamage * (1 + totalAttackBonus / 100f);

        return finalDamage;
    }

    // [리팩토링] 타겟팅 각도 계산 시에도 시전자의 위치 정보가 필요합니다.
    public float GetTargetingAngle(TargetingType targetingType, Transform casterTransform, Transform spawnPoint)
    {
        if (casterTransform == null || spawnPoint == null)
        {
             Debug.LogError("[EffectExecutor] GetTargetingAngle: casterTransform 또는 spawnPoint가 null입니다!");
            return 0f;
        }

        Transform target = TargetingSystem.FindTarget(targetingType, casterTransform);

        if (target != null)
        {
            Vector2 directionToTarget = (target.position - spawnPoint.position).normalized;
            return Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        }
        else
        {
            return spawnPoint.eulerAngles.z;
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - OnDestroy() 시작. (프레임: {Time.frameCount})");
    }
}