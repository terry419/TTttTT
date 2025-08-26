using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// �ĵ�, ���� �� ���� ȿ���� �����ϴ� ����Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_Area_", menuName = "GameData/v8.0/Modules/AreaEffect")]
public class AreaEffectSO : CardEffectSO
{
    [Header("���� ȿ�� ������")]
    [Tooltip("������ �ĵ� �Ǵ� ���� ȿ���� ������ (DamagingZone.cs ����)")]
    public AssetReferenceGameObject effectPrefabRef;

    [Header("�ĵ�/���� ȿ�� ����")]
    [Tooltip("�ĵ�/������ �� ���� �ð� (��)")]
    public float effectDuration = 3f;

    [Tooltip("�ĵ�/������ Ȯ�� �ӵ� (�ʴ�)")]
    public float effectExpansionSpeed = 1f;

    [Tooltip("�ĵ�/������ Ȯ���ϴ� �ð� (��)")]
    public float effectExpansionDuration = 3.1f;

    [Tooltip("���� ����� �� ƽ ������ ���� (��)")]
    public float effectTickInterval = 100.0f;

    [Tooltip("���� ����� �� ƽ �� ���ط�. 0���� ũ�� ����, 0�̸� ���� Ÿ�� �ĵ����� ���ֵ˴ϴ�.")]
    public float damagePerTick = 0f;

    /// <summary>
    /// AreaEffect�� ������ �����Ͽ� �������� �����ϰ� �ʱ�ȭ�մϴ�.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ����.");

        // �� ����� ���� Execute �ܰ迡�� ���� ������ ������ �����ϴ�.
        // ������ ���� �� �ʱ�ȭ�� EffectExecutor�� ����ϰ� �˴ϴ�.
    }
}