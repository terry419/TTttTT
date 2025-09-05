// ���: ./TTttTT/Assets/1.Scripts/AI/Decisions/Decision.cs
using UnityEngine;

/// <summary>
/// [����� AI �ý����� �ٽ� ���赵 2/3]
/// '�÷��̾ ���� �ȿ� �ִ°�?'�� ���� ��� '����' ��ǰ���� ����� �ϴ� �⺻ ����(���赵)�Դϴ�.
/// ��� ���� ��ǰ�� �ݵ�� "��(true)" �Ǵ� "�ƴϿ�(false)" �� �ϳ��� ����ؾ� �մϴ�.
/// </summary>
public abstract class Decision : ScriptableObject
{
    /// <summary>
    /// ���� ������ ��Ȳ�� �Ǵ��Ͽ� �ൿ�� ��ȯ�ؾ� �ϸ� true, �ƴϸ� false�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="monster">�Ǵ��� ��ü�� �Ǵ� ������ '����'</param>
    /// <returns>�ൿ�� ��ȯ���� ���� (true/false)</returns>
    public abstract bool Decide(MonsterController monster);
}