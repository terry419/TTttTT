// ���� ���: Assets/Scripts/AI/Behaviors/ChaseBehavior.cs
using UnityEngine;

/// <summary>
/// [�ű� AI �ý����� ù ��° ��ǰ 1/1]
/// '�÷��̾ �߰��Ѵ�'�� ���� ����� �����ϴ� �ൿ(Behavior) ��ǰ�Դϴ�.
/// PoC �ܰ迡���� �� �ൿ �ϳ������� �� �ý����� ������ �����մϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Chase")]
public class ChaseBehavior : MonsterBehavior
{
    [Tooltip("�߰� �� ������ �⺻ �̵� �ӵ��� ������ �����Դϴ�. 1.0�� 100% �ӵ��� �ǹ��մϴ�.")]
    public float speedMultiplier = 1.0f;

    public override void OnEnter(MonsterController monster)
    {
        // 3�� ������(���� ���� �α�)�� ������ ���� �α��Դϴ�.
        // ���߿��� �� �α׸� �����ϴ� �ý����� ���������, PoC �ܰ迡���� �̷��� ���� ����մϴ�.
        Debug.Log($"[AI Log | Time: {Time.time:F2}] Monster: '{monster.gameObject.name}', Event: State Entered, State: 'ChaseBehavior'");
    }

    public override void OnExecute(MonsterController monster)
    {
        if (monster.playerTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = ((Vector3)monster.playerTransform.position - monster.transform.position).normalized;
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;
    }
}