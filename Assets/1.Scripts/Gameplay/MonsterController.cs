using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;


[RequireComponent(typeof(Rigidbody2D), typeof(MonsterStats))]
public class MonsterController : MonoBehaviour
{
    // --- 상태 머신 관련 ---
    private enum State { Spawning, Chasing, Patrolling, Fleeing }
    private State currentState = State.Spawning;
    private State baseState; // 도망치기 전의 원래 상태를 저장할 변수
    private MonsterDataSO monsterData;
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

    // --- AI 성능 최적화용 변수 ---
    private const float STATE_CHECK_INTERVAL = 0.2f; // AI가 상태를 체크하는 주기 (0.2초)
    private float stateCheckTimer; // 다음 체크까지의 시간을 재는 타이머
    // --- 내부 참조 및 상태 변수 ---
    private Vector3 startPosition;
    private Vector3 patrolTargetPosition;
    // --- 기존 변수 선언부 (완벽 보존) ---
    public static event Action<float, Vector3> OnMonsterDamaged;
    public static event Action<MonsterController> OnMonsterDied;
    [HideInInspector] public float maxHealth; // MonsterStats에서 참조할 수 있도록 유지
    private MonsterStats monsterStats;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private bool isInvulnerable = false;
    private bool isDead = false;
    private const float DAMAGE_INTERVAL = 0.1f;
    private float damageTimer = 0f;
    private bool isTouchingPlayer = false;
    private HashSet<string> _hitShotIDsThisFrame = new HashSet<string>();
    private int _lastHitFrame;
    private bool isVelocityOverridden = false;
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
    void OnDisable()
    {
        if (ServiceLocator.IsRegistered<MonsterManager>())
        {
            ServiceLocator.Get<MonsterManager>()?.UnregisterMonster(this);
        }
    }
    public void Initialize(MonsterDataSO data)
    {
        if (data == null)
        {
            Debug.LogError("[MonsterController] Initialize에 유효한 MonsterDataSO가 전달되지 않았습니다! 스폰 로직을 확인하세요.", gameObject);
            // 유효한 데이터 없이는 몬스터가 아무것도 할 수 없으므로, 즉시 비활성화 처리 등을 고려할 수도 있습니다.
            gameObject.SetActive(false);
            return;
        }
        monsterData = data;
        monsterStats.Initialize(data);
        maxHealth = monsterStats.FinalMaxHealth;
        isDead = false;
        this.playerDetectionRadius = data.playerDetectionRadius;
        this.loseSightRadius = data.loseSightRadius;
        this.patrolRadius = data.patrolRadius;
        this.patrolSpeedMultiplier = data.patrolSpeedMultiplier;
        this.fleeTriggerRadius = data.fleeTriggerRadius;
        this.fleeSafeRadius = data.fleeSafeRadius;
        this.fleeSpeedMultiplier = data.fleeSpeedMultiplier;
        // Flee 파라미터 읽어오기
        this.fleeCondition = data.fleeCondition;
        this.fleeOnHealthPercentage = data.fleeOnHealthPercentage;
        this.allyCheckRadius = data.allyCheckRadius;
        this.fleeWhenAlliesLessThan = data.fleeWhenAlliesLessThan;
        startPosition = transform.position;
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
        Debug.Log($"<color=cyan>[FSM-Init]</color> '{monsterData.monsterName}' spawned. BaseBehavior={baseState}");
    }
    void Update()
    {
        if (isDead) return;
        stateCheckTimer += Time.deltaTime;
        if (stateCheckTimer >= STATE_CHECK_INTERVAL)
        {
            UpdateStateTransitions();
            stateCheckTimer = 0f;
        }
        // 기존 접촉 피해 로직 완벽 보존
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
    void FixedUpdate()
    {
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

    private void ExecuteFleeMovement()
    {
        // 플레이어로부터 멀어지는 방향 벡터를 계산합니다. (내 위치 - 플레이어 위치)
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        rb.velocity = direction * monsterStats.FinalMoveSpeed * fleeSpeedMultiplier;
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        if (isDead) return;
        rb.velocity = newVelocity;
        isVelocityOverridden = true;
    }
    private void UpdateStateTransitions()
    {
        if (playerTransform == null) return;
        
        if (monsterData.canFlee)
        {
            bool shouldFlee = CheckFleeCondition();
            
            // 현재 도망 상태가 아닌데, 도망쳐야 할 상황이라면
            if (currentState != State.Fleeing && shouldFlee)
            {
                ChangeState(State.Fleeing);
                return; // 도망 상태로 전환했으면 다른 로직은 체크할 필요 없음
            }
            // 현재 도망 상태인데, 더 이상 도망칠 필요가 없다면
            else if (currentState == State.Fleeing && !shouldFlee)
            {
                ChangeState(baseState); // 원래의 기본 상태로 복귀
                return;
            }
        }

        // Flee 상태가 아닐 때만 기존의 Patrol <-> Chase 전환 로직을 실행
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

    // 도망 조건을 확인하는 로직을 별도 함수로 분리
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

                bool decision = (allies.Length - 1) < fleeWhenAlliesLessThan;

                string logMessage = string.Format(
                    gameObject.name,
                    gameObject.GetInstanceID(),
                    allies.Length,
                    allies.Length - 1,
                    fleeWhenAlliesLessThan
                );

                return decision;
        }
        return false;
    }

    private void ExecutePatrolMovement()
    {
        // 목표 지점까지의 제곱 거리가 충분히 가까워지면 다음 목표를 갱신
        if ((patrolTargetPosition - transform.position).sqrMagnitude < 0.25f) // 0.5f * 0.5f
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
        var mapManager = ServiceLocator.Get<MapManager>();
        if (mapManager != null)
        {
            // 예시: 미래에 MapManager에 추가될 수 있는 함수.
            // 지금 당장 이 함수를 만들 필요는 없지만, 로직이 들어갈 위치를 명시해두는 것입니다.
            // patrolTargetPosition = mapManager.WrapPositionIfOutOfBounds(patrolTargetPosition);
        }
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
            Debug.Log("[DAMAGE-DEBUG 2/4] 충돌 대상이 Player임을 확인.");
            isTouchingPlayer = true;
            damageTimer = DAMAGE_INTERVAL; // 즉시 데미지를 주기 위해 타이머 초기화
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
    private void ApplyContactDamage()
    {
        if (playerTransform != null && playerTransform.TryGetComponent<CharacterStats>(out var playerStats))
        {
            playerStats.TakeDamage(monsterStats.FinalContactDamage);
        }
    }
    public void TakeDamage(float damage)
    {
        Debug.Log($"<color=red>[Damage Calculation]</color> Monster '{gameObject.name}' is about to take {damage} damage.");
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
        else
        {
            Debug.LogWarning($"[MonsterController] 자폭 VFX 프리팹({monsterData.explosionVfxRef.AssetGUID})을 스폰하지 못했습니다.");
        }

        float sqrDistanceToPlayer = (deathPosition - playerController.transform.position).sqrMagnitude;
        float explosionSqrRadius = monsterData.explosionRadius * monsterData.explosionRadius;

        if (sqrDistanceToPlayer <= explosionSqrRadius)
        {
            if (playerController.TryGetComponent<CharacterStats>(out var playerStats))
            {
                playerStats.TakeDamage(monsterData.explosionDamage);
                Debug.Log($"<color=red>[Player Damaged]</color> 몬스터 자폭으로 플레이어가 {monsterData.explosionDamage}의 피해를 입었습니다.");
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
    void OnCollisionExit2D(Collision2D collision)
    {
        LeavePlayer(collision.gameObject);
    }
    void OnTriggerExit2D(Collider2D other)
    {
        LeavePlayer(other.gameObject);
    }
    public bool RegisterHitByShot(string shotID, bool allowMultipleHits)
    {
        if (string.IsNullOrEmpty(shotID))
        {
            // ID가 없는 특수 공격은 중복 체크를 하지 않고 항상 유효한 공격으로 처리합니다.
            return true;
        }
        // 다중 히트가 허용되면, 항상 true를 반환하여 피해를 입게 합니다.
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