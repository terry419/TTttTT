// --- 파일명: TargetingSystem.cs ---

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

        // [수정] 비활성화된 몬스터(예: 풀에 있는 몬스터)는 타겟에서 제외하도록 필터링 추가
        var activeMonsters = allMonsters.Where(m => m.gameObject.activeInHierarchy).ToArray();
        if (activeMonsters.Length == 0) return null;

        switch (type)
        {
            case TargetingType.Nearest:
                return activeMonsters.OrderBy(m => Vector3.Distance(origin.position, m.transform.position)).FirstOrDefault()?.transform;

            case TargetingType.HighestHealth:
                // [수정] Y좌표 대신 실제 몬스터의 currentHealth를 기준으로 체력이 가장 높은 적을 찾도록 수정
                return activeMonsters.OrderByDescending(m => m.currentHealth).FirstOrDefault()?.transform;

            case TargetingType.LowestHealth:
                // [수정] Y좌표 대신 실제 몬스터의 currentHealth를 기준으로 체력이 가장 낮은 적을 찾도록 수정
                return activeMonsters.OrderBy(m => m.currentHealth).FirstOrDefault()?.transform;

            case TargetingType.Random:
                return activeMonsters[Random.Range(0, activeMonsters.Length)].transform;

            case TargetingType.Forward:
            default:
                return null;
        }
    }
}