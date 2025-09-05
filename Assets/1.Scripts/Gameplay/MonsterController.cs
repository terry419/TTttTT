// 경로: ./TTttTT/Assets/1.Scripts/Gameplay/MonsterController.cs

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;

/// <summary>
/// [AI 엔진 최종본]
/// 몬스터 게임 오브젝트에 부착되는 '뇌'이자 '엔진'입니다.
/// 이 스크립트는 어떤 '행동 부품(Behavior)'을 사용할지만 결정하고, 실제 행동은 부품에게 위임합니다.
/// 기존 FSM(상태 머신) AI와 신규 모듈형 AI를 모두 지원합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(MonsterStats))]
public class MonsterController : MonoBehaviour
{
    // ========================================================================
    // A. 신규 모듈형 AI 시스템을 위한 변수들
    // ========================================================================
    private MonsterBehavior currentBehavior; // 현재 실행 중인 행동 부품(SO)
    private float behaviorCheckTimer = 0f;   // 행동 부품의 실행 주기를 조절하기 위한 타이머

    // [1단계 추가] 행동 부품들이 접근할 수 있도록 공개된 상태 변수들
    [HideInInspector] public MonsterDataSO monsterData;      // 이 몬스터의 모든 정보가 담긴 데이터 파일
    [HideInInspector] public MonsterStats monsterStats;     // 몬스터의 체력, 공격력 등 능력치 정보
    [HideInInspector] public Rigidbody2D rb;               // 몬스터의 물리적 움직임을 담당하는 부품
    [HideInInspector] public Transform playerTransform;    // 플레이어의 위치 정보
    [HideInInspector] public Vector3 startPosition;        // 몬스터가 처음 생성된 위치
    [HideInInspector] public float stateTimer = 0f;        // 현재 행동(state)으로 전환된 후 경과 시간

    // ========================================================================
    // B. 기존 FSM AI 시스템을 위한 변수들 (호환성을 위해 유지)
    // ========================================================================
    private enum State { Spawning, Chasing, Patrolling, Fleeing }
    private State currentState = State.Spawning;
    // (기존 FSM 관련 변수들은 생략... 원본 코드와 동일하게 유지됩니다)
    #region Legacy FSM Variables
    private State baseState;
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
    private const float STATE_CHECK_INTERVAL = 0.2f;
    private float stateCheckTimer;
    #endregion

    // ========================================================================
    // C. 모든 AI가 공통으로 사용하는 변수 및 참조
    // ========================================================================
    private bool isInvulnerable = false;
    private bool isDead = false;
    private const float DAMAGE_INTERVAL = 0.1f;
    private float damageTimer = 0f;
    private bool isTouchingPlayer = false;
    private bool isVelocityOverridden = false;
    private HashSet<string> _hitShotIDsThisFrame = new HashSet<string>();
    private int _lastHitFrame;
    public static event Action<float, Vector3> OnMonsterDamaged;
    public static event Action<MonsterController> OnMonsterDied;

    // ========================================================================
    // D. Unity 생명주기 함수들 (Awake, OnEnable, Update 등)
    // ========================================================================
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        monsterStats = GetComponent<MonsterStats>();
    }

    void OnEnable()
    {
        ServiceLocator.Get<MonsterManager>()?.RegisterMonster(this);
        isDead = false;
        isInvulnerable = false;
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
    }

    public void Initialize(MonsterDataSO data)
    {
        if (data == null)
        {
            Debug.LogError($"[AI Error] 몬스터 '{gameObject.name}'에 MonsterDataSO가 할당되지 않았습니다!", this.gameObject);
            gameObject.SetActive(false);
            return;
        }

        this.monsterData = data;
        monsterStats.Initialize(data);
        startPosition = transform.position;
        isDead = false;

        // ★★★★★ 핵심 분기점 ★★★★★
        // MonsterDataSO의 'useNewAI' 플래그를 확인하여 어떤 AI 시스템을 사용할지 결정합니다.
        if (monsterData.useNewAI && monsterData.initialBehavior != null)
        {
            // --- 신규 모듈형 AI 시스템 사용 ---
            Debug.Log($"<color=cyan>[AI-Init]</color> Monster: '{gameObject.name}', AI System: 'New Modular AI', Initial Behavior: '{monsterData.initialBehavior.name}'");
            ChangeBehavior(monsterData.initialBehavior);
        }
        else
        {
            // --- 기존 FSM AI 시스템 사용 (호환성 유지) ---
            Debug.Log($"<color=orange>[AI-Init]</color> Monster: '{gameObject.name}', AI System: 'Legacy FSM'");
            // (기존 FSM 초기화 코드는 생략... 원본과 동일)
            #region Legacy FSM Initialization
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
            #endregion
        }
    }

    void Update()
    {
        if (isDead) return;

        // ★★★★★ 핵심 실행부 ★★★★★
        if (currentBehavior != null) // 'currentBehavior'가 존재하면 신규 AI가 실행 중이라는 의미
        {
            // [개선 4. 메모리] 매 프레임이 아닌 0.2초마다 한 번씩만 행동을 결정하도록 하여 부하를 줄입니다.
            behaviorCheckTimer += Time.deltaTime;
            stateTimer += Time.deltaTime; // 현재 상태 유지 시간 측정

            if (behaviorCheckTimer >= 0.2f)
            {
                // 현재 장착된 '행동 부품'에게 "이제 네가 할 일을 해"라고 명령합니다.
                currentBehavior.OnExecute(this);
                behaviorCheckTimer = 0f;
            }
        }
        else // 'currentBehavior'가 없다면, 기존 AI가 실행 중이라는 의미
        {
            // (기존 FSM 업데이트 로직은 생략... 원본과 동일)
            #region Legacy FSM Update
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
            #endregion
        }
    }

    void FixedUpdate()
    {
        // FixedUpdate는 물리 계산을 위한 업데이트 함수입니다.
        // 신규 AI는 Behavior 부품이 직접 물리(rb.velocity)를 제어하므로,
        // 신규 AI가 실행 중일 때는 이 함수가 아무것도 하지 않도록 막습니다.
        if (currentBehavior != null) return;

        // (기존 FSM 물리 이동 로직은 생략... 원본과 동일)
        #region Legacy FSM FixedUpdate
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
        #endregion
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

        // [개선 3. 보기 편한 로그] 상태 전환 시 상세한 로그를 남깁니다.
        string oldBehaviorName = (currentBehavior != null) ? currentBehavior.name : "None";
        string newBehaviorName = (newBehavior != null) ? newBehavior.name : "None";

        // 1. 기존 행동이 있었다면, 마무리(OnExit) 함수를 호출하여 안전하게 종료시킵니다.
        if (currentBehavior != null)
        {
            currentBehavior.OnExit(this);
        }

        // 2. 현재 행동 부품을 새로운 것으로 교체합니다.
        currentBehavior = newBehavior;
        stateTimer = 0f; // 상태가 바뀌었으므로 타이머를 리셋합니다.

        // 3. 새로운 행동의 초기화(OnEnter) 함수를 호출하여 시작 준비를 합니다.
        if (currentBehavior != null)
        {
            currentBehavior.OnEnter(this);
        }
    }

    // (이 아래의 모든 함수들은 기존 스크립트와 동일하므로 생략합니다)
    #region Unchanged Methods (Legacy AI, Common Functions)
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
    public void SetVelocity(Vector2 newVelocity)
    {
        if (isDead) return;
        rb.velocity = newVelocity;
        isVelocityOverridden = true;
    }
    void OnCollisionEnter2D(Collision2D collision) { CheckForPlayer(collision.gameObject); }
    void OnTriggerEnter2D(Collider2D other) { CheckForPlayer(other.gameObject); }
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
    void OnCollisionExit2D(Collision2D collision) { LeavePlayer(collision.gameObject); }
    void OnTriggerExit2D(Collider2D other) { LeavePlayer(other.gameObject); }
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
        if (string.IsNullOrEmpty(shotID)) return true;
        if (allowMultipleHits) return true;
        if (_lastHitFrame != Time.frameCount)
        {
            _hitShotIDsThisFrame.Clear();
            _lastHitFrame = Time.frameCount;
        }
        if (_hitShotIDsThisFrame.Contains(shotID)) return false;
        _hitShotIDsThisFrame.Add(shotID);
        return true;
    }
    void OnDisable()
    {
        if (ServiceLocator.IsRegistered<MonsterManager>())
        {
            ServiceLocator.Get<MonsterManager>()?.UnregisterMonster(this);
        }
    }
    #endregion
}