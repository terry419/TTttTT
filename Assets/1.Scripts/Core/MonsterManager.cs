// 파일명: MonsterManager.cs
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    private readonly List<MonsterController> activeMonsters = new List<MonsterController>();
    public IReadOnlyList<MonsterController> ActiveMonsters => activeMonsters;

    void Awake()
    {
        ServiceLocator.Register<MonsterManager>(this);
        // 이 매니저는 _GameplaySession 프리팹에 넣어두면 DontDestroyOnLoad가 필요 없습니다.
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