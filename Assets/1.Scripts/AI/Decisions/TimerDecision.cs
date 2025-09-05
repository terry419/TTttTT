// ���: ./TTttTT/Assets/1.Scripts/AI/Decisions/TimerDecision.cs
using UnityEngine;

/// <summary>
/// [���� ��ǰ] ���� �ൿ�� ������ �� ������ �ð��� ����ߴ��� �Ǵ��ϴ� 'Ÿ�̸� ����'�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Timer")]
public class TimerDecision : Decision
{
    [Tooltip("�Ǵ� ������ �Ǵ� �ð�(��)�Դϴ�.")]
    public float duration = 5f;

    public override bool Decide(MonsterController monster)
    {
        // MonsterController�� ����� �� stateTimer(���� �ൿ ��� �ð�)�� ����մϴ�.
        // ��� �ð��� ������ duration���� ũ�ų� ������ true�� ��ȯ�մϴ�.
        return monster.stateTimer >= duration;
    }
}