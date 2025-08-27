// [리팩토링 완료] v8.0 공존을 위한 분기 로직 추가
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EffectExecutor : MonoBehaviour
{
    // --- 구버전 시스템을 위한 핸들러 ---
    private Dictionary<CardEffectType, ICardEffectHandler> effectHandlers;

    // --- v8.0 시스템을 위한 풀 ---
    private EffectContextPool contextPool = new EffectContextPool();

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<EffectExecutor>())
        {
            ServiceLocator.Register<EffectExecutor>(this);
            DontDestroyOnLoad(gameObject);
            InitializeHandlers(); // 구버전 핸들러 초기화
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 구버전 시스템을 위한 Execute 메소드 (기존 코드 유지) ---
    public void Execute(CardDataSO cardData, CharacterStats casterStats, Transform spawnPoint, float actualDamageDealt = 0f)
    {
        Debug.Log($"[Exec<Legacy>] 구버전 시스템 실행: {cardData.cardName}");
        if (cardData == null || casterStats == null || spawnPoint == null) return;

        if (effectHandlers.TryGetValue(cardData.effectType, out ICardEffectHandler handler))
        {
            handler.Execute(cardData, this, casterStats, spawnPoint);
        }
    }

    // ★★★ [핵심 추가] v8.0 신규 시스템을 위한 Execute 메소드 ★★★
    public void Execute(NewCardDataSO cardData, CharacterStats casterStats, Transform spawnPoint)
    {
        Debug.Log($"<color=cyan>[Exec<v8.0>]</color> 신규 시스템 실행: {cardData.basicInfo.cardName}");
        if (cardData == null || casterStats == null || spawnPoint == null) return;

        // 1. 컨텍스트 풀에서 EffectContext 가져오기
        EffectContext context = contextPool.Get();
        context.Caster = casterStats;
        context.SpawnPoint = spawnPoint;

        // 2. 카드에 연결된 모든 모듈을 순차적으로 실행
        foreach (var moduleEntry in cardData.modules)
        {
            // Addressable을 통해 실제 모듈 에셋을 로드합니다. (실제 로딩은 비동기 처리 필요)
            CardEffectSO module = moduleEntry.moduleReference.Asset as CardEffectSO;
            if (module != null)
            {
                // 각 모듈의 Execute 실행
                module.Execute(context);
            }
        }

        // 3. 사용이 끝난 컨텍스트는 풀에 반환
        contextPool.Return(context);
    }


    // --- 구버전 시스템을 위한 초기화 및 헬퍼 메소드 (기존 코드 유지) ---
    private void InitializeHandlers()
    {
        effectHandlers = new Dictionary<CardEffectType, ICardEffectHandler>
        {
            { CardEffectType.SplitShot, new SplitShotHandler() },
            { CardEffectType.Wave, new WaveHandler() },
            { CardEffectType.Lightning, new LightningHandler() }
        };
    }

    public float CalculateTotalDamage(CardDataSO cardData, CharacterStats casterStats)
    {
        if (casterStats == null || cardData == null || cardData.baseDamage <= 0) return 0f;
        float totalAttackBonus = casterStats.FinalDamage;
        return cardData.baseDamage * (1 + totalAttackBonus / 100f);
    }

    public float GetTargetingAngle(TargetingType targetingType, Transform casterTransform, Transform spawnPoint)
    {
        if (casterTransform == null || spawnPoint == null) return 0f;
        Transform target = TargetingSystem.FindTarget(targetingType, casterTransform);
        if (target != null)
        {
            Vector2 directionToTarget = (target.position - spawnPoint.position).normalized;
            return Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        }
        return spawnPoint.eulerAngles.z;
    }
}