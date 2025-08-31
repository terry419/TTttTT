using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class TargetingSystem
{
    public static Transform FindTarget(TargetingType type, Transform origin)
    {
        return FindTarget(type, origin, null);
    }

    public static Transform FindTarget(TargetingType type, Transform origin, HashSet<GameObject> exclusions)
    {
        var monsterManager = ServiceLocator.Get<MonsterManager>();
        if (monsterManager == null) return null;

        var availableMonsters = monsterManager.ActiveMonsters.Where(m => exclusions == null || !exclusions.Contains(m.gameObject)).ToList();

        if (availableMonsters.Count == 0) return null;

        switch (type)
        {
            case TargetingType.Nearest:
                Transform nearestMonster = null;
                float minDistanceSq = float.MaxValue;

                foreach (var monster in availableMonsters)
                {
                    // Vector3.Distance는 제곱근 연산이 포함되어 느리므로, 제곱 거리를 비교하는 것이 훨씬 빠릅니다.
                    float distanceSq = (origin.position - monster.transform.position).sqrMagnitude;
                    if (distanceSq < minDistanceSq)
                    {
                        minDistanceSq = distanceSq;
                        nearestMonster = monster.transform;
                    }
                }
                return nearestMonster;
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            case TargetingType.HighestHealth:
                // [ 변경] GetCurrentHealth() 메서드를 사용합니다.
                return availableMonsters.OrderByDescending(m => m.GetCurrentHealth()).FirstOrDefault()?.transform;
            case TargetingType.LowestHealth:
                // [ 변경] GetCurrentHealth() 메서드를 사용합니다.
                return availableMonsters.OrderBy(m => m.GetCurrentHealth()).FirstOrDefault()?.transform;
            case TargetingType.Random:
                return availableMonsters[Random.Range(0, availableMonsters.Count)].transform;
            case TargetingType.Forward:
            default:
                return null;
        }
    }
}