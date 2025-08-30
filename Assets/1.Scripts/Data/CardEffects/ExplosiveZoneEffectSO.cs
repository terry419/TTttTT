// ���� ���: Assets/1/Scripts/Data/CardEffects/ExplosiveZoneEffectSO.cs
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// ��� ���ؿ� ���� ���� ������ ��� �����ϴ� ī�� ȿ��(���)�� ������ SO�Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_ExplosiveZone_", menuName = "GameData/v8.0/Modules/ExplosiveZoneEffect")]
public class ExplosiveZoneEffectSO : CardEffectSO, IPreloadable
{
    // ========================================================================
    // ## ��� ���� ȿ�� ���� ##
    // ========================================================================
    [Header("��� ���� ȿ��")]

    [Tooltip("���� ������ ������ ����� �ݰ��Դϴ�.")]
    public float explosionRadius = 5f;

    [Tooltip("���� ���� �ð� ȿ�� �������Դϴ�. (��: VFX_Explosion_Converge)")]
    public AssetReferenceGameObject explosionVfxRef;

    // ========================================================================
    // ## ���� ���� ���� ȿ�� ���� ##
    // ========================================================================
    [Header("���� ���� ���� ȿ��")]

    [Tooltip("���� ���� ������ ������/�ð��� �ݰ��Դϴ�.")]
    public float zoneRadius = 8f;

    [Tooltip("���� ���� ���� ������ (��: DamageZone_Cracks_Prefab)")]
    public AssetReferenceGameObject dotZonePrefabRef;

    [Tooltip("���� ���� �ð� (��)")]
    public float zoneDuration = 5f;

    [Tooltip("������ 1ƽ�� �����")]
    public float zoneDamagePerTick = 5f;

    [Tooltip("����� ƽ ���� (��). 0.5�� 0.5�ʸ��� 1ƽ")]
    public float zoneTickInterval = 0.5f;

    /// <summary>
    /// ������: �� ȿ���� �⺻������ '�ǰ� ��(OnHit)' �ߵ��ϵ��� �����մϴ�.
    /// </summary>
    public ExplosiveZoneEffectSO()
    {
        trigger = EffectTrigger.OnHit;
    }

    /// <summary>
    /// �� ī�� ȿ���� ������ ����Ǵ� �������Դϴ�.
    /// ��� ���߰� ���� ���� ������ ���������� ȣ���մϴ�.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        HandleExplosion(context);   // 1. ��� ���� ����
        HandleZoneCreation(context);  // 2. ���� ���� ���� ����
    }

    /// <summary>
    /// ��� ������ ���� ������� �ð� ȿ���� ó���մϴ�.
    /// </summary>
    private void HandleExplosion(EffectContext context)
    {
        // ��ȿ���� ���� ���̸� �������� ����
        if (explosionRadius <= 0) return;

        // ���� ����� ��� (ī�� �⺻ ���ط� * ��ȭ ���� ���ʽ� * ������ ���� ���ʽ�)
        float finalExplosionDamage = 0;
        if (context.SourceCardInstance != null)
        {
            float baseDamageToUse = context.Platform.baseDamage;
            int enhancementLevel = context.SourceCardInstance.EnhancementLevel;
            float enhancedBaseDamage = baseDamageToUse * (1f + enhancementLevel * 0.1f);
            finalExplosionDamage = enhancedBaseDamage * (1 + context.Caster.FinalDamageBonus / 100f);
        }

        // ������ explosionRadius ũ���� �� �ȿ� �ִ� ��� �ݶ��̴��� ����
        Collider2D[] hitColliders = new Collider2D[30]; // �ִ� 30�������� ����
        int numColliders = Physics2D.OverlapCircleNonAlloc(context.HitPosition, explosionRadius, hitColliders);

        // ������ ��� �ݶ��̴��� ��ȸ�ϸ� ���Ϳ��� ������� ����
        for (int i = 0; i < numColliders; i++)
        {
            if (hitColliders[i].TryGetComponent<MonsterController>(out var monster))
            {
                monster.TakeDamage(finalExplosionDamage);
            }
        }

        // Addressable�� ����� �ð� ȿ��(VFX) �������� ��ȿ���� Ȯ��
        if (explosionVfxRef.RuntimeKeyIsValid())
        {
            // PoolManager�� ���� �񵿱������� VFX ������Ʈ�� ������
            ServiceLocator.Get<PoolManager>().GetAsync(explosionVfxRef.AssetGUID).ContinueWith(vfxGO => {
                if (vfxGO != null)
                {
                    vfxGO.transform.position = context.HitPosition; // ���� ��ġ�� �̵�
                    float scale = explosionRadius * 2f; // ���� �ݰ濡 ���� �ð� ȿ�� ũ�� ���� (�ݰ�*2=����)
                    vfxGO.transform.localScale = new Vector3(scale, scale, 1f);
                }
            });
        }
    }

    /// <summary>
    /// ���� ���� ���� ������Ʈ�� �����ϰ� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void HandleZoneCreation(EffectContext context)
    {
        if (zoneRadius <= 0 || !dotZonePrefabRef.RuntimeKeyIsValid()) return;

        // PoolManager�� ���� ���� �������� ������
        ServiceLocator.Get<PoolManager>().GetAsync(dotZonePrefabRef.AssetGUID).ContinueWith(zoneGO => {
            // ������ ������Ʈ�� DamageZoneController ��ũ��Ʈ�� �ִ��� Ȯ��
            if (zoneGO != null && zoneGO.TryGetComponent<DamageZoneController>(out var zoneController))
            {
                zoneController.transform.position = context.HitPosition; // ���� ��ġ ����
                // DamageZoneController�� �ʱ�ȭ�ϸ鼭 ���ӽð�, �ݰ�, ����� �� �� ��ũ��Ʈ�� �����͸� ����
                zoneController.Initialize(zoneDuration, zoneRadius, zoneDamagePerTick, zoneTickInterval);
            }
        });
    }

    /// <summary>
    /// ���� ���� �� �ε�(Preloading) �� �̸� ������ �� ������ ����� ��ȯ�մϴ�.
    /// </summary>
    public IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload()
    {
        if (explosionVfxRef != null && explosionVfxRef.RuntimeKeyIsValid())
            yield return explosionVfxRef;
        if (dotZonePrefabRef != null && dotZonePrefabRef.RuntimeKeyIsValid())
            yield return dotZonePrefabRef;
    }
}