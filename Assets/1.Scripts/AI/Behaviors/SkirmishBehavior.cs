// ���: ./TTttTT/Assets/1/Scripts/AI/Behaviors/SkirmishBehavior.cs
using UnityEngine;

/// <summary>
/// [�ű� ��� �ൿ ��ǰ] ���� ������ �Ÿ��� �����ϱ� ���� ���� �� ���� �ݺ��ϴ� '�Ÿ� ����' �ൿ�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Skirmish")]
public class SkirmishBehavior : MonsterBehavior
{
    [Header("�Ÿ� ����")]
    [Tooltip("�� �Ÿ����� ��������� �����մϴ�.")]
    public float tooCloseDistance = 7f;
    [Tooltip("�� �Ÿ����� �־����� �����մϴ�.")]
    public float tooFarDistance = 15f;
    [Tooltip("�Ÿ� ���� �� �̵� �ӵ� �����Դϴ�.")]
    public float speedMultiplier = 1.0f;

    public override void OnExecute(MonsterController monster)
    {
        if (monster.playerTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        Vector3 monsterPos = monster.transform.position;
        Vector3 playerPos = monster.playerTransform.position;
        float distanceSq = Vector3.SqrMagnitude(playerPos - monsterPos);

        // 1. �÷��̾�� �ʹ� ����� ���: ����
        if (distanceSq < tooCloseDistance * tooCloseDistance)
        {
            Vector2 direction = (monsterPos - playerPos).normalized;
            monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;
        }
        // 2. �÷��̾�� �ʹ� �� ���: ����
        else if (distanceSq > tooFarDistance * tooFarDistance)
        {
            Vector2 direction = (playerPos - monsterPos).normalized;
            monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;
        }
        // 3. ������ �Ÿ��� ���: �������� ����
        else
        {
            monster.rb.velocity = Vector2.zero;
        }

        // �Ÿ� ���� �ൿ�� �ϴٰ�, �ٸ� �ൿ(��: ��ȯ)���� ��ȯ�� ������ �Ǿ����� Ȯ���մϴ�.
        CheckTransitions(monster);
    }
}