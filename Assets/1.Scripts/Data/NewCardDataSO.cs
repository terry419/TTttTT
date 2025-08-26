// ��õ ���: Assets/1.Scripts/Data/NewCardDataSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;

/// <summary>
/// v8.0 ��Ű��ó�� �ٽ��� �Ǵ� '�÷���' ScriptableObject�Դϴ�.
/// ī���� �⺻ �߻� ���, �нú� ����, �׸��� ������ ��� ���(��ǰ)���� ������ �����մϴ�.
/// </summary>
[CreateAssetMenu(fileName = "NewCard_", menuName = "GameData/v8.0/New Card Platform")]
public class NewCardDataSO : ScriptableObject
{
    [Header("[1] �⺻ ���� (UI ǥ�ÿ�)")]
    public BasicInfo basicInfo;

    [Header("[2] �нú� �ɷ�ġ (���� �� ����)")]
    public StatModifiers statModifiers;

    [Header("[3] �÷��� �⺻ �߻� ���")]
    [Tooltip("�� ���� �߻��ϴ� ����ü�� �� �����Դϴ�.")]
    public int projectileCount = 1;
    [Tooltip("����ü�� ������ �� �����Դϴ�. 0�̸� ��� ����ü�� ���� �������� �����ϴ�.")]
    public float spreadAngle = 0f;
    [Tooltip("�� ī�尡 ����ϴ� ��� �������� �� ���� �̸� �ε������� ���� ���� �����Դϴ�.")]
    public int preloadCount = 10;
    [Tooltip("�� ī�尡 �߻��ϴ� ��� ȿ���� �⺻ ���ط��Դϴ�.")]
    public float baseDamage = 10f;
    [Tooltip("�� ī�尡 �߻��ϴ� �⺻ ����ü�� �ӵ��Դϴ�.")]
    public float baseSpeed = 10f;


    [Header("[4] ��� ���� ���� (��� ��ǰ)")]
    [Tooltip("�� �÷����� ������ ��� ���(CardEffectSO)���� ���⿡ ����մϴ�.")]
    public List<ModuleEntry> modules;


    [Header("[5] ��Ÿ ����")]
    [Tooltip("���� �÷��� �� �귿 ��� �� ī�尡 ���õ� Ȯ�� ����ġ�Դϴ�.")]
    public float selectionWeight = 1f;
    [Tooltip("���� ���� �� �������� ������ Ȯ�� ����ġ�Դϴ�.")]
    public float rewardAppearanceWeight;
    [Tooltip("�� ī�带 �ر��ϱ� ���� �����Դϴ�. (�̱���)")]
    public string unlockCondition;


    /// <summary>
    /// EffectExecutor�� �߻翡 �ʿ��� �⺻ ����� ��û�� �� ����ϴ� ���� �޼ҵ��Դϴ�.
    /// </summary>
    /// <returns>�߻� ����� ���� FiringSpec ����ü</returns>
    public FiringSpec GetFiringSpecs()
    {
        Log.Print($"[NewCardDataSO] '{basicInfo.cardName}'�� FiringSpec ��û. �⺻ ���ط�: {baseDamage}");
        // ����� FiringSpec�� baseDamage�� ������, ���� ����ü ������ ���� �� Ȯ��� �� �ֽ��ϴ�.
        return new FiringSpec
        {
            baseDamage = this.baseDamage
        };
    }
}

/// <summary>
/// NewCardDataSO�� �ν����Ϳ��� ����� ���� �����ϱ� ���� Serializable Ŭ�����Դϴ�.
/// </summary>
[Serializable]
public class ModuleEntry
{
    [Tooltip("�ν����Ϳ��� �� ����� ������ ���� �˾ƺ� �� �ֵ��� ������ �����ϼ���.")]
    public string description;

    [Tooltip("���� ��� ������ ��� �ִ� CardEffectSO ������ ���⿡ �����մϴ�.")]
    public AssetReferenceT<CardEffectSO> moduleReference;
}