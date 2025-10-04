// ��õ ���: Assets/1.Scripts/Data/Shared/BasicInfo.cs
using UnityEngine.Localization;
using System;
using UnityEngine.Localization;
using UnityEngine;

/// <summary>
/// ī���� �̸�, ������, Ÿ�� �� UI ǥ�ÿ� �ʿ��� �⺻ ������ ��� ���� Ŭ�����Դϴ�.
/// </summary>
[Serializable]
public class BasicInfo
{
    [Tooltip("ī�� ID (��: warrior_basic_001)")]
    public string cardID;
    [Tooltip("UI�� ǥ�õ� ī�� �̸�")]
    public LocalizedString cardName; // string -> LocalizedString ���� ����
    [Tooltip("ī�� �߾ӿ� ǥ�õ� �Ϸ���Ʈ")]
    public Sprite cardIllustration;
    [Tooltip("ī���� Ÿ�� (���� �Ǵ� ���)")]
    public CardType type;
    [Tooltip("ī���� ���")]
    public CardRarity rarity;
    [Tooltip("ī�� ȿ�� ���� �ؽ�Ʈ"), TextArea(3, 5)]
    public LocalizedString effectDescription; // string -> LocalizedString ���� ����
}