// 파일 경로: Assets/1/Scripts/Data/CardEffects/ExplosiveZoneEffectSO.cs
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 즉발 피해와 지속 피해 장판을 모두 생성하는 카드 효과(모듈)의 데이터 SO입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_ExplosiveZone_", menuName = "GameData/v8.0/Modules/ExplosiveZoneEffect")]
public class ExplosiveZoneEffectSO : CardEffectSO, IPreloadable
{
    // ========================================================================
    // ## 즉발 폭발 효과 섹션 ##
    // ========================================================================
    [Header("즉발 폭발 효과")]

    [Tooltip("최초 폭발의 물리적 대미지 반경입니다.")]
    public float explosionRadius = 5f;

    [Tooltip("최초 폭발 시각 효과 프리팹입니다. (예: VFX_Explosion_Converge)")]
    public AssetReferenceGameObject explosionVfxRef;

    // ========================================================================
    // ## 지속 피해 장판 효과 섹션 ##
    // ========================================================================
    [Header("지속 피해 장판 효과")]

    [Tooltip("지속 피해 장판의 물리적/시각적 반경입니다.")]
    public float zoneRadius = 8f;

    [Tooltip("지속 피해 장판 프리팹 (예: DamageZone_Cracks_Prefab)")]
    public AssetReferenceGameObject dotZonePrefabRef;

    [Tooltip("장판 지속 시간 (초)")]
    public float zoneDuration = 5f;

    [Tooltip("장판의 1틱당 대미지")]
    public float zoneDamagePerTick = 5f;

    [Tooltip("대미지 틱 간격 (초). 0.5는 0.5초마다 1틱")]
    public float zoneTickInterval = 0.5f;

    /// <summary>
    /// 생성자: 이 효과는 기본적으로 '피격 시(OnHit)' 발동하도록 설정합니다.
    /// </summary>
    public ExplosiveZoneEffectSO()
    {
        trigger = EffectTrigger.OnHit;
    }

    /// <summary>
    /// 이 카드 효과가 실제로 실행되는 진입점입니다.
    /// 즉발 폭발과 지속 장판 생성을 순차적으로 호출합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        HandleExplosion(context);   // 1. 즉발 폭발 실행
        HandleZoneCreation(context);  // 2. 지속 장판 생성 실행
    }

    /// <summary>
    /// 즉발 폭발의 물리 대미지와 시각 효과를 처리합니다.
    /// </summary>
    private void HandleExplosion(EffectContext context)
    {
        // 유효하지 않은 값이면 실행하지 않음
        if (explosionRadius <= 0) return;

        // 최종 대미지 계산 (카드 기본 피해량 * 강화 레벨 보너스 * 시전자 스탯 보너스)
        float finalExplosionDamage = 0;
        if (context.SourceCardInstance != null)
        {
            float baseDamageToUse = context.Platform.baseDamage;
            int enhancementLevel = context.SourceCardInstance.EnhancementLevel;
            float enhancedBaseDamage = baseDamageToUse * (1f + enhancementLevel * 0.1f);
            finalExplosionDamage = enhancedBaseDamage * (1 + context.Caster.FinalDamageBonus / 100f);
        }

        // 지정된 explosionRadius 크기의 원 안에 있는 모든 콜라이더를 감지
        Collider2D[] hitColliders = new Collider2D[30]; // 최대 30개까지만 감지
        int numColliders = Physics2D.OverlapCircleNonAlloc(context.HitPosition, explosionRadius, hitColliders);

        // 감지된 모든 콜라이더를 순회하며 몬스터에게 대미지를 입힘
        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].TryGetComponent<MonsterController>(out var monster))
            {
                monster.TakeDamage(finalExplosionDamage);
            }
        }

        // Addressable로 연결된 시각 효과(VFX) 프리팹이 유효한지 확인
        if (explosionVfxRef.RuntimeKeyIsValid())
        {
            // PoolManager를 통해 비동기적으로 VFX 오브젝트를 가져옴
            ServiceLocator.Get<PoolManager>().GetAsync(explosionVfxRef.AssetGUID).ContinueWith(vfxGO => {
                if (vfxGO != null)
                {
                    vfxGO.transform.position = context.HitPosition; // 폭발 위치로 이동
                    float scale = explosionRadius * 2f; // 물리 반경에 맞춰 시각 효과 크기 조절 (반경*2=지름)
                    vfxGO.transform.localScale = new Vector3(scale, scale, 1f);
                }
            });
        }
    }

    /// <summary>
    /// 지속 피해 장판 오브젝트를 생성하고 초기화합니다.
    /// </summary>
    private void HandleZoneCreation(EffectContext context)
    {
        if (zoneRadius <= 0 || !dotZonePrefabRef.RuntimeKeyIsValid()) return;

        // PoolManager를 통해 장판 프리팹을 가져옴
        ServiceLocator.Get<PoolManager>().GetAsync(dotZonePrefabRef.AssetGUID).ContinueWith(zoneGO => {
            // 가져온 오브젝트에 DamageZoneController 스크립트가 있는지 확인
            if (zoneGO != null && zoneGO.TryGetComponent<DamageZoneController>(out var zoneController))
            {
                zoneController.transform.position = context.HitPosition; // 장판 위치 설정
                // DamageZoneController를 초기화하면서 지속시간, 반경, 대미지 등 이 스크립트의 데이터를 전달
                zoneController.Initialize(zoneDuration, zoneRadius, zoneDamagePerTick, zoneTickInterval);
            }
        });
    }

    /// <summary>
    /// 게임 시작 전 로딩(Preloading) 시 미리 생성해 둘 프리팹 목록을 반환합니다.
    /// </summary>
    public IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload()
    {
        if (explosionVfxRef != null && explosionVfxRef.RuntimeKeyIsValid())
            yield return explosionVfxRef;
        if (dotZonePrefabRef != null && dotZonePrefabRef.RuntimeKeyIsValid())
            yield return dotZonePrefabRef;
    }
}