// ���� ���: Assets/1.Scripts/Core/PlayerRunData.cs (���� ����)
using System;
using System.Collections.Generic;
using UnityEngine; // [SerializeField]�� ����ϱ� ���� �ʿ��մϴ�.

/// <summary>
/// �� ���� ���� �÷���(Run) ���� �����Ǵ� ��� �÷��̾� �����͸� ��� Ŭ�����Դϴ�.
/// �������ֽ� ��� ���� ������ ��ȿ�� ���� ����� ���ԵǾ� �ֽ��ϴ�.
/// </summary>
[Serializable]
public class PlayerRunData
{

    // �� ��(Run)�� ����� �Ǵ� ĳ������ ���� �������Դϴ�.
    public CharacterDataSO characterData;

    // �� ��(Run)���� ���� ĳ������ �⺻ �ɷ�ġ�Դϴ�.
    public BaseStats baseStats;

    // ���� ü���Դϴ�. ���� �ٲ� �� ���� �����˴ϴ�.
    public float currentHealth;

    // 현재 최대 체력입니다. 이 또한 런타임에 변경될 수 있습니다.
    public float maxHealth;

    // (2�ܰ迡�� ������ ������) ���� ������ ī�� ���
    public List<CardInstance> ownedCards = new List<CardInstance>();

    // (2�ܰ迡�� ������ ������) ���� ������ ī�� ���
    public List<CardInstance> equippedCards = new List<CardInstance>();

    // (���� Ȯ��) ���� ������ ���� ���
    public List<ArtifactDataSO> ownedArtifacts = new List<ArtifactDataSO>();

    /// <summary>
    /// �������ֽ� ���, �� �����Ͱ� ��ȿ�� �������� Ȯ���ϴ� �޼����Դϴ�.
    /// ���� ���, ĳ���� ������ ���� ��� ��ȿ���� ���� �����ͷ� �Ǵ��� �� �ֽ��ϴ�.
    /// </summary>
    public bool IsValid()
    {
        bool isValid = characterData != null && baseStats != null && currentHealth >= 0;
        if (!isValid)
        {
            Debug.LogError("[PlayerRunData] ������ ��ȿ�� ���� ����! ĳ���ͳ� �⺻ ���� ������ �����ϴ�.");
        }
        return isValid;
    }
}