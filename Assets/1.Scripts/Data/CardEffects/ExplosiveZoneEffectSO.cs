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
    [Tooltip("폭발 시각 효과 프리팹")]
    public AssetReferenceGameObject explosionVfxRef;

    [Header("지속 피해 장판 효과")]
    [Tooltip("지속 피해 장판 프리팹 (DamageZoneController.cs 포함)")]
    public AssetReferenceGameObject dotZonePrefabRef;
    [Tooltip("장판의 반경")]
    public float zoneRadius = 3.5f;
    [Tooltip("장판 지속 시간 (초)")]
    public float zoneDuration = 5f;
    [Tooltip("장판이 1틱당 입히는 고정 피해량")]
    public float zoneDamagePerTick = 5f;
    [Tooltip("피해를 입히는 주기 (초). 0.5로 설정 시 0.5초마다 1틱")]
    public float zoneTickInterval = 0.5f;

    public ExplosiveZoneEffectSO()
    {
        trigger = EffectTrigger.OnHit;
    }

    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행.");
        HandleExplosion(context);
        HandleZoneCreation(context);
    }

    private void HandleExplosion(EffectContext context)
    {
        if (explosionRadius <= 0 || context.SourceCardInstance == null) return;

        // [수정] 폭발 피해량은 투사체를 발사한 카드의 최종 피해량 공식을 그대로 따릅니다.
        // 1. 기본 데미지 결정 (첫 타격이므로 override 무시)
        float baseDamageToUse = context.Platform.baseDamage;

        // 2. 카드 강화 레벨 적용
        int enhancementLevel = context.SourceCardInstance.EnhancementLevel;
        float enhancedBaseDamage = baseDamageToUse * (1f + enhancementLevel * 0.1f);

        // 3. 플레이어 종합 스탯("가중치") 적용
        float finalExplosionDamage = enhancedBaseDamage * (1 + context.Caster.FinalDamageBonus / 100f);

        Collider2D[] hitColliders = new Collider2D[30];
        int numColliders = Physics2D.OverlapCircleNonAlloc(context.HitPosition, explosionRadius, hitColliders);

        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].CompareTag(Tags.Monster) && hitColliders[i].TryGetComponent<MonsterController>(out var monster))
            {
                monster.TakeDamage(finalExplosionDamage);
            }
        }

        if (explosionVfxRef.RuntimeKeyIsValid())
        {
            ServiceLocator.Get<PoolManager>().GetAsync(explosionVfxRef.AssetGUID).ContinueWith(vfxGO => {
                if (vfxGO != null) vfxGO.transform.position = context.HitPosition;
            });
        }
    }

    private void HandleZoneCreation(EffectContext context)
    {
        if (!dotZonePrefabRef.RuntimeKeyIsValid()) return;

        ServiceLocator.Get<PoolManager>().GetAsync(dotZonePrefabRef.AssetGUID).ContinueWith(zoneGO => {
            if (zoneGO != null && zoneGO.TryGetComponent<DamageZoneController>(out var zoneController))
            {
                zoneController.transform.position = context.HitPosition;
                // [수정] 세분화된 설정값을 모두 전달합니다.
                zoneController.Initialize(zoneDuration, zoneRadius, zoneDamagePerTick, zoneTickInterval);
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