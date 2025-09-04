// ϸ: MonsterManager.cs
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    private readonly List<MonsterController> activeMonsters = new List<MonsterController>();
    public IReadOnlyList<MonsterController> ActiveMonsters => activeMonsters;

    void Awake()
    {
        ServiceLocator.Register<MonsterManager>(this);
        // 이 매니저는 _GameplaySession 객체의 자식으로 존재하므로, DontDestroyOnLoad를 직접 호출할 필요가 없습니다.
    }

    public void RegisterMonster(MonsterController monster)
    {
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);
        }
    }

    public void UnregisterMonster(MonsterController monster)
    {
        if (activeMonsters.Contains(monster))
        {
            activeMonsters.Remove(monster);
        }
    }
}