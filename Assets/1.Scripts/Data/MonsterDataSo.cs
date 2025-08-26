// ϸ: MonsterDataSO.cs
// : Assets/1.Scripts/Data/MonsterDataSO.cs
using UnityEngine;

/// <summary>
///     ͸ ϴ ScriptableObjectԴϴ.
/// ü, ӵ, ݷ,     Ӽ   ϳ   ֽϴ.
/// </summary>
[CreateAssetMenu(fileName = "MonsterData_", menuName = "GameData/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    [Header("⺻ ")]
    [Tooltip("͸ ã   IDԴϴ. (: slime_normal, goblin_archer)")]
    public string monsterID;

    [Tooltip("  ǥõ ̸Դϴ.")]
    public string monsterName;

    [Header("ɷġ")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;

    [Header("")]
    [Tooltip(" Ͱ    ϼ.")]
    public GameObject prefab; // [] string ٽ GameObject 

    // [Ȯ ] ȹ ޵ پ  ൿ   
    // public enum MonsterBehaviorType { Chase, Flee, Patrol, ExplodeOnDeath }
    // public MonsterBehaviorType behaviorType;
}