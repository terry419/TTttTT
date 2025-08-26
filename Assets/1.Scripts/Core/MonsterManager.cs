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
        //  Ŵ _GameplaySession տ ־θ DontDestroyOnLoad ʿ ϴ.
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