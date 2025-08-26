using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

/// <summary>
/// ������ ���� ȿ�� ��� �� �ϳ��� �������� �����Ͽ� �����ϴ� �� ����Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_Random_", menuName = "GameData/v8.0/Modules/RandomEffect")]
public class RandomEffectSO : CardEffectSO
{
    [Header("���� ȿ�� Ǯ")]
    [Tooltip("�������� ���õ� ȿ�� ��� ������ ����Դϴ�.")]
    public List<AssetReferenceT<CardEffectSO>> effectPool;

    /// <summary>
    /// effectPool���� �������� ��� �ϳ��� ��� EffectExecutor���� ������ �����մϴ�.
    /// �� ����� ���� ������ EffectExecutor���� ó���ؾ� �մϴ�.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ����. ȿ�� Ǯ ����: {effectPool?.Count ?? 0}��.");
        // �� ����� �����͸� �����ϰ�, ���� ������ EffectExecutor�� ����մϴ�.
        // EffectExecutor�� �� ����� ������, effectPool �� �ϳ��� ��� �ٽ� Execute�� ȣ���ؾ� �մϴ�.
    }
}