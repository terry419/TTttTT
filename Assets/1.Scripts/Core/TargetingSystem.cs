using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class TargetingSystem
{
    // 기존 FindTarget 함수는 그대로 둡니다 (하위 호환성)
    public static Transform FindTarget(TargetingType type, Transform origin)
    {
        return FindTarget(type, origin, null);
    }

    // ★★★ [핵심 추가] 제외 목록을 받는 새로운 FindTarget 함수 ★★★
    public static Transform FindTarget(TargetingType type, Transform origin, HashSet<GameObject> exclusions)
    {
        var monsterManager = ServiceLocator.Get<MonsterManager>();
        if (monsterManager == null) return null;

        // [수정] 제외 목록(exclusions)에 포함되지 않은 몬스터만 필터링합니다.
        var availableMonsters = monsterManager.ActiveMonsters.Where(m => exclusions == null || !exclusions.Contains(m.gameObject)).ToList();

        if (availableMonsters.Count == 0) return null;

        switch (type)
        {
            case TargetingType.Nearest:
                return availableMonsters.OrderBy(m => Vector3.Distance(origin.position, m.transform.position)).FirstOrDefault()?.transform;

            case TargetingType.HighestHealth:
                return availableMonsters.OrderByDescending(m => m.currentHealth).FirstOrDefault()?.transform;

            case TargetingType.LowestHealth:
                return availableMonsters.OrderBy(m => m.currentHealth).FirstOrDefault()?.transform;

            case TargetingType.Random:
                return availableMonsters[Random.Range(0, availableMonsters.Count)].transform;

            case TargetingType.Forward:
            default:
                return null;
        }
    }
}