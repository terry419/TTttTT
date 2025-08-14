using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가

/// <summary>
/// 다양한 조건에 맞는 적을 찾는 기능을 제공하는 정적 클래스입니다.
/// </summary>
public static class TargetingSystem
{
    // 씬에 있는 모든 몬스터를 담아둘 배열
    private static MonsterController[] allMonsters;

    /// <summary>
    /// 지정된 조준 방식에 따라 목표 대상을 찾습니다.
    /// </summary>
    public static Transform FindTarget(TargetingType type, Transform origin)
    {
        // 씬에 있는 모든 몬스터를 찾습니다. (성능을 위해 매 프레임 호출하지 않도록 주의)
        allMonsters = Object.FindObjectsOfType<MonsterController>();

        if (allMonsters == null || allMonsters.Length == 0)
        {
            return null; // 몬스터가 없으면 null 반환
        }

        switch (type)
        {
            case TargetingType.Nearest:
                return allMonsters.OrderBy(m => Vector3.Distance(origin.position, m.transform.position)).FirstOrDefault()?.transform;

            case TargetingType.HighestHealth:
                // MonsterController에 public float currentHealth; 변수가 있어야 합니다.
                // return allMonsters.OrderByDescending(m => m.currentHealth).FirstOrDefault()?.transform;
                return allMonsters.OrderByDescending(m => m.transform.position.y).FirstOrDefault()?.transform; // 임시: 체력 대신 높이로 정렬

            case TargetingType.LowestHealth:
                // return allMonsters.OrderBy(m => m.currentHealth).FirstOrDefault()?.transform;
                return allMonsters.OrderBy(m => m.transform.position.y).FirstOrDefault()?.transform; // 임시: 체력 대신 높이로 정렬

            case TargetingType.Random:
                return allMonsters[Random.Range(0, allMonsters.Length)].transform;

            case TargetingType.Forward:
            default:
                return null; // 정면 발사는 목표가 필요 없으므로 null 반환
        }
    }
}