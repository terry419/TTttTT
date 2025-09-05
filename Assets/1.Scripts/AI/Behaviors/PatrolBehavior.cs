// ���: ./TTttTT/Assets/1.Scripts/AI/Behaviors/PatrolBehavior.cs
using UnityEngine;

/// <summary>
/// [�ൿ ��ǰ] �ڽ��� ���� ��ġ �ֺ��� �������� ���ƴٴϸ� �����ϴ� �ൿ�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Patrol")]
public class PatrolBehavior : MonsterBehavior
{
    [Tooltip("������ �ݰ��Դϴ�. ���� ��ġ�κ��� �� �Ÿ� �ȿ��� �����Դϴ�.")]
    public float patrolRadius = 10f;
    [Tooltip("���� �� �̵� �ӵ� �����Դϴ�.")]
    public float speedMultiplier = 0.5f;

    // �� ������ ���� ��ü���� ���� ����Ǿ�� �ϹǷ�, ���� ��Ʈ�ѷ��� �����ؾ� ������,
    // ���⼭�� ������ ������ ���� static�� ����մϴ�. (���߿� �� ������ AI�� ���� �� ������ �� �ֽ��ϴ�.)
    private Vector3 _patrolTargetPosition;

    public override void OnEnter(MonsterController monster)
    {
        // ���� �ൿ�� �������ڸ��� ù ��° ���� ��ǥ ������ ���մϴ�.
        UpdatePatrolTarget(monster);
    }

    public override void OnExecute(MonsterController monster)
    {
        // ��ǥ ������ ���� �����ߴٸ�, ���� ��ǥ ������ ���� ���մϴ�.
        if (Vector3.SqrMagnitude(monster.transform.position - _patrolTargetPosition) < 1f)
        {
            UpdatePatrolTarget(monster);
        }

        // ��ǥ ������ ���� �̵��մϴ�.
        Vector2 direction = (_patrolTargetPosition - monster.transform.position).normalized;
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;

        // ���� �߿� �ٸ� �ൿ���� ��ȯ�� ����(�÷��̾� �߰� ��)�� Ȯ���մϴ�.
        CheckTransitions(monster);
    }

    // ������ ���ο� ��ǥ ������ ����ϴ� �Լ�
    private void UpdatePatrolTarget(MonsterController monster)
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        _patrolTargetPosition = monster.startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
    }
}