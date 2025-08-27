// 경로: ./TTttTT/Assets/1.Scripts/Core/EffectExecutor.cs

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
        context.Platform = cardData; // [추가] 컨텍스트에 원본 카드(플랫폼) 정보 저장

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

    // [수정] 연쇄 효과 실행 시 원본 카드(platform) 정보를 함께 전달받도록 변경
    public void ExecuteChainedEffect(AssetReferenceT<CardEffectSO> moduleRef, CharacterStats caster, Transform spawnPoint, NewCardDataSO platform)
    {
        if (caster == null || spawnPoint == null || !moduleRef.RuntimeKeyIsValid()) return;
        Debug.Log($"<color=magenta>[Chained Effect]</color> 연쇄 효과 실행 요청: {moduleRef.AssetGUID}");

        // 새로운 컨텍스트를 생성하여 연쇄 효과를 실행하는 코루틴을 시작합니다.
        StartCoroutine(ExecuteSingleModuleCoroutine(moduleRef, caster, spawnPoint, platform));
    }

    private IEnumerator ExecuteSingleModuleCoroutine(AssetReferenceT<CardEffectSO> moduleRef, CharacterStats caster, Transform spawnPoint, NewCardDataSO platform)
    {
        EffectContext context = contextPool.Get();
        context.Caster = caster;
        context.SpawnPoint = spawnPoint;
        context.Platform = platform; // [추가] 전달받은 원본 카드 정보를 컨텍스트에 저장
        context.HitTarget = spawnPoint.GetComponent<MonsterController>(); // 연쇄 효과의 대상은 피격된 몬스터
        context.HitPosition = spawnPoint.position;

        var handle = moduleRef.LoadAssetAsync<CardEffectSO>();
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            CardEffectSO module = handle.Result;
            if (module != null)
            {
                // 모듈 타입에 따라 분기하여 실제 로직 실행
                switch (module)
                {
                    case ProjectileEffectSO p:
                        // 연쇄 투사체는 platform 정보가 없으므로 null을 전달. 단, context에는 정보가 남아있음.
                        HandleProjectileModule(p, null, context);
                        break;
                    case AreaEffectSO a:
                        // 연쇄 광역 효과도 platform 정보가 없으므로 null을 전달.
                        HandleAreaModule(a, null, context);
                        break;
                    default:
                        module.Execute(context);
                        break;
                }
            }
            moduleRef.ReleaseAsset();
        }
        else
        {
            Debug.LogError($"[EffectExecutor] 연쇄 효과 모듈 로딩 실패: {moduleRef.AssetGUID}");
        }

        contextPool.Return(context);
    }

    // --- v8.0 모듈 처리 헬퍼 함수들 ---

    private void HandleProjectileModule(ProjectileEffectSO pModule, NewCardDataSO platform, EffectContext context)
    {
        Debug.Log($" -> 투사체 모듈 처리: {pModule.name}");
        if (!pModule.bulletPrefabReference.RuntimeKeyIsValid()) return;

        // [수정] platform이 null일 경우 context의 Platform 정보를 사용 (연쇄 효과 대응)
        var activePlatform = platform ?? context.Platform;
        if (activePlatform == null)
        {
            Debug.LogError($"[EffectExecutor] 투사체 모듈({pModule.name})을 실행할 Platform 정보를 찾을 수 없습니다!");
            return;
        }

        // [수정] FinalDamageBonus 사용
        float totalDamage = activePlatform.baseDamage * (1 + context.Caster.FinalDamageBonus / 100f);
        float baseAngle = GetTargetingAngle(pModule.targetingType, context.Caster.transform, context.SpawnPoint);
        float angleStep = (activePlatform.projectileCount > 1 && activePlatform.spreadAngle > 0) ?
            activePlatform.spreadAngle / (activePlatform.projectileCount - 1) : 0;
        float startAngle = baseAngle - (activePlatform.spreadAngle / 2f);

        string shotID = Guid.NewGuid().ToString();
        for (int i = 0; i < activePlatform.projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 direction = rotation * Vector2.right;

            pModule.bulletPrefabReference.InstantiateAsync(context.SpawnPoint.position, rotation).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result.TryGetComponent<BulletController>(out var bullet))
                {
                    // [수정] Initialize 호출 시 activePlatform 전달
                    bullet.Initialize(direction, activePlatform.baseSpeed * pModule.speed, totalDamage, shotID, activePlatform, pModule, context.Caster);
                }
                else
                {
                    Addressables.Release(handle);
                }
            };
        }
    }

    private void HandleAreaModule(AreaEffectSO aModule, NewCardDataSO platform, EffectContext context)
    {
        // [디버그 추가 1] HandleAreaModule 진입 로그
        Debug.Log($"[EffectExecutor-DEBUG 1] HandleAreaModule 진입. 모듈: '{aModule.name}'");

        if (aModule.effectPrefabRef == null || !aModule.effectPrefabRef.RuntimeKeyIsValid())
        {
            // [디버그 추가 1-1] 프리팹 참조가 유효하지 않을 경우 로그
            Debug.LogError($"[EffectExecutor-DEBUG 1-1] CRITICAL: '{aModule.name}' 모듈의 effectPrefabRef가 할당되지 않았거나 유효하지 않습니다!");
            return;
        }

        // [디버그 추가 2] 프리팹 비동기 생성 시작 로그
        Debug.Log($"[EffectExecutor-DEBUG 2] Addressables를 통해 '{aModule.effectPrefabRef.AssetGUID}' 프리팹 생성을 시작합니다.");

        aModule.effectPrefabRef.InstantiateAsync(context.SpawnPoint.position, Quaternion.identity).Completed += (handle) =>
        {
            // [디버그 추가 3] 비동기 생성 완료 콜백 진입 로그
            Debug.Log($"[EffectExecutor-DEBUG 3] 프리팹 생성 콜백 진입. Handle Status: {handle.Status}");

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // [디버그 추가 4] 프리팹 생성 성공 로그
                Debug.Log($"[EffectExecutor-DEBUG 4] 프리팹 생성 성공! 생성된 오브젝트: '{handle.Result?.name ?? "NULL"}'");

                if (handle.Result.TryGetComponent<DamagingZone>(out var zone))
                {
                    // [디버그 추가 5] DamagingZone 컴포넌트 찾기 성공 로그
                    Debug.Log("[EffectExecutor-DEBUG 5] DamagingZone 컴포넌트를 성공적으로 찾았습니다.");

                    // [디버그 추가 6] NullReferenceException 발생 전, context.Caster 객체 상태 확인
                    if (context.Caster == null)
                    {
                        Debug.LogError("[EffectExecutor-DEBUG 6] CRITICAL: zone.Initialize() 호출 직전, 'context.Caster'가 null입니다! 여기서 예외가 발생할 가능성이 높습니다.");
                    }
                    else
                    {
                        Debug.Log($"[EffectExecutor-DEBUG 6] 'context.Caster'가 유효합니다. Caster 이름: {context.Caster.name}");
                    }

                    float finalSingleHitDamage = aModule.singleHitDamage * (1 + context.Caster.FinalDamageBonus / 100f);
                    float finalDamagePerTick = aModule.damagePerTick * (1 + context.Caster.FinalDamageBonus / 100f);

                    // [디버그 추가 7] zone.Initialize() 호출 직전 로그
                    Debug.Log("[EffectExecutor-DEBUG 7] zone.Initialize()를 호출합니다.");
                    zone.Initialize(
                        singleHitDmg: finalSingleHitDamage,
                        continuousDmgPerTick: finalDamagePerTick,
                        tickInt: aModule.effectTickInterval,
                        totalDur: aModule.effectDuration,
                        maxRadius: aModule.maxExpansionRadius,
                        expDur: aModule.effectExpansionDuration,
                        isWave: aModule.isSingleHitWaveMode,
                        shotID: Guid.NewGuid().ToString()
                    );
                }
                else
                {
                    // [디버그 추가 5-1] DamagingZone 컴포넌트 찾기 실패 로그
                    Debug.LogError($"[EffectExecutor-DEBUG 5-1] CRITICAL: 생성된 프리팹 '{handle.Result.name}'에서 DamagingZone 컴포넌트를 찾지 못했습니다!");
                    Addressables.Release(handle); // 컴포넌트 없으면 릴리즈 처리
                }
            }
            else
            {
                // [디버그 추가 4-1] 프리팹 생성 실패 로그
                Debug.LogError($"[EffectExecutor-DEBUG 4-1] CRITICAL: 프리팹 생성에 실패했습니다! Status: {handle.Status}");
            }
        };
    }

    // --- 기타 헬퍼 및 구버전 호환용 코드 ---
    private void InitializeHandlers()
    {
        effectHandlers = new Dictionary<CardEffectType, ICardEffectHandler>
        {
            { CardEffectType.SplitShot, new SplitShotHandler() },
            { CardEffectType.Wave, new WaveHandler() },
            { CardEffectType.Lightning, new LightningHandler() }
        };
    }

    // [수정] FinalDamageBonus 사용
    public float CalculateTotalDamage(CardDataSO cardData, CharacterStats casterStats)
    {
        if (casterStats == null || cardData == null || cardData.baseDamage <= 0) return 0f;
        float totalAttackBonus = casterStats.FinalDamageBonus;
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