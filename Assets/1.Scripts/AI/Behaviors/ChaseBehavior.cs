// : ./TTttTT/Assets/1.Scripts/AI/Behaviors/ChaseBehavior.cs
using UnityEngine;

/// <summary>
/// [ൿ ǰ]    ӵ ̵ϴ ൿԴϴ.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Chase")]
public class ChaseBehavior : MonsterBehavior
{
    [Tooltip("߰  ⺻ ̵ ӵ  Դϴ. 1.0 100% ӵ ǹմϴ.")]
    public float speedMultiplier = 1.0f;

    public override void OnEnter(MonsterController monster)
    {
        base.OnEnter(monster);
        // Ư ʱȭ ۾ ϴ.
    }

    public override void OnExecute(MonsterController monster)
    {
        // If there is no player, do not move.
        if (monster.targetTransform == null)
        {
            monster.rb.velocity = Vector2.zero;
            return;
        }

        // 1. Calculate the vector towards the player.
        Vector2 direction = (monster.targetTransform.position - monster.transform.position).normalized;

        // 2. Calculate speed based on the monster's final move speed and multiplier, and move towards the target.
        monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * speedMultiplier;

        // 3. Check if there are any other behaviors to transition to. (e.g., if player is in attack range)
        CheckTransitions(monster);
    }

    public override void OnExit(MonsterController monster)
    {
        // ߰ ൿ  , Ȥ   ֱ  ӵ 0 ʱȭմϴ.
        monster.rb.velocity = Vector2.zero;
    }
}