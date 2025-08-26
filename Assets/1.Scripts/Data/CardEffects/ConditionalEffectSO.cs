using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Ư�� ������ �����Ǿ��� ���� ����� ȿ���� �ߵ���Ű�� �� ����Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_Conditional_", menuName = "GameData/v8.0/Modules/ConditionalEffect")]
public class ConditionalEffectSO : CardEffectSO
{
    [Header("���Ǻ� ���� ����")]
    [Tooltip("�� ����� ������ Ʈ���� �����Դϴ�. (��: OnCrit)")]
    public EffectTrigger condition;

    [Tooltip("������ �����Ǿ��� �� ����� ȿ�� ����Դϴ�.")]
    public AssetReferenceT<CardEffectSO> effectToTrigger;

    /// <summary>
    /// ���Ǻ� ��� ��ü�� �������� ������ �������� �ʽ��ϴ�.
    /// EffectExecutor�� �� ����� ���縦 �����ϰ�, 'condition'�� �����Ǿ��� �� 'effectToTrigger'�� ��� ���������� �մϴ�.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ����. ����: {condition}.");
        // �� ����� �����͸� �����ϴ� ���Ҹ� �մϴ�.
    }
}