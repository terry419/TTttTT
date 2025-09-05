// ���: ./TTttTT/Assets/1/Scripts/AI/Decisions/HealthDecision.cs
using UnityEngine;

/// <summary>
/// [�ű� ���� ��ǰ] �ڽ��� ���� ü�� ������ �������� �Ǵ��ϴ� '����'�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Health")]
public class HealthDecision : Decision
{
    [Tooltip("�Ǵ� ������ �Ǵ� ü���� ����(%)�Դϴ�. 30���� �����ϸ� 30%�� �ǹ��մϴ�.")]
    [Range(0f, 100f)]
    public float healthPercentageThreshold = 30f;

    [Tooltip("üũ: ü���� ����ġ '����'�� �� �� / üũ ����: ü���� ����ġ '�̻�'�� �� ��")]
    public bool triggerWhenBelow = true;

    public override bool Decide(MonsterController monster)
    {
        if (monster.monsterStats == null) return false;

        // ���� ü�� ������ ����մϴ� (0.0 ~ 1.0 ������ ��)
        float currentHealthRatio = monster.monsterStats.CurrentHealth / monster.monsterStats.FinalMaxHealth;
        // Inspector���� �Է��� �ۼ�Ʈ ���� ������ ��ȯ�մϴ�.
        float thresholdRatio = healthPercentageThreshold / 100f;

        if (triggerWhenBelow)
        {
            // ���� ü���� ����ġ '����'�� �� true�� ��ȯ�մϴ�.
            return currentHealthRatio <= thresholdRatio;
        }
        else
        {
            // ���� ü���� ����ġ '�̻�'�� �� true�� ��ȯ�մϴ�.
            return currentHealthRatio >= thresholdRatio;
        }
    }
}