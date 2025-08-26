// ��õ ���: Assets/1.Scripts/Data/Shared/StatModifiers.cs
using UnityEngine;
using System;

/// <summary>
/// ī�� ���� �� �÷��̾�� ����Ǵ� �������� ���� ���ʽ��� �����ϴ� ���� Ŭ�����Դϴ�.
/// </summary>
[Serializable]
public class StatModifiers
{
    [Tooltip("���ݷ¿� �������� % ����")]
    public float damageMultiplier;
    [Tooltip("���� �ӵ��� �������� % ����")]
    public float attackSpeedMultiplier;
    [Tooltip("�̵� �ӵ��� �������� % ����")]
    public float moveSpeedMultiplier;
    [Tooltip("�ִ� ü�¿� �������� % ����")]
    public float healthMultiplier;
    [Tooltip("ġ��Ÿ Ȯ���� �������� % ����")]
    public float critRateMultiplier;
    [Tooltip("ġ��Ÿ ���ط��� �������� % ����")]
    public float critDamageMultiplier;
}