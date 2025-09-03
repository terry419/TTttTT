// ���� ���: Assets/1.Scripts/Data/CardEffects/CreateGravityPulseZoneEffectSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "Module_CreateGravityPulseZone_", menuName = "GameData/v8.0/Modules/CreateGravityPulseZoneEffect")]
public class CreateGravityPulseZoneEffectSO : CardEffectSO
{
    [Header("--- ���� ���� ���� ---")]
    [Tooltip("�ݵ�� GravityPulseZoneController.cs ������Ʈ�� ���� '����' �������̾�� �մϴ�.")]
    public AssetReferenceGameObject ZonePrefabRef;

    [Tooltip("���� ��ü�� �����Ǵ� �ð� (��).")]
    public float ZoneDuration = 8f;

    [Header("--- �߷� ȿ�� ---")]
    [Tooltip("���͸� ������� ȿ���� �ִ� �ݰ��Դϴ�.")]
    public float PullRadius = 8f;

    [Tooltip("���͸� �߽����� ������� ���� ũ���Դϴ�.")]
    public float PullForce = 150f;

    [Header("--- �Ƶ� ���� ȿ�� ---")]
    [Tooltip("Ŀ���� �۾����� �ӵ��Դϴ�. �������� �����ϴ�.")]
    public float PulseSpeed = 5f;

    [Tooltip("�ּ� ũ�� �����Դϴ�. (��: 0.2�� �ִ� �ݰ��� 20%���� �۾���)")]
    [Range(0f, 1f)]
    public float MinPulseScaleRatio = 0.2f;

    [Tooltip("���ظ� �ִ� �ֱ�(��)�Դϴ�. (��: 0.5�� 1�ʿ� 2�� ����)")]
    public float DamageTickInterval = 0.5f;


    public override void Execute(EffectContext context)
    {
        if (ZonePrefabRef == null || !ZonePrefabRef.RuntimeKeyIsValid())
        {
            Debug.LogError($"[{name}] ���� �������� ��ȿ�ϰ� �������� �ʾҽ��ϴ�!");
            return;
        }
        CreateZoneAsync(context).Forget();
    }

    private async UniTaskVoid CreateZoneAsync(EffectContext context)
    {
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        GameObject zoneGO = await poolManager.GetAsync(ZonePrefabRef.AssetGUID);
        if (zoneGO != null && zoneGO.TryGetComponent<GravityPulseZoneController>(out var zoneController))
        {
            // ��ź ��ġ�� ����
            Vector3 spawnPosition = context.HitPosition;
            zoneGO.transform.position = spawnPosition;

            // ���� ���ط� ��� (ī�� ��ȭ ����, �÷��̾� ���� ���ʽ� ����)
            float baseDamage = context.Platform.baseDamage;
            int enhancementLevel = context.SourceCardInstance?.EnhancementLevel ?? 0;
            float enhancedBaseDamage = baseDamage * (1f + enhancementLevel * 0.1f);
            float finalDamage = enhancedBaseDamage * (1 + context.Caster.FinalDamageBonus / 100f);

            zoneController.Initialize(ZoneDuration, PullRadius, PullForce, finalDamage, PulseSpeed, MinPulseScaleRatio, DamageTickInterval);
        }
    }
}