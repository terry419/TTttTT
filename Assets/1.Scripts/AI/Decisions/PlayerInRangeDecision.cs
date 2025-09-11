// ���: ./TTttTT/Assets/1.Scripts/AI/Decisions/PlayerInRangeDecision.cs
using UnityEngine;

/// <summary>
/// [���� ��ǰ] �÷��̾ ������ ���� �ȿ� �ִ��� �Ǵ��ϴ� '����'�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Player In Range")]
public class PlayerInRangeDecision : Decision
{
    [Tooltip("�Ǵ� ������ �Ǵ� �Ÿ�(������)�Դϴ�.")]
    public float range = 10f;

    [Tooltip("üũ ��, �Ǵ� ����� �ݴ�� �������ϴ�. (���� '��'�� ������ true)")]
    public bool negate = false;

    public override bool Decide(MonsterController monster)
    {
        if (monster.targetTransform == null) return false;

        float sqrDistance = (monster.targetTransform.position - monster.transform.position).sqrMagnitude;
        bool isInRange = sqrDistance < range * range;
        bool result = negate ? !isInRange : isInRange;

        float distance = Mathf.Sqrt(sqrDistance);

        return result;
    }
}