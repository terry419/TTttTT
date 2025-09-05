// ���: ./TTttTT/Assets/1/Scripts/AI/Decisions/CooldownDecision.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [�ű� ���� ��ǰ] Ư�� �ൿ�� ���� ���ð�(��Ÿ��)�� �����ϴ� '����'�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Cooldown")]
public class CooldownDecision : Decision
{
    [Tooltip("���� ���ð�(��)�Դϴ�.")]
    public float cooldown = 8f;

    public override bool Decide(MonsterController monster)
    {
        // 1. ������ ���� ��Ÿ�� �������� Ȯ���մϴ�.
        // �� Decision ����(this)�� ���� �˶��� ������ �ִ��� ã�ƺ��ϴ�.
        if (monster.cooldownTimers.TryGetValue(this, out float lastUseTime))
        {
            // 2. �˶��� ������ �ִٸ�, ���� �ð��� ������ ��� �ð��� ���մϴ�.
            if (Time.time - lastUseTime >= cooldown)
            {
                // 3. ��Ÿ���� �� �����ٸ�, "��� ����!"(true)�� ��ȯ�ϰ�,
                //    ��� ���� ��Ÿ���� ���� ���� �ð��� ���� ����մϴ�.
                monster.cooldownTimers[this] = Time.time;
                return true;
            }
            else
            {
                // 4. ���� ��Ÿ���� ���� �ִٸ�, "��� �Ұ�!"(false)�� ��ȯ�մϴ�.
                return false;
            }
        }
        else
        {
            // 5. �� ��ų�� �� ���� �� ���� ���ٸ�, �翬�� "��� ����!"(true)�� ��ȯ�ϰ�,
            //    ���� ��� �ð��� ����մϴ�.
            monster.cooldownTimers.Add(this, Time.time);
            return true;
        }
    }
}