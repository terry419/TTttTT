using UnityEngine;
using System.Linq; // Linq ����� ���� �߰�

/// <summary>
/// �پ��� ���ǿ� �´� ���� ã�� ����� �����ϴ� ���� Ŭ�����Դϴ�.
/// </summary>
public static class TargetingSystem
{
    // ���� �ִ� ��� ���͸� ��Ƶ� �迭
    private static MonsterController[] allMonsters;

    /// <summary>
    /// ������ ���� ��Ŀ� ���� ��ǥ ����� ã���ϴ�.
    /// </summary>
    public static Transform FindTarget(TargetingType type, Transform origin)
    {
        // ���� �ִ� ��� ���͸� ã���ϴ�. (������ ���� �� ������ ȣ������ �ʵ��� ����)
        allMonsters = Object.FindObjectsOfType<MonsterController>();

        if (allMonsters == null || allMonsters.Length == 0)
        {
            return null; // ���Ͱ� ������ null ��ȯ
        }

        switch (type)
        {
            case TargetingType.Nearest:
                return allMonsters.OrderBy(m => Vector3.Distance(origin.position, m.transform.position)).FirstOrDefault()?.transform;

            case TargetingType.HighestHealth:
                // MonsterController�� public float currentHealth; ������ �־�� �մϴ�.
                // return allMonsters.OrderByDescending(m => m.currentHealth).FirstOrDefault()?.transform;
                return allMonsters.OrderByDescending(m => m.transform.position.y).FirstOrDefault()?.transform; // �ӽ�: ü�� ��� ���̷� ����

            case TargetingType.LowestHealth:
                // return allMonsters.OrderBy(m => m.currentHealth).FirstOrDefault()?.transform;
                return allMonsters.OrderBy(m => m.transform.position.y).FirstOrDefault()?.transform; // �ӽ�: ü�� ��� ���̷� ����

            case TargetingType.Random:
                return allMonsters[Random.Range(0, allMonsters.Length)].transform;

            case TargetingType.Forward:
            default:
                return null; // ���� �߻�� ��ǥ�� �ʿ� �����Ƿ� null ��ȯ
        }
    }
}