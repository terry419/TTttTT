// 파일명: MonsterDataSO.cs
// 경로: Assets/1.Scripts/Data/MonsterDataSO.cs
using UnityEngine;

/// <summary>
/// 몬스터 한 종류의 모든 데이터를 정의하는 ScriptableObject입니다.
/// 체력, 속도, 공격력, 프리팹 등 몬스터의 모든 속성을 이 파일 하나로 관리할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "MonsterData_", menuName = "GameData/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("데이터를 찾기 위한 고유 ID입니다. (예: slime_normal, goblin_archer)")]
    public string monsterID;

    [Tooltip("게임 내에 표시될 이름입니다.")]
    public string monsterName;

    [Header("능력치")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;

    [Header("참조")]
    [Tooltip("이 몬스터가 사용할 프리팹을 직접 연결하세요.")]
    public GameObject prefab; // [수정] string에서 다시 GameObject로 변경

    // [확장 예정] 기획서에 언급된 다양한 몬스터 행동 패턴을 위한 데이터
    // public enum MonsterBehaviorType { Chase, Flee, Patrol, ExplodeOnDeath }
    // public MonsterBehaviorType behaviorType;
}