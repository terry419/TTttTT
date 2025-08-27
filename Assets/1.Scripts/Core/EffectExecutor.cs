// [리팩토링 완료] v8.0 실행 엔진 재구축
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectExecutor : MonoBehaviour
{
    private Dictionary<CardEffectType, ICardEffectHandler> effectHandlers;
    private EffectContextPool contextPool = new EffectContextPool();

    void Awake()
    {
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

    // --- 구버전 시스템을 위한 Execute (변경 없음) ---
    public void Execute(CardDataSO cardData, CharacterStats casterStats, Transform spawnPoint, float actualDamageDealt = 0f)
    {
        Debug.Log($"[Exec<Legacy>] 구버전 시스템 실행: {cardData.cardName}");
        if (cardData == null || casterStats == null || spawnPoint == null) return;
        if (effectHandlers.TryGetValue(cardData.effectType, out ICardEffectHandler handler))
        {
            handler.Execute(cardData, this, casterStats, spawnPoint);
        }
    }

    // ★★★ [핵심 수정] v8.0 신규 시스템을 위한 Execute 메소드 ★★★
    public void Execute(NewCardDataSO cardData, CharacterStats casterStats, Transform spawnPoint)
    {
        Debug.Log($"<color=cyan>[Exec<v8.0>]</color> 신규 시스템 실행: {cardData.basicInfo.cardName}");
        if (cardData == null || casterStats == null || spawnPoint == null) return;

        StartCoroutine(ExecuteV8_Coroutine(cardData, casterStats, spawnPoint));
    }

    private IEnumerator ExecuteV8_Coroutine(NewCardDataSO cardData, CharacterStats casterStats, Transform spawnPoint)
    {
        EffectContext context = contextPool.Get();
        context.Caster = casterStats;
        context.SpawnPoint = spawnPoint;

        foreach (var moduleEntry in cardData.modules)
        {
            if (!moduleEntry.moduleReference.RuntimeKeyIsValid()) continue;

            var handle = moduleEntry.moduleReference.LoadAssetAsync<CardEffectSO>();
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                CardEffectSO module = handle.Result;
                if (module != null)
                {
                    // 모듈 타입에 따라 실제 행동을 분기합니다.
                    switch (module)
                    {
                        case ProjectileEffectSO p:
                            HandleProjectileModule(p, cardData, context);
                            break;
                        case AreaEffectSO a:
                            HandleAreaModule(a, cardData, context);
                            break;
                        // TODO: 다른 모듈 타입에 대한 case 추가 필요
                        default:
                            module.Execute(context);
                            break;
                    }
                }
                // 사용한 에셋 핸들은 메모리 누수 방지를 위해 해제합니다.
                moduleEntry.moduleReference.ReleaseAsset();
            }
            else
            {
                Debug.LogError($"[EffectExecutor] 모듈 로딩 실패: {moduleEntry.moduleReference.AssetGUID}");
            }
        }

        contextPool.Return(context);
    }

    // --- v8.0 모듈 처리 헬퍼 함수들 ---

    private void HandleProjectileModule(ProjectileEffectSO pModule, NewCardDataSO platform, EffectContext context)
    {
        Debug.Log($" -> 투사체 모듈 처리: {pModule.name}");
        if (!pModule.bulletPrefabReference.RuntimeKeyIsValid()) return;

        // 최종 피해량 계산: 플랫폼의 기본 피해량 * 시전자 스탯 보너스
        float totalDamage = platform.baseDamage * (1 + context.Caster.FinalDamage / 100f);

        float baseAngle = GetTargetingAngle(pModule.targetingType, context.Caster.transform, context.SpawnPoint);
        float angleStep = (platform.projectileCount > 1 && platform.spreadAngle > 0) ? platform.spreadAngle / (platform.projectileCount - 1) : 0;
        float startAngle = baseAngle - (platform.spreadAngle / 2f);

        string shotID = Guid.NewGuid().ToString();

        for (int i = 0; i < platform.projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 direction = rotation * Vector2.right;

            // Addressable 프리팹 비동기 생성 요청
            pModule.bulletPrefabReference.InstantiateAsync(context.SpawnPoint.position, rotation).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result.TryGetComponent<BulletController>(out var bullet))
                {
                    // 새로 만든 v8.0용 Initialize 함수 호출
                    bullet.Initialize(direction, platform.baseSpeed * pModule.speed, totalDamage, shotID, pModule);
                }
                else
                {
                    // 생성 실패 시 핸들 해제
                    Addressables.Release(handle);
                }
            };
        }
    }

    private void HandleAreaModule(AreaEffectSO aModule, NewCardDataSO platform, EffectContext context)
    {
        Debug.Log($" -> 광역 효과 모듈 처리: {aModule.name}");
        if (!aModule.effectPrefabRef.RuntimeKeyIsValid()) return;

        aModule.effectPrefabRef.InstantiateAsync(context.SpawnPoint.position, Quaternion.identity).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result.TryGetComponent<DamagingZone>(out var zone))
            {
                // 파동 모드일 때는 플랫폼의 기본 피해량을, 장판 모드일 때는 모듈의 초기 충격 피해량을 사용
                float initialDamage = aModule.isSingleHitWaveMode ? platform.baseDamage * (1 + context.Caster.FinalDamage / 100f) : aModule.singleHitDamage;

                zone.Initialize(
                    singleHitDmg: initialDamage,
                    continuousDmgPerTick: aModule.damagePerTick,
                    tickInt: aModule.effectTickInterval,
                    totalDur: aModule.effectDuration,
                    expSpeed: aModule.effectExpansionSpeed,
                    expDur: aModule.effectExpansionDuration,
                    isWave: aModule.isSingleHitWaveMode,
                    shotID: Guid.NewGuid().ToString()
                );
            }
            else
            {
                Addressables.Release(handle);
            }
        };
    }

    // --- 기타 헬퍼 및 구버전 호환용 코드 (변경 없음) ---
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