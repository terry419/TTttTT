// ���: Assets/1.Scripts/Data/CardEffects/ExplosiveZoneEffectSO.cs
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "Module_ExplosiveZone_", menuName = "GameData/v8.0/Modules/ExplosiveZoneEffect")]
public class ExplosiveZoneEffectSO : CardEffectSO, IPreloadable
{
    [Header("��� ���� ȿ��")]
    [Tooltip("���� �ݰ�")]
    public float explosionRadius = 3f;
    [Tooltip("���� �ð� ȿ�� ������")]
    public AssetReferenceGameObject explosionVfxRef;

    [Header("���� ���� ���� ȿ��")]
    [Tooltip("���� ���� ���� ������ (DamageZoneController.cs ����)")]
    public AssetReferenceGameObject dotZonePrefabRef;
    [Tooltip("������ �ݰ�")]
    public float zoneRadius = 3.5f;
    [Tooltip("���� ���� �ð� (��)")]
    public float zoneDuration = 5f;
    [Tooltip("������ 1ƽ�� ������ ���� ���ط�")]
    public float zoneDamagePerTick = 5f;
    [Tooltip("���ظ� ������ �ֱ� (��). 0.5�� ���� �� 0.5�ʸ��� 1ƽ")]
    public float zoneTickInterval = 0.5f;

    public ExplosiveZoneEffectSO()
    {
        trigger = EffectTrigger.OnHit;
    }

    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ����.");
        HandleExplosion(context);
        HandleZoneCreation(context);
    }

    private void HandleExplosion(EffectContext context)
    {
        if (explosionRadius <= 0 || context.SourceCardInstance == null) return;

        // [����] ���� ���ط��� ����ü�� �߻��� ī���� ���� ���ط� ������ �״�� �����ϴ�.
        // 1. �⺻ ������ ���� (ù Ÿ���̹Ƿ� override ����)
        float baseDamageToUse = context.Platform.baseDamage;

        // 2. ī�� ��ȭ ���� ����
        int enhancementLevel = context.SourceCardInstance.EnhancementLevel;
        float enhancedBaseDamage = baseDamageToUse * (1f + enhancementLevel * 0.1f);

        // 3. �÷��̾� ���� ����("����ġ") ����
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
                // [����] ����ȭ�� �������� ��� �����մϴ�.
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