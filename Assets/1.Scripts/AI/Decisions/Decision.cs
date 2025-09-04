// ���� ���: Assets/Scripts/AI/Decisions/Decision.cs
using UnityEngine;

/// <summary>
/// [�ű� AI �ý����� ���� 2/4]
/// ��� '�Ǵ�' ��ǰ('�÷��̾ ���� �ȿ� �ִ°�?' ��)�� ��ӹ޾ƾ� �ϴ� �߻� Ŭ�����Դϴ�.
/// �̴� '����' ��ǰ�� �԰��� ���ϴ� �Ͱ� �����ϴ�.
/// ��� ������ 'Decide'��� ����� ���� "��" �Ǵ� "�ƴϿ�" (true/false) �� �ϳ��θ� ����ؾ� �մϴ�.
/// </summary>
public abstract class Decision : ScriptableObject
{
    /// <summary>
    /// ���� ��Ȳ�� �Ǵ��Ͽ� �ൿ�� ��ȯ�ؾ� �ϸ� true, �ƴϸ� false�� ��ȯ�մϴ�.
    /// </summary>
    public abstract bool Decide(MonsterController monster);
}