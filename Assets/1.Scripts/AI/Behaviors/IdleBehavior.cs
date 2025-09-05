// ���: ./TTttTT/Assets/1.Scripts/AI/Behaviors/IdleBehavior.cs
using UnityEngine;

/// <summary>
/// [�ൿ ��ǰ] �ƹ��͵� ���� �ʰ� ���ڸ��� ����, �ٸ� �ൿ���� ��ȯ�� ���Ǹ� Ȯ���ϴ� ���� �⺻���� �ൿ�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Idle")]
public class IdleBehavior : MonsterBehavior
{
    public override void OnEnter(MonsterController monster)
    {
        // �� �ൿ���� ������ ��, ������ �������� ������ ����ϴ�.
        monster.rb.velocity = Vector2.zero;
    }

    public override void OnExecute(MonsterController monster)
    {
        // �� ������ Ư���� �� ���� �����Ƿ�, �ٸ� �ൿ���� ��ȯ�� ������ �Ǿ����� �˻縸 �մϴ�.
        CheckTransitions(monster);
    }

    public override void OnExit(MonsterController monster)
    {
        // �ٸ� �ൿ���� �Ѿ �� Ư���� ������ ���� �����ϴ�.
    }
}