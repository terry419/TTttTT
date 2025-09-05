// 경로: ./TTttTT/Assets/1.Scripts/Data/MonsterDataSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;

// 이 파일 상단의 enum 정의들은 기존과 동일하게 유지합니다.
public enum MonsterBehaviorType { Chase, Patrol }
public enum FleeCondition { PlayerProximity, LowHealth, Outnumbered }

[CreateAssetMenu(fileName = "MonsterData_", menuName = "GameData/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    [Header("--- [1] 기본 정보 ---")]
    public string monsterID;
    public string monsterName;
    public AssetReferenceGameObject prefabRef; // 몬스터 프리팹

    [Header("--- [2] 기본 능력치 ---")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;

    // ======================================================================
    [Header("--- [3] AI 시스템 선택 ---")]
    [Tooltip("체크 시, 이 몬스터는 신규 모듈형 AI 시스템을 사용합니다. 체크 해제 시 기존 AI로 동작합니다.")]
    public bool useNewAI;

    [Header("--- [4] 신규 모듈형 AI 설정 ---")]
    [Tooltip("이 몬스터가 처음 시작할 행동 부품(Behavior) 에셋을 여기에 연결합니다. (useNewAI가 true일 때만 작동)")]
    public MonsterBehavior initialBehavior;

    // 이 아래로는 기존 AI와 공통으로 사용하는 특수 능력(자폭 등) 설정입니다.
    // ======================================================================

    [Header("--- [5] 특수 능력: 자폭 (Explode) ---")]
    [Tooltip("체크 시, 이 몬스터는 사망 시 플레이어를 대상으로 자폭을 시도합니다.")]
    public bool canExplodeOnDeath;
    public AssetReferenceGameObject explosionVfxRef;
    public float explosionDelay = 0.5f;
    public float explosionDamage = 20f;
    public float explosionRadius = 3f;

    // ======================================================================
    // 기존 FSM AI를 위한 설정값들은 남겨두어, 언제든 useNewAI를 끄고 예전 방식으로 되돌릴 수 있도록 합니다.
    [Header("--- [6] 구버전(FSM) AI 설정 (useNewAI가 false일 때만 작동) ---")]
    [Tooltip("이 몬스터의 기본 행동 패턴입니다.")]
    public MonsterBehaviorType behaviorType = MonsterBehaviorType.Chase;
    [Tooltip("플레이어를 감지하고 추격을 시작하는 반경입니다.")]
    public float playerDetectionRadius = 8f;
    [Tooltip("추격 중인 플레이어를 놓치는 반경입니다.")]
    public float loseSightRadius = 15f;
    [Tooltip("순찰 시 스폰 지점에서 얼마나 멀리까지 갈지 결정하는 반경입니다.")]
    public float patrolRadius = 5f;
    [Range(0.1f, 1f)]
    public float patrolSpeedMultiplier = 0.5f;
    [Header("구버전(FSM) 특수 능력: 도망 (Flee)")]
    public bool canFlee;
    public FleeCondition fleeCondition = FleeCondition.PlayerProximity;
    public float fleeTriggerRadius = 6f;
    [Range(0.1f, 1f)] public float fleeOnHealthPercentage = 0.3f;
    public float allyCheckRadius = 10f;
    public int fleeWhenAlliesLessThan = 2;
    public float fleeSafeRadius = 12f;
    [Range(0, 2f)] public float fleeSpeedMultiplier = 1.2f;
    // ======================================================================
}