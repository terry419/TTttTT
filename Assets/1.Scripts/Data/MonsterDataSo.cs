/* 경로 유지: TTttTT/Assets/1/Scripts/Data/MonsterDataSo.cs */
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum MonsterBehaviorType
{
    Chase, // 추격
    Patrol, // 순찰
    Flee   // 도망 (향후 구현)
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

    public MonsterBehaviorType behaviorType = MonsterBehaviorType.Chase;

    // Patrol 타입 전용 파라미터들
    [Header("AI 행동 파라미터")]
    [Tooltip("Patrol 타입: 플레이어를 감지하고 추격을 시작하는 반경입니다.")]
    public float playerDetectionRadius = 8f;

    [Tooltip("Patrol 타입: 추격 중인 플레이어를 놓치는 반경입니다.")]
    public float loseSightRadius = 15f;

    [Tooltip("Patrol 타입: 순찰 시 스폰 지점에서 얼마나 멀리까지 갈지 결정하는 반경입니다.")]
    public float patrolRadius = 5f;

    [Tooltip("Patrol 타입: 순찰 시 이동 속도에 곱해지는 배율입니다. (1 = 100%)")]
    [Range(0.1f, 1f)]
    public float patrolSpeedMultiplier = 0.5f;

    [Header("도망 파라미터 (Flee 타입 전용)")]
    [Tooltip("플레이어가 이 반경 안으로 들어오면 도망치기 시작합니다.")]
    public float fleeTriggerRadius = 6f;

    [Tooltip("플레이어가 이 반경 밖으로 나가면 도망을 멈추고 순찰을 시작합니다.")]
    public float fleeSafeRadius = 12f;

    [Tooltip("도망칠 때의 이동 속도 배율입니다. (1.2 = 120%)")]
    [Range(1f, 2f)]
    public float fleeSpeedMultiplier = 1.2f;


    [Header("특수 능력: 자폭 (플레이어 대상)")]
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
}