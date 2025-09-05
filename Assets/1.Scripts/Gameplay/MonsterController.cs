// 경로: ./TTttTT/Assets/1.Scripts/Gameplay/MonsterController.cs
// (상단 using 부분은 생략했습니다. 코드 내용은 모두 포함됩니다.)
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D), typeof(MonsterStats))]
public class MonsterController : MonoBehaviour
{
    #region Variables
    private MonsterBehavior currentBehavior;
    private float behaviorCheckTimer = 0f;

    [HideInInspector] public MonsterDataSO monsterData;
    [HideInInspector] public MonsterStats monsterStats;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Transform playerTransform;
    [HideInInspector] public Rigidbody2D playerRigidbody;
    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public float stateTimer = 0f;

    [HideInInspector] public int chargeState; // 돌진의 내부 상태 (0: 조준, 1: 준비, 2: 돌진)
    [HideInInspector] public Vector2 chargeDirection; // 자신만의 돌진 방향
    [HideInInspector] public float chargeDistanceRemaining; // 자신만의 남은 돌진 거리

    private int _originalLayer;

    #region Legacy FSM Variables
    private enum State { Spawning, Chasing, Patrolling, Fleeing }
    private State currentState = State.Spawning;
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

    #region Common Variables
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
    #endregion

    // (이 아래의 모든 함수들은 이전과 동일합니다. ApplySelfStatusEffect 함수만 추가되었습니다.)
    #region Methods
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        monsterStats = GetComponent<MonsterStats>();
        _originalLayer = gameObject.layer;
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
            playerRigidbody = playerController.rb;
        }
    }

    public void Initialize(MonsterDataSO data)
    {
        if (data == null)
        {
            Log.Error(Log.LogCategory.AI_Init, $"몬스터 '{gameObject.name}'에 MonsterDataSO가 할당되지 않았습니다!");
            gameObject.SetActive(false);
            return;
        }

        this.monsterData = data;
        monsterStats.Initialize(data);
        startPosition = transform.position;
        isDead = false;

        if (monsterData.useNewAI && monsterData.initialBehavior != null)
        {
            Log.Info(Log.LogCategory.AI_Init, $"Monster: '{gameObject.name}', AI System: 'New Modular AI', Initial Behavior: '{monsterData.initialBehavior.name}'");
            ChangeBehavior(monsterData.initialBehavior);
        }
        else
        {
            Log.Info(Log.LogCategory.AI_Init, $"Monster: '{gameObject.name}', AI System: 'Legacy FSM'");
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
                default:
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

        if (currentBehavior != null)
        {
            behaviorCheckTimer += Time.deltaTime;
            stateTimer += Time.deltaTime;

            if (behaviorCheckTimer >= 0.2f)
            {
                currentBehavior.OnExecute(this);
                behaviorCheckTimer = 0f;
            }
        }
        else
        {
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

    public void ChangeBehavior(MonsterBehavior newBehavior)
    {
        if (currentBehavior == newBehavior) return;

        string oldBehaviorName = (currentBehavior != null) ? currentBehavior.name : "None";
        string newBehaviorName = (newBehavior != null) ? newBehavior.name : "None";
        Log.Info(Log.LogCategory.AI_Transition, $"Monster: '{gameObject.name}', Behavior Changed: '{oldBehaviorName}' -> '{newBehaviorName}'");

        if (currentBehavior != null)
        {
            currentBehavior.OnExit(this);
        }

        currentBehavior = newBehavior;
        stateTimer = 0f;

        if (currentBehavior != null)
        {
            currentBehavior.OnEnter(this);
        }
    }

    public void SetLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public void ResetLayer()
    {
        gameObject.layer = _originalLayer;
    }


    public void ApplySelfStatusEffect(StatusEffectDataSO effectData)
    {
        if (effectData == null) return;

        var statusEffectManager = ServiceLocator.Get<StatusEffectManager>();
        if (statusEffectManager != null)
        {
            var instance = new StatusEffectInstance(this.gameObject, effectData);
            statusEffectManager.ApplyStatusEffect(this.gameObject, instance);
            Log.Info(Log.LogCategory.AI_Behavior, $"{name}이(가) 자신에게 '{effectData.effectName}' 효과를 적용했습니다.");
        }
    }

    #region Unchanged Methods
    void FixedUpdate()
    {
        if (currentBehavior != null) return;
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
        Log.Info(Log.LogCategory.AI_Transition, $"[Legacy FSM] '{monsterData.monsterName}' 상태 변경: {currentState} -> {newState}");
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
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 플레이어와 부딪혔는지 확인
        if (collision.gameObject.CompareTag(Tags.Player))
        {
            // 2. 현재 행동이 '돌진' 중인지 확인 (ChargeBehavior의 Charging 상태 코드는 '2'입니다)
            if (currentBehavior is ChargeBehavior && chargeState == 2)
            {
                Log.Info(Log.LogCategory.AI_Behavior, $"{name}이(가) 플레이어와 충돌하여 자폭합니다!");

                // 3. 자폭 및 데미지 처리를 위해 Die() 함수를 즉시 호출
                Die().Forget();
            }
            else
            {
                // 이 부분이 없으면, 몬스터는 돌진할 때만 플레이어와 충돌하게 됩니다.
                CheckForPlayer(collision.gameObject);
            }
        }
    }
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
    public async UniTaskVoid Die()
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
    #endregion
}
#endregion