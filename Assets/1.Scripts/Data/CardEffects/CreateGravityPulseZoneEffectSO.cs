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
    [Tooltip("�� ������ ���ϴ� �Ƶ��� �⺻ ���ط��Դϴ�.")]
    public float pulseDamage = 15f;

    [Tooltip("üũ ��, �÷��̾��� ���� ���ݷ� ���ʽ�(%)�� �� ������ ���ط��� ������ �ݴϴ�.")]
    public bool scalesWithPlayerDamageBonus = true;


    [Tooltip("�ּ� ũ�� �����Դϴ�. (��: 0.2�� �ִ� �ݰ��� 20%���� �۾���)")]
    [Range(0f, 1f)]
    public float MinPulseScaleRatio = 0.2f;

    [Tooltip("������� ���ذ� �߻��ϴ� �ֱ�(��)�Դϴ�. (����: 0.5)")]
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
            Vector3 spawnPosition = context.HitPosition;
            zoneGO.transform.position = spawnPosition;

            float finalDamage = this.pulseDamage;
            if (scalesWithPlayerDamageBonus)
            {
                finalDamage *= (1 + context.Caster.FinalDamageBonus / 100f);
            }

            zoneController.Initialize(ZoneDuration, PullRadius, PullForce, finalDamage, MinPulseScaleRatio, DamageTickInterval);
        }
    }
}