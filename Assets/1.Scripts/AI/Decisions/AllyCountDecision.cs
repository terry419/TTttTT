// ���: ./TTttTT/Assets/1/Scripts/AI/Decisions/AllyCountDecision.cs
using UnityEngine;

/// <summary>
/// [�ű� ���� ��ǰ] �ֺ��� �Ʊ� ���� ���ڸ� �������� �Ǵ��ϴ� '����'�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Ally Count")]
public class AllyCountDecision : Decision
{
    [Tooltip("�Ʊ��� Ž���� �ֺ� �ݰ��Դϴ�.")]
    public float checkRadius = 10f;

    [Tooltip("�Ǵ� ������ �Ǵ� �Ʊ��� �����Դϴ�.")]
    public int allyCountThreshold = 2;

    [Tooltip("üũ: �Ʊ��� ����ġ '�̸�'�� �� �� / üũ ����: �Ʊ��� ����ġ '�̻�'�� �� ��")]
    public bool triggerWhenBelow = true;

    // �ֺ� ���͸� ���� �ӽ� �迭 (�Ź� ���� �������� �ʾ� ���ɿ� �����մϴ�)
    private static Collider2D[] _monsterColliders = new Collider2D[50];

    public override bool Decide(MonsterController monster)
    {
        // Physics2D.OverlapCircleNonAlloc�� ������ ��ġ�� �ִ� ��� �ݶ��̴��� ã�� �迭�� ����ְ�, �� ���ڸ� ��ȯ�մϴ�.
        int hitCount = Physics2D.OverlapCircleNonAlloc(monster.transform.position, checkRadius, _monsterColliders, LayerMask.GetMask("Monster"));

        // ã�� ���ڿ��� �ڱ� �ڽ�(1)�� �� ���� ������ �ֺ� �Ʊ��� �����Դϴ�.
        int allyCount = hitCount - 1;

        if (triggerWhenBelow)
        {
            // �ֺ� �Ʊ��� ����ġ '�̸�'�� �� true�� ��ȯ�մϴ�.
            return allyCount < allyCountThreshold;
        }
        else
        {
            // �ֺ� �Ʊ��� ����ġ '�̻�'�� �� true�� ��ȯ�մϴ�.
            return allyCount >= allyCountThreshold;
        }
    }
}