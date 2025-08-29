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
    [Tooltip("���� ���ط� (�÷��� ī���� �⺻ ���ط��� ���� ����)")]
    public float explosionDamageMultiplier = 1.5f;
    [Tooltip("���� �ð� ȿ�� ������")]
    public AssetReferenceGameObject explosionVfxRef;

    [Header("���� ���� ���� ȿ��")]
    [Tooltip("���� ���� ���� ������ (DamageZoneController.cs ����)")]
    public AssetReferenceGameObject dotZonePrefabRef;
    [Tooltip("���� ���� �ð�")]
    public float zoneDuration = 5f;
    [Tooltip("�ʴ� ���ط� (�÷��� ī���� �⺻ ���ط��� ���� ����)")]
    public float damagePerSecondMultiplier = 0.5f;

    public ExplosiveZoneEffectSO()
    {
        // �� ����� ����ü�� �������� �� �ߵ��մϴ�.
        trigger = EffectTrigger.OnHit;
    }

    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ����.");

        // ���� ���� ����: 1. ���� -> 2. ���� ����
        HandleExplosion(context);
        HandleZoneCreation(context);
    }

    private void HandleExplosion(EffectContext context)
    {
        if (explosionRadius <= 0) return;
        // OverlapCircleNonAlloc�� ����Ͽ� GC �δ� ����
        Collider2D[] hitColliders = new Collider2D[20]; // �ִ� 20�������� ����
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
        // ���� VFX ��� (�񵿱�� �����Ͽ� ���� ������ ���� ����)
        if (explosionVfxRef.RuntimeKeyIsValid())
        {
            ServiceLocator.Get<PoolManager>().GetAsync(explosionVfxRef.AssetGUID).ContinueWith(vfxGO => {
                if (vfxGO != null) vfxGO.transform.position = context.HitPosition;
                // ��ƼŬ �ý����� ��� �ڵ� ��Ȱ��ȭ �� Ǯ ��ȯ ���� �ʿ�
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
