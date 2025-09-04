/* 경로 유지: TTttTT/Assets/1/Scripts/Data/MonsterDataSo.cs */
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum MonsterBehaviorType
{
    Chase, // 추격
    Patrol // 순찰
}

public enum FleeCondition
{
    PlayerProximity,  // 기본: 플레이어 근접 시
    LowHealth,        // 체력이 일정 비율 이하일 때
    Outnumbered       // 주변 아군 수가 일정 이하일 때
}

[CreateAssetMenu(fileName = "MonsterData_", menuName = "GameData/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    // --- 기본 정보, 능력치 (변경 없음) ---
    public string monsterID;
    public string monsterName;

    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;

    [Header("행동 패턴")]
    [Tooltip("이 몬스터의 기본 행동 패턴입니다. 도망 조건이 아닐 때 이 패턴으로 행동합니다.")]
    public MonsterBehaviorType behaviorType = MonsterBehaviorType.Chase;
    
    [Header("AI 행동 파라미터 (Patrol 타입 전용)")]
    [Tooltip("Patrol 타입: 플레이어를 감지하고 추격을 시작하는 반경입니다.")]
    public float playerDetectionRadius = 8f;

    [Tooltip("Patrol 타입: 추격 중인 플레이어를 놓치는 반경입니다.")]
    public float loseSightRadius = 15f;

    [Tooltip("Patrol 타입: 순찰 시 스폰 지점에서 얼마나 멀리까지 갈지 결정하는 반경입니다.")]
    public float patrolRadius = 5f;

    [Tooltip("Patrol 타입: 순찰 시 이동 속도에 곱해지는 배율입니다. (1 = 100%)")]
    [Range(0.1f, 1f)]
    public float patrolSpeedMultiplier = 0.5f;

    // ▼▼▼ [수정] Flee를 '특수 능력' 섹션으로 옮겨 모듈화합니다. ▼▼▼
    [Header("특수 능력: 도망 (Flee)")]
    [Tooltip("체크 시, 이 몬스터는 특정 조건 하에 도망치는 능력을 가집니다.")]
    public bool canFlee;
    
    [Tooltip("몬스터가 도망을 시작하는 조건을 선택합니다.")]
    public FleeCondition fleeCondition = FleeCondition.PlayerProximity;

    // --- 이하 Flee 관련 모든 파라미터는 그대로 유지 ---
    [Tooltip("조건(PlayerProximity): 플레이어가 이 반경 안으로 들어오면 도망칩니다.")]
    public float fleeTriggerRadius = 6f;
    
    [Tooltip("조건(LowHealth): 최대 체력 대비 현재 체력이 이 비율(%) 이하일 때 도망칩니다.")]
    [Range(0.1f, 1f)] public float fleeOnHealthPercentage = 0.3f;

    [Tooltip("조건(Outnumbered): 이 반경 내 아군 몬스터 수가 설정 값 미만일 때 도망칩니다.")]
    public float allyCheckRadius = 10f;
    [Tooltip("조건(Outnumbered): 주변 아군 수가 이 값 미만일 때 도망칩니다.")]
    public int fleeWhenAlliesLessThan = 2;
    
    // --- 공통 파라미터 ---
    [Tooltip("플레이어가 이 반경 밖으로 나가면 도망을 멈추고 순찰을 시작합니다.")]
    public float fleeSafeRadius = 12f;
    [Tooltip("도망칠 때의 이동 속도 배율입니다. (1.2 = 120%)")]
    [Range(0, 2f)] public float fleeSpeedMultiplier = 1.2f;


    [Header("특수 능력: 자폭 (Explode)")]
    [Tooltip("체크 시, 이 몬스터는 사망 시 플레이어를 대상으로 자폭을 시도합니다.")]
    public bool canExplodeOnDeath;

    [Tooltip("자폭 시 시각 효과(VFX)로만 사용될 프리팹입니다. 데미지 로직이 없어야 합니다.")]
    public AssetReferenceGameObject explosionVfxRef; // explosionPrefabRef -> explosionVfxRef로 이름 변경

    [Tooltip("사망 후 폭발이 발생하기까지의 지연 시간(초)입니다.")]
    public float explosionDelay = 0.5f;

    [Tooltip("자폭 피해량")]
    public float explosionDamage = 20f;

    [Tooltip("자폭 피해 반경")]
    public float explosionRadius = 3f;

    public AssetReferenceGameObject prefabRef;

    [Header("--- [신규] 모듈형 AI 설정 ---")]
    [Tooltip("체크 시, 이 몬스터는 기존 AI 대신 새로운 모듈형 AI 시스템을 사용합니다.")]
    public bool useNewAI;

    [Tooltip("이 몬스터가 처음 시작할 행동 부품(Behavior) 에셋을 여기에 연결합니다.")]
    public MonsterBehavior initialBehavior;
}