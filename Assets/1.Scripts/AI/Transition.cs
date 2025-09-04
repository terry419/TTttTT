// ���� ���: Assets/Scripts/AI/Transition.cs
using System;
using UnityEngine;

/// <summary>
/// [�ű� AI �ý����� ���� 3/4]
/// "� '�Ǵ�'�� ���� ��, ���� '�ൿ'�� �����ΰ�?"��� �ϳ��� ��Ģ�� �����ϴ� ������ �����Դϴ�.
/// </summary>
[Serializable]
public class Transition
{
    [Tooltip("'����'�� �ش��ϴ� '�Ǵ�' ��ǰ(Decision)�� ���⿡ �����մϴ�.")]
    public Decision decision;

    [Tooltip("���� '�Ǵ�'�� ���� ���, ��ȯ�� ���� '�ൿ' ��ǰ(Behavior)�� ���⿡ �����մϴ�.")]
    public MonsterBehavior nextBehavior;
}