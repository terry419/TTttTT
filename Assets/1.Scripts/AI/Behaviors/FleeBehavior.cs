// ���: ./TTttTT/Assets/1.Scripts/AI/Behaviors/FleeBehavior.cs
using UnityEngine;

/// <summary>
/// [�ൿ ��ǰ] ������κ��� �־����� �������� �̵��ϴ� �ൿ�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Flee")]
public class FleeBehavior : MonsterBehavior
{
    [Tooltip("���� �� �⺻ �̵� �ӵ��� ������ �����Դϴ�.")]
    public float speedMultiplier = 1.2f; // ���� ������ �߰ݺ��� �ణ ������ �����մϴ�.

    public override void OnExecute(MonsterController monster)
    {
        if (monster.playerTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        // 1. �÷��̾�κ��� '�־�����' ���� ���͸� ����մϴ�. (�ڽ��� ��ġ - �÷��̾� ��ġ)
        Vector2 direction = (monster.transform.position - monster.playerTransform.position).normalized;

        // 2. ���� �������� �̵���ŵ�ϴ�.
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;

        // 3. ����ġ�ٰ� �ٸ� �ൿ���� ��ȯ�� ������ �Ǿ����� Ȯ���մϴ�. (��: �÷��̾ ����� �־���)
        CheckTransitions(monster);
    }
}