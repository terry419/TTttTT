// ���: ./TTttTT/Assets/1/Scripts/Data/CardEffects/ExplosiveZoneEffectSO.cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// [���� �и�] ���� ��߼� ���� ����(����)�� ����ϴ� ����Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_ExplosiveZone_", menuName = "GameData/v8.0/Modules/ExplosiveZoneEffect")]
public class ExplosiveZoneEffectSO : CardEffectSO
{
    [Header("���� ����")]
    [Tooltip("������ ���ظ� �ִ� ���� (������).")]
    public float explosionRadius = 5f;

    [Tooltip("���� �ð� ȿ��(VFX)�� ��巹���� �ּ�.")]
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