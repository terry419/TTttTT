// ���: ./TTttTT/Assets/1.Scripts/AI/Transition.cs
using System;
using UnityEngine;

/// <summary>
/// [����� AI �ý����� �ٽ� ���赵 3/3]
/// "� '����'�� ����, � '�ൿ'���� ��ȯ�� ���ΰ�?" ��� ���� ��Ģ�� �����ϴ� ������ �����Դϴ�.
/// [System.Serializable]�� �� Ŭ������ Unity Inspector â�� ǥ�õǰ� ���ִ� C#�� Ư�� ����Դϴ�.
/// </summary>
[Serializable]
public class Transition
{
    [Tooltip("�ൿ ��ȯ ���θ� �Ǵ��� '����' ��ǰ(.asset)�� ���⿡ �����մϴ�.")]
    public Decision decision;

    [Tooltip("�� '����'�� ����� '��'�� ���, �������� ��ȯ�� '�ൿ' ��ǰ(.asset)�� ���⿡ �����մϴ�.")]
    public MonsterBehavior nextBehavior;
}