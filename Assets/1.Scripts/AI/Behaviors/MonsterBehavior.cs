// 생성 경로: Assets/1.Scripts/AI/Behaviors/MonsterBehavior.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class MonsterBehavior : ScriptableObject
{
    public List<Transition> transitions = new List<Transition>();

    public virtual void OnEnter(MonsterController monster) { }
    public abstract void OnExecute(MonsterController monster);
    public virtual void OnExit(MonsterController monster) { }

    protected void CheckTransitions(MonsterController monster)
    {
        if (transitions == null || transitions.Count == 0) return;
        foreach (var transition in transitions)
        {
            if (transition.decision != null && transition.decision.Decide(monster))
            {
                if (transition.nextBehavior != null)
                {
                    monster.ChangeBehavior(transition.nextBehavior);
                    return;
                }
            }
        }
    }
}