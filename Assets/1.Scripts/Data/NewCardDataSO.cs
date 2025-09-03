// ���� ���: Assets/1.Scripts/Data/NewCardDataSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewCard_", menuName = "GameData/v8.0/New Card Platform")]
public class NewCardDataSO : ScriptableObject
{
    [Header("[1] �⺻ ���� (UI ǥ���)")]
    public BasicInfo basicInfo;

    [Header("[2] ���� ���ʽ� (�÷��̾� ��ȭ)")]
    public StatModifiers statModifiers;

    [Header("[3] ī�� ���� �߻� ����")]
    [Tooltip("�� ī���� ���� ���� �ֱ�(��)�Դϴ�. 1.0�� 1�ʿ� �� ��, 0.2�� 1�ʿ� 5���� �ǹ��մϴ�.")]
    public float attackInterval = 1.0f; 

    [Tooltip("üũ ��, ���� ����ü�� �� ���͸� ���� �� Ÿ���� �� �ֽ��ϴ�.")]
    public bool allowMultipleHits = false;

    [Tooltip("�� ���� �߻��ϴ� ����ü�� �����Դϴ�.")]
    public int projectileCount = 1;

    [Tooltip("����ü�� ������ �����Դϴ�. 0�̸� ��� ����ü�� �� �������� �����ϴ�.")]
    public float spreadAngle = 0f;

    [Tooltip("�� ī�尡 ����ϴ� ����ü�� �̸� �� �� �����ص��� ���մϴ�.")]
    public int preloadCount = 10;

    [Tooltip("ī�尡 �߻��ϴ� ����ü�� �⺻ ���ط��Դϴ�.")]
    public float baseDamage = 10f;

    [Tooltip("ī�尡 �߻��ϴ� ����ü�� �⺻ �ӵ��Դϴ�.")]
    public float baseSpeed = 10f;

    [Header("[4] ���� ȿ�� (���)")]
    [Tooltip("�� ī�忡 ������ Ư�� ȿ��(CardEffectSO) ����Դϴ�.")]
    public List<ModuleEntry> modules;

    [Header("[5] ��Ÿ ��Ÿ ����")]
    [Tooltip("ī�� �������� �� ī�尡 ������ Ȯ�� ����ġ�Դϴ�.")]
    public float selectionWeight = 1f;

    [Tooltip("���� ��Ͽ� �� ī�尡 ������ Ȯ�� ����ġ�Դϴ�.")]
    public float rewardAppearanceWeight;

    [Tooltip("�� ī�带 �ر��ϱ� ���� �����Դϴ�. (�̱���)")]
    public string unlockCondition;

    public ICardAction CreateAction()
    {
        return new ModuleAction();
    }
}

[Serializable]
public class ModuleEntry
{
    [Tooltip("�� ��⿡ ���� �����Դϴ�.")]
    public string description;

    [Tooltip("������ CardEffectSO ����� ���⿡ �����մϴ�.")]
    public AssetReferenceT<CardEffectSO> moduleReference;
}