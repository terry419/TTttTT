// ��õ ���: Assets/1.Scripts/Data/Shared/BasicInfo.cs
using System;
using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// ī���� �̸�, ������, Ÿ�� �� UI ǥ�ÿ� �ʿ��� �⺻ ������ ��� ���� Ŭ�����Դϴ�.
/// </summary>
[Serializable]
public class BasicInfo
{
    [Tooltip("ī���� ���� ID (��: warrior_basic_001)")]
    public string cardID;
    [Tooltip("UI�� ǥ�õ� ī���� �̸� (���ö���¡)")]
    public LocalizedString cardName;
    [Tooltip("ī�� �߾ӿ� ǥ�õ� ���� �Ϸ���Ʈ")]
    public Sprite cardIllustration;
    [Tooltip("ī���� Ÿ�� (���� �Ǵ� ����)")]
    public CardType type;
    [Tooltip("ī���� ��͵�")]
    public CardRarity rarity;
    [Tooltip("ī�� ȿ�� ���� �ؽ�Ʈ")]
    public LocalizedString effectDescription;
}