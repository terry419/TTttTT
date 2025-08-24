// --- ϸ: TargetingSystem.cs ---

using UnityEngine;
using System.Linq;
using System.Collections.Generic; // Added for IReadOnlyList

public static class TargetingSystem
{
    public static Transform FindTarget(TargetingType type, Transform origin)
    {
        var monsterManager = ServiceLocator.Get<MonsterManager>();
        if (monsterManager == null) return null;

        var activeMonsters = monsterManager.ActiveMonsters;
        if (activeMonsters.Count == 0) return null;

        switch (type)
        {
            case TargetingType.Nearest:
                return activeMonsters.OrderBy(m => Vector3.Distance(origin.position, m.transform.position)).FirstOrDefault()?.transform;

            case TargetingType.HighestHealth:
                // [] Yǥ    currentHealth  ü    ã 
                return activeMonsters.OrderByDescending(m => m.currentHealth).FirstOrDefault()?.transform;

            case TargetingType.LowestHealth:
                // [] Yǥ    currentHealth  ü    ã 
                return activeMonsters.OrderBy(m => m.currentHealth).FirstOrDefault()?.transform;

            case TargetingType.Random:
                return activeMonsters[Random.Range(0, activeMonsters.Count)].transform; // Changed .Length to .Count

            case TargetingType.Forward:
            default:
                return null;
        }
    }
}