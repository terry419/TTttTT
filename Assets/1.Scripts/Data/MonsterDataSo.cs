// 파일명: MonsterDataSO.cs
// 경로: Assets/1.Scripts/Data/MonsterDataSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets; // 상단에 추가

/// <summary>
/// 몬스터의 데이터를 정의하는 ScriptableObject입니다.
/// 체력, 이동 속도, 공격력 등 다양한 속성을 하나로 묶어 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "MonsterData_", menuName = "GameData/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("몬스터를 식별하는 고유 ID입니다. (예: slime_normal, goblin_archer)")]
    public string monsterID;

    [Tooltip("UI에 표시될 이름입니다.")]
    public string monsterName;

    [Header("능력치")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;

    [Header("프리팹")]
    [Tooltip("이 몬스터가 사용할 프리팹입니다.")]
    // public GameObject prefab; // 이 줄을 주석 처리하거나 삭제
    public AssetReferenceGameObject prefabRef; // 이 줄로 교체

    // [확장] 향후 행동 패턴 등을 추가할 수 있습니다.
    // public enum MonsterBehaviorType { Chase, Flee, Patrol, ExplodeOnDeath }
    // public MonsterBehaviorType behaviorType;
}
