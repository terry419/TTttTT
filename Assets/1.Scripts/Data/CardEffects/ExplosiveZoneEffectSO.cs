// 경로: Assets/1.Scripts/Data/CardEffects/ExplosiveZoneEffectSO.cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// [카드 효과] 총알 명중 시, 폭발 효과(데미지)를 생성하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_ExplosiveZone_", menuName = "GameData/CardData/Modules/ExplosiveZoneEffect")]
public class ExplosiveZoneEffectSO : CardEffectSO
{
    [Header("폭발 설정")]
    [Tooltip("폭발이 영향을 미치는 반경 (미터).")]
    public float explosionRadius = 5f;

    [Tooltip("폭발 시각 효과(VFX)로 사용될 프리팹 주소.")]
    public AssetReferenceGameObject explosionVfxRef;

    public override void Execute(EffectContext context)
    {
        HandleExplosion(context);
    }

    private void HandleExplosion(EffectContext context)
    {
        if (explosionRadius <= 0) return;

        float finalExplosionDamage = 0;
        if (context.SourceCardInstance != null)
        {
            float baseDamageToUse = context.Platform.baseDamage;
            int enhancementLevel = context.SourceCardInstance.EnhancementLevel;
            float enhancedBaseDamage = baseDamageToUse * (1f + enhancementLevel * 0.1f);
            finalExplosionDamage = enhancedBaseDamage * (1 + context.Caster.FinalDamageBonus / 100f);
        }

        Collider2D[] hitColliders = new Collider2D[30];
        int numColliders = Physics2D.OverlapCircleNonAlloc(context.HitPosition, explosionRadius, hitColliders);

        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].TryGetComponent<MonsterController>(out var monster))
            {
                // [수정] 총알에 직접 맞은 대상은 폭발 피해에서 제외합니다.
                if (monster == context.HitTarget)
                {
                    continue;
                }
                monster.TakeDamage(finalExplosionDamage);
            }
        }

        if (explosionVfxRef.RuntimeKeyIsValid())
        {
            ServiceLocator.Get<PoolManager>().GetAsync(explosionVfxRef.AssetGUID).ContinueWith(vfxGO => {
                if (vfxGO != null)
                {
                    vfxGO.transform.position = context.HitPosition;
                    float scale = explosionRadius * 2f;
                    vfxGO.transform.localScale = new Vector3(scale, scale, 1f);
                }
            });
        }
    }
}