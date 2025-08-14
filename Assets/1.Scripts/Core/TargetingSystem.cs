// --- ���ϸ�: TargetingSystem.cs ---

using UnityEngine;
using System.Linq;

public static class TargetingSystem
{
    private static MonsterController[] allMonsters;

    public static Transform FindTarget(TargetingType type, Transform origin)
    {
        allMonsters = Object.FindObjectsOfType<MonsterController>();

        if (allMonsters == null || allMonsters.Length == 0)
        {
            return null;
        }

        // [����] ��Ȱ��ȭ�� ����(��: Ǯ�� �ִ� ����)�� Ÿ�ٿ��� �����ϵ��� ���͸� �߰�
        var activeMonsters = allMonsters.Where(m => m.gameObject.activeInHierarchy).ToArray();
        if (activeMonsters.Length == 0) return null;

        switch (type)
        {
            case TargetingType.Nearest:
                return activeMonsters.OrderBy(m => Vector3.Distance(origin.position, m.transform.position)).FirstOrDefault()?.transform;

            case TargetingType.HighestHealth:
                // [����] Y��ǥ ��� ���� ������ currentHealth�� �������� ü���� ���� ���� ���� ã���� ����
                return activeMonsters.OrderByDescending(m => m.currentHealth).FirstOrDefault()?.transform;

            case TargetingType.LowestHealth:
                // [����] Y��ǥ ��� ���� ������ currentHealth�� �������� ü���� ���� ���� ���� ã���� ����
                return activeMonsters.OrderBy(m => m.currentHealth).FirstOrDefault()?.transform;

            case TargetingType.Random:
                return activeMonsters[Random.Range(0, activeMonsters.Length)].transform;

            case TargetingType.Forward:
            default:
                return null;
        }
    }
}