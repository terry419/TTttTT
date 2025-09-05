// ���: ./TTttTT/Assets/1.Scripts/AI/Behaviors/ChaseBehavior.cs
using UnityEngine;

/// <summary>
/// [�ൿ ��ǰ] ����� ���� ������ �ӵ��� �̵��ϴ� �ൿ�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Chase")]
public class ChaseBehavior : MonsterBehavior
{
    [Tooltip("�߰� �� �⺻ �̵� �ӵ��� ������ �����Դϴ�. 1.0�� 100% �ӵ��� �ǹ��մϴ�.")]
    public float speedMultiplier = 1.0f;

    public override void OnEnter(MonsterController monster)
    {
        // Ư���� �ʱ�ȭ �۾��� �����ϴ�.
    }

    public override void OnExecute(MonsterController monster)
    {
        // �÷��̾ ���ٸ� �������� �ʽ��ϴ�.
        if (monster.playerTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        // 1. �÷��̾� �������� ���ϴ� ���͸� ����մϴ�.
        Vector2 direction = (monster.playerTransform.position - monster.transform.position).normalized;

        // 2. ������ ���� �̵� �ӵ��� ������ ���Ͽ� �ӵ��� �����ϰ�, ���� ������ ���� �̵���ŵ�ϴ�.
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;

        // 3. �߰��� �ϴٰ� �ٸ� �ൿ���� ��ȯ�� ������ �Ǿ����� Ȯ���մϴ�. (��: �÷��̾ �ʹ� �־���)
        CheckTransitions(monster);
    }

    public override void OnExit(MonsterController monster)
    {
        // �߰� �ൿ�� ���� ��, Ȥ�� �� ������ ���ֱ� ���� �ӵ��� 0���� �ʱ�ȭ�մϴ�.
        monster.rb.velocity = Vector2.zero;
    }
}