// 경로: Assets/1.Scripts/Data/CardEffects/ExplosiveZoneEffectSO.cs
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "Module_ExplosiveZone_", menuName = "GameData/v8.0/Modules/ExplosiveZoneEffect")]
public class ExplosiveZoneEffectSO : CardEffectSO, IPreloadable
{
    [Header("즉시 폭발 효과")]
    [Tooltip("폭발 반경")]
    public float explosionRadius = 3f;
    [Tooltip("폭발 피해량 (플랫폼 카드의 기본 피해량에 대한 배율)")]
    public float explosionDamageMultiplier = 1.5f;
    [Tooltip("폭발 시각 효과 프리팹")]
    public AssetReferenceGameObject explosionVfxRef;

    [Header("지속 피해 장판 효과")]
    [Tooltip("지속 피해 장판 프리팹 (DamageZoneController.cs 포함)")]
    public AssetReferenceGameObject dotZonePrefabRef;
    [Tooltip("장판 지속 시간")]
    public float zoneDuration = 5f;
    [Tooltip("초당 피해량 (플랫폼 카드의 기본 피해량에 대한 배율)")]
    public float damagePerSecondMultiplier = 0.5f;

    public ExplosiveZoneEffectSO()
    {
        // 이 모듈은 투사체가 명중했을 때 발동합니다.
        trigger = EffectTrigger.OnHit;
    }

    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행.");

        // 실행 순서 보장: 1. 폭발 -> 2. 장판 생성
        HandleExplosion(context);
        HandleZoneCreation(context);
    }

    private void HandleExplosion(EffectContext context)
    {
        if (explosionRadius <= 0) return;
        // OverlapCircleNonAlloc을 사용하여 GC 부담 감소
        Collider2D[] hitColliders = new Collider2D[20]; // 최대 20개까지만 감지
        int numColliders = Physics2D.OverlapCircleNonAlloc(context.HitPosition, explosionRadius, hitColliders);
        float explosionDamage = context.Platform.baseDamage * explosionDamageMultiplier;
        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].CompareTag(Tags.Monster))
            {
                if (hitColliders[i].TryGetComponent<MonsterController>(out var monster))
                {
                    monster.TakeDamage(explosionDamage);
                }
            }
        }
        // 폭발 VFX 재생 (비동기로 실행하여 다음 로직을 막지 않음)
        if (explosionVfxRef.RuntimeKeyIsValid())
        {
            ServiceLocator.Get<PoolManager>().GetAsync(explosionVfxRef.AssetGUID).ContinueWith(vfxGO => {
                if (vfxGO != null) vfxGO.transform.position = context.HitPosition;
                // 파티클 시스템의 경우 자동 비활성화 및 풀 반환 로직 필요
            });
        }
    }

    private void HandleZoneCreation(EffectContext context)
    {
        if (!dotZonePrefabRef.RuntimeKeyIsValid()) return;
        ServiceLocator.Get<PoolManager>().GetAsync(dotZonePrefabRef.AssetGUID).ContinueWith(zoneGO => {
            if (zoneGO != null && zoneGO.TryGetComponent<DamageZoneController>(out var zoneController))
            {
                float damagePerTick = context.Platform.baseDamage * damagePerSecondMultiplier;
                zoneController.transform.position = context.HitPosition;
                zoneController.Initialize(zoneDuration, damagePerTick);
            }
        });
    }

    public IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload()
    {
        if (explosionVfxRef != null && explosionVfxRef.RuntimeKeyIsValid())
            yield return explosionVfxRef;
        if (dotZonePrefabRef != null && dotZonePrefabRef.RuntimeKeyIsValid())
            yield return dotZonePrefabRef;
    }
}
