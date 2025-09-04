// 경로: ./TTttTT/Assets/1.Scripts/Gameplay/MonsterController.cs

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;

/// <summary>
/// 이 스크립트는 몬스터 게임 오브젝트에 부착되는 '뇌'와 같은 역할을 합니다.
/// 몬스터의 움직임, 피격, 죽음 등 모든 핵심 기능을 총괄합니다.
/// 1단계 수정으로, 이 '뇌'는 기존 AI 방식과 새로운 AI 방식(부품 조립식) 두 가지를 모두 처리할 수 있게 됩니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(MonsterStats))]
public class MonsterController : MonoBehaviour
{
    // ========================================================================
    // A. 기존 AI(FSM) 시스템을 위한 변수들
    // ========================================================================
    // 이 부분은 기존에 사용하던 상태 머신(Finite State Machine) 관련 변수들입니다.
    // 새로운 AI 시스템이 완전히 자리 잡기 전까지는 삭제하지 않고 그대로 유지하여 안정성을 확보합니다.
    private enum State { Spawning, Chasing, Patrolling, Fleeing }
    private State currentState = State.Spawning;
    private State baseState; // 도망치기 전의 원래 상태를 저장할 변수
    private Vector3 patrolTargetPosition;
    private float playerDetectionRadius;
    private float loseSightRadius;
    private float patrolRadius;
    private float patrolSpeedMultiplier;
    private float fleeTriggerRadius;
    private float fleeSafeRadius;
    private float fleeSpeedMultiplier;
    private FleeCondition fleeCondition;
    private float fleeOnHealthPercentage;
    private float allyCheckRadius;
    private int fleeWhenAlliesLessThan;

    // AI가 다음 행동을 결정하기까지의 시간 간격입니다. (0.2초)
    private const float STATE_CHECK_INTERVAL = 0.2f;
    private float stateCheckTimer; // 다음 체크까지의 시간을 재는 타이머

    // ========================================================================
    // B. 신규 모듈형 AI 시스템을 위한 변수들
    // ========================================================================
    // 이 부분은 1단계에서 새로 추가되는 변수들입니다.
    // '행동 부품(Behavior)'을 담아두고 실행하는 역할을 합니다.
    private MonsterBehavior currentBehavior; // 현재 실행 중인 행동 부품(SO)
    private float behaviorCheckTimer = 0f;   // 행동 부품의 실행 주기를 조절하여 성능을 최적화하기 위한 타이머

    // ========================================================================
    // C. 모든 AI가 공통으로 사용하는 변수 및 참조
    // ========================================================================
    // 이 변수들은 몬스터의 '몸'이나 '감각'에 해당합니다.
    // 기존 AI든 새로운 AI든 상관없이 이 변수들을 공통으로 사용하여 몬스터를 제어합니다.

    private MonsterDataSO monsterData; // 몬스터의 모든 정보가 담긴 데이터 파일

    // [HideInInspector]는 Unity 에디터의 Inspector 창에 변수가 보이지 않게 숨겨주는 기능입니다.
    // 코드를 통해서만 접근하도록 하여 실수를 방지합니다.
    // [1단계 수정] 외부의 'Behavior' 부품들이 몬스터의 몸(Rigidbody)이나 능력치(Stats)에 접근할 수 있도록
    // private에서 public으로 접근 권한을 변경합니다.
    [HideInInspector] public MonsterStats monsterStats;  // 몬스터의 체력, 공격력 등 능력치 정보
    [HideInInspector] public Rigidbody2D rb;          // 몬스터의 물리적 움직임을 담당하는 부품
    [HideInInspector] public Transform playerTransform; // 플레이어의 위치 정보
    [HideInInspector] public Vector3 startPosition;     // 몬스터가 처음 생성된 위치

    // --- 기타 상태 변수 ---
    private bool isInvulnerable = false; // 무적 상태 여부
    private bool isDead = false;         // 죽었는지 여부
    private const float DAMAGE_INTERVAL = 0.1f;
    private float damageTimer = 0f;
    private bool isTouchingPlayer = false; // 플레이어와 닿아있는지 여부
    private bool isVelocityOverridden = false; // 외부 요인(넉백 등)에 의해 속도가 강제로 변경되었는지 여부
    private HashSet<string> _hitShotIDsThisFrame = new HashSet<string>();
    private int _lastHitFrame;

    // --- 이벤트 ---
    // 이벤트는 몬스터에게 무슨 일이 생겼을 때 다른 시스템(UI, 사운드 등)에 알려주는 '방송'과 같습니다.
    public static event Action<float, Vector3> OnMonsterDamaged;
    public static event Action<MonsterController> OnMonsterDied;

    // ========================================================================
    // D. Unity 생명주기 함수들 (Awake, OnEnable, Update 등)
    // ========================================================================

    void Awake()
    {
        // 이 몬스터가 가진 부품들을 미리 찾아 변수에 저장해둡니다.
        // 'GetComponent'는 자기 자신에게 부착된 부품을 찾는 명령입니다.
        // 이 작업은 게임 시작 시 한 번만 이루어지므로 효율적입니다.
        rb = GetComponent<Rigidbody2D>();
        monsterStats = GetComponent<MonsterStats>();
    }

    void OnEnable()
    {
        // 몬스터가 활성화될 때마다(예: 재사용될 때) 초기 상태로 리셋합니다.
        ServiceLocator.Get<MonsterManager>()?.RegisterMonster(this);
        isDead = false;
        isInvulnerable = false;
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
    }

    void OnDisable()
    {
        // 몬스터가 비활성화될 때, 관리자에게 알려 목록에서 제외시킵니다.
        if (ServiceLocator.IsRegistered<MonsterManager>())
        {
            ServiceLocator.Get<MonsterManager>()?.UnregisterMonster(this);
        }
    }

    public void Initialize(MonsterDataSO data)
    {
        // 몬스터가 처음 스폰될 때 호출되는 초기화 함수입니다.
        // 몬스터 데이터(SO)를 받아와서 필요한 모든 설정을 세팅합니다.
        if (data == null)
        {
            Debug.LogError($"[AI Error] 몬스터 '{gameObject.name}'에 MonsterDataSO가 할당되지 않았습니다! 스폰 로직을 확인하세요.");
            gameObject.SetActive(false);
            return;
        }

        monsterData = data;
        monsterStats.Initialize(data);
        startPosition = transform.position;
        isDead = false;

        // ================================================================
        // ★ 1단계 핵심 수정 ★: 여기서 어떤 AI를 사용할지 결정합니다.
        // ================================================================
        if (monsterData.useNewAI && monsterData.initialBehavior != null)
        {
            // 몬스터 데이터 파일에 'useNewAI'가 체크되어 있으면, 새로운 AI 시스템을 시작합니다.
            Debug.Log($"[AI Log | Time: {Time.time:F2}] Monster: '{gameObject.name}', Event: Initialization, AI System: 'New Modular AI'");
            currentState = State.Spawning; // 기존 상태는 더미 값으로 둡니다.
            ChangeBehavior(monsterData.initialBehavior);
        }
        else
        {
            // 'useNewAI'가 체크되어 있지 않으면, 기존의 AI 시스템을 그대로 사용합니다.
            Debug.Log($"[AI Log | Time: {Time.time:F2}] Monster: '{gameObject.name}', Event: Initialization, AI System: 'Legacy FSM'");
            this.playerDetectionRadius = data.playerDetectionRadius;
            this.loseSightRadius = data.loseSightRadius;
            this.patrolRadius = data.patrolRadius;
            this.patrolSpeedMultiplier = data.patrolSpeedMultiplier;
            this.fleeTriggerRadius = data.fleeTriggerRadius;
            this.fleeSafeRadius = data.fleeSafeRadius;
            this.fleeSpeedMultiplier = data.fleeSpeedMultiplier;
            this.fleeCondition = data.fleeCondition;
            this.fleeOnHealthPercentage = data.fleeOnHealthPercentage;
            this.allyCheckRadius = data.allyCheckRadius;
            this.fleeWhenAlliesLessThan = data.fleeWhenAlliesLessThan;

            switch (monsterData.behaviorType)
            {
                case MonsterBehaviorType.Patrol:
                    baseState = State.Patrolling;
                    currentState = State.Patrolling;
                    UpdatePatrolTarget();
                    break;
                default: // Chase
                    baseState = State.Chasing;
                    currentState = State.Chasing;
                    break;
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        // ================================================================
        // ★ 1단계 핵심 수정 ★: 현재 어떤 AI가 실행 중인지에 따라 로직을 분기합니다.
        // ================================================================
        if (currentBehavior != null) // 'currentBehavior'가 존재한다면, 새로운 AI가 실행 중이라는 의미입니다.
        {
            // [개선안 4. 메모리 줄이기] 매 프레임이 아닌 0.2초마다 한 번씩만 행동을 결정하도록 하여 부하를 줄입니다.
            behaviorCheckTimer += Time.deltaTime;
            if (behaviorCheckTimer >= 0.2f)
            {
                currentBehavior.OnExecute(this); // 행동 부품에게 "이제 네가 할 일을 해"라고 명령합니다.
                behaviorCheckTimer = 0f;
            }
        }
        else // 'currentBehavior'가 없다면, 기존 AI가 실행 중이라는 의미입니다.
        {
            // 이 블록 안의 코드는 기존 MonsterController의 Update와 완전히 동일합니다.
            stateCheckTimer += Time.deltaTime;
            if (stateCheckTimer >= STATE_CHECK_INTERVAL)
            {
                UpdateStateTransitions();
                stateCheckTimer = 0f;
            }

            if (isTouchingPlayer)
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= DAMAGE_INTERVAL)
                {
                    ApplyContactDamage();
                    damageTimer = 0f;
                }
            }
        }
    }

    void FixedUpdate()
    {
        // FixedUpdate는 물리 계산을 위한 업데이트 함수입니다.
        // 새로운 AI는 Behavior 부품이 직접 물리(rb.velocity)를 제어하므로,
        // 새로운 AI가 실행 중일 때는 이 함수가 아무것도 하지 않도록 막습니다.
        if (currentBehavior != null) return;

        // 아래는 모두 기존 AI 시스템을 위한 물리 이동 로직입니다.
        if (isVelocityOverridden)
        {
            isVelocityOverridden = false;
            return;
        }

        if (playerTransform == null)
        {
            var playerController = ServiceLocator.Get<PlayerController>();
            if (playerController != null) playerTransform = playerController.transform;
            else { rb.velocity = Vector2.zero; return; }
        }
        if (isInvulnerable || isDead) { rb.velocity = Vector2.zero; return; }

        switch (currentState)
        {
            case State.Patrolling:
                ExecutePatrolMovement();
                break;
            case State.Chasing:
                ExecuteChaseMovement();
                break;
            case State.Fleeing:
                ExecuteFleeMovement();
                break;
            default:
                rb.velocity = Vector2.zero;
                break;
        }
    }

    // ========================================================================
    // E. 신규 모듈형 AI를 위한 핵심 함수
    // ========================================================================

    /// <summary>
    /// 현재 몬스터의 행동 부품(Behavior)을 교체합니다.
    /// 이것이 새로운 AI 시스템의 '상태 전환' 핵심 로직입니다.
    /// </summary>
    /// <param name="newBehavior">새롭게 실행할 행동 부품</param>
    public void ChangeBehavior(MonsterBehavior newBehavior)
    {
        if (currentBehavior == newBehavior) return;

        // [개선안 3. 보기 편한 로그] 상태 전환 시 상세한 로그를 남깁니다.
        string oldBehaviorName = (currentBehavior != null) ? currentBehavior.name : "None";
        string newBehaviorName = (newBehavior != null) ? newBehavior.name : "None";
        Debug.Log($"[AI Log | Time: {Time.time:F2}] Monster: '{gameObject.name}', Event: Behavior Changed, From: '{oldBehaviorName}', To: '{newBehaviorName}'");

        // 1. 기존 행동의 마무리(OnExit) 함수를 호출합니다.
        if (currentBehavior != null)
        {
            currentBehavior.OnExit(this);
        }

        // 2. 현재 행동 부품을 새로운 것으로 교체합니다.
        currentBehavior = newBehavior;

        // 3. 새로운 행동의 초기화(OnEnter) 함수를 호출합니다.
        if (currentBehavior != null)
        {
            currentBehavior.OnEnter(this);
        }
    }

    // ========================================================================
    // F. 기존 AI(FSM)를 위한 함수들
    // ========================================================================
    // 이 함수들은 기존 AI 시스템에서만 사용됩니다. 수정 없이 그대로 둡니다.

    private void UpdateStateTransitions()
    {
        if (playerTransform == null) return;

        if (monsterData.canFlee)
        {
            bool shouldFlee = CheckFleeCondition();

            if (currentState != State.Fleeing && shouldFlee)
            {
                ChangeState(State.Fleeing);
                return;
            }
            else if (currentState == State.Fleeing && !shouldFlee)
            {
                ChangeState(baseState);
                return;
            }
        }

        if (currentState == State.Patrolling || currentState == State.Chasing)
        {
            float sqrDistanceToPlayer = (playerTransform.position - transform.position).sqrMagnitude;
            if (currentState == State.Patrolling && sqrDistanceToPlayer < playerDetectionRadius * playerDetectionRadius)
            {
                ChangeState(State.Chasing);
            }
            else if (currentState == State.Chasing && monsterData.behaviorType == MonsterBehaviorType.Patrol && sqrDistanceToPlayer > loseSightRadius * loseSightRadius)
            {
                ChangeState(State.Patrolling);
            }
        }
    }

    private bool CheckFleeCondition()
    {
        switch (fleeCondition)
        {
            case FleeCondition.PlayerProximity:
                return (playerTransform.position - transform.position).sqrMagnitude < fleeTriggerRadius * fleeTriggerRadius;

            case FleeCondition.LowHealth:
                return monsterStats.CurrentHealth / monsterStats.FinalMaxHealth <= fleeOnHealthPercentage;

            case FleeCondition.Outnumbered:
                Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, allyCheckRadius, LayerMask.GetMask("Monster"));
                return (allies.Length - 1) < fleeWhenAlliesLessThan;
        }
        return false;
    }

    private void ExecutePatrolMovement()
    {
        if ((patrolTargetPosition - transform.position).sqrMagnitude < 0.25f)
        {
            UpdatePatrolTarget();
        }
        Vector2 direction = (patrolTargetPosition - transform.position).normalized;
        rb.velocity = direction * monsterStats.FinalMoveSpeed * patrolSpeedMultiplier;
    }

    private void ExecuteChaseMovement()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = direction * monsterStats.FinalMoveSpeed;
    }

    private void ExecuteFleeMovement()
    {
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        rb.velocity = direction * monsterStats.FinalMoveSpeed * fleeSpeedMultiplier;
    }

    private void ChangeState(State newState)
    {
        if (currentState == newState) return;
        Debug.Log($"<color=orange>[FSM-Transition]</color> '{monsterData.monsterName}' 상태 변경: {currentState} -> {newState}");
        currentState = newState;
    }

    private void UpdatePatrolTarget()
    {
        Vector2 randomOffset = Random.insideUnitCircle * patrolRadius;
        patrolTargetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
    }

    // ========================================================================
    // G. 공통 기능 함수들 (피격, 죽음, 충돌 등)
    // ========================================================================
    // 이 아래의 함수들은 몬스터의 기본 기능으로, 어떤 AI 시스템을 사용하든 공통으로 호출됩니다.

    public void SetVelocity(Vector2 newVelocity)
    {
        if (isDead) return;
        rb.velocity = newVelocity;
        isVelocityOverridden = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForPlayer(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CheckForPlayer(other.gameObject);
    }

    private void CheckForPlayer(GameObject target)
    {
        if (target.CompareTag(Tags.Player))
        {
            isTouchingPlayer = true;
            damageTimer = DAMAGE_INTERVAL;
        }
    }

    private void LeavePlayer(GameObject target)
    {
        if (target.CompareTag(Tags.Player))
        {
            isTouchingPlayer = false;
            damageTimer = 0f;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        LeavePlayer(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        LeavePlayer(other.gameObject);
    }

    private void ApplyContactDamage()
    {
        if (playerTransform != null && playerTransform.TryGetComponent<CharacterStats>(out var playerStats))
        {
            playerStats.TakeDamage(monsterStats.FinalContactDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        monsterStats.TakeDamage(damage);
        if (monsterStats.IsDead())
        {
            Die().Forget();
        }
    }

    public void NotifyDamageTaken(float finalDamage)
    {
        OnMonsterDamaged?.Invoke(finalDamage, transform.position);
    }

    private async UniTaskVoid Die()
    {
        if (isDead) return;
        isDead = true;
        OnMonsterDied?.Invoke(this);

        if (monsterData.canExplodeOnDeath && monsterData.explosionVfxRef != null && monsterData.explosionVfxRef.RuntimeKeyIsValid())
        {
            await PlayerTargetedExplosionAsync(transform.position, monsterData);
            if (this == null) return;
        }

        ServiceLocator.Get<PoolManager>().Release(gameObject);
    }

    private static async UniTask PlayerTargetedExplosionAsync(Vector3 deathPosition, MonsterDataSO monsterData)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(monsterData.explosionDelay));

        var poolManager = ServiceLocator.Get<PoolManager>();
        var playerController = ServiceLocator.Get<PlayerController>();

        if (poolManager == null || playerController == null) return;

        GameObject vfxGO = await poolManager.GetAsync(monsterData.explosionVfxRef.AssetGUID);

        if (vfxGO != null)
        {
            vfxGO.transform.position = deathPosition;
        }

        float sqrDistanceToPlayer = (deathPosition - playerController.transform.position).sqrMagnitude;
        float explosionSqrRadius = monsterData.explosionRadius * monsterData.explosionRadius;

        if (sqrDistanceToPlayer <= explosionSqrRadius)
        {
            if (playerController.TryGetComponent<CharacterStats>(out var playerStats))
            {
                playerStats.TakeDamage(monsterData.explosionDamage);
            }
        }
    }

    public float GetCurrentHealth()
    {
        return monsterStats != null ? monsterStats.CurrentHealth : 0;
    }

    public void SetInvulnerable(float duration)
    {
        StartCoroutine(InvulnerableRoutine(duration));
    }

    private IEnumerator InvulnerableRoutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public bool RegisterHitByShot(string shotID, bool allowMultipleHits)
    {
        if (string.IsNullOrEmpty(shotID))
        {
            return true;
        }
        if (allowMultipleHits)
        {
            return true;
        }
        if (_lastHitFrame != Time.frameCount)
        {
            _hitShotIDsThisFrame.Clear();
            _lastHitFrame = Time.frameCount;
        }
        if (_hitShotIDsThisFrame.Contains(shotID))
        {
            return false;
        }
        _hitShotIDsThisFrame.Add(shotID);
        return true;
    }
}
