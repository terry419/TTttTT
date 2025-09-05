// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterController.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(MonsterStats))]
public class MonsterController : MonoBehaviour
{
    public static event Action<MonsterController> OnMonsterDied;

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
    [HideInInspector] public int chargeState;
    [HideInInspector] public Vector2 chargeDirection;
    [HideInInspector] public float chargeDistanceRemaining;
    [HideInInspector] public Dictionary<ScriptableObject, float> cooldownTimers = new Dictionary<ScriptableObject, float>();
    [HideInInspector] public bool countsTowardKillGoal = true;

    private int _originalLayer;
    private MonsterGlobalModifiers _globalModifiers;
    private bool isDead = false;

    private bool isTouchingPlayer = false;
    private const float DAMAGE_INTERVAL = 0.1f;
    private float damageTimer = 0f;
    #endregion

    #region Initialization & Lifecycle
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        monsterStats = GetComponent<MonsterStats>();
        _originalLayer = gameObject.layer;
        _globalModifiers = GetComponent<MonsterGlobalModifiers>();
    }

    void OnEnable()
    {
        ServiceLocator.Get<MonsterManager>()?.RegisterMonster(this);
        isDead = false;
        isTouchingPlayer = false; // 활성화 시 초기화
        damageTimer = 0f;       // 활성화 시 초기화
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
            playerRigidbody = playerController.rb;
        }
    }

    public void Initialize(MonsterDataSO data)
    {
        this.monsterData = data;
        monsterStats.Initialize(this, data);
        startPosition = transform.position;
        isDead = false;

        if (_globalModifiers != null)
            _globalModifiers.Initialize(monsterData.globalModifierRules);

        if (monsterData.useNewAI && monsterData.initialBehavior != null)
        {
            ChangeBehavior(monsterData.initialBehavior);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (isTouchingPlayer)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= DAMAGE_INTERVAL)
            {
                ApplyContactDamage();
                damageTimer = 0f;
            }
        }

        if (currentBehavior == null) return;

        behaviorCheckTimer += Time.deltaTime;
        stateTimer += Time.deltaTime;

        if (behaviorCheckTimer >= 0.2f)
        {
            currentBehavior.OnExecute(this);
            behaviorCheckTimer = 0f;
        }
    }
    #endregion

    #region Collision Methods
    // 
    private void HandlePlayerContact(GameObject target, bool isEntering)
    {
        if (!target.CompareTag(Tags.Player)) return;

        isTouchingPlayer = isEntering;

        if (isEntering)
        {
            // 돌진 중인 경우 즉시 자폭 로직으로 넘어감
            if (currentBehavior is ChargeBehavior && chargeState == 2)
            {
                Log.Info(Log.LogCategory.AI_Behavior, $"[진단] {name}이(가) 플레이어와 '돌진 중 충돌'! 자폭을 시도합니다.");
                monsterStats.HandleDeath().Forget();
            }
            else // 일반 접촉인 경우 타이머 시작
            {
                Log.Info(Log.LogCategory.AI_Behavior, $"[진단] {name}이(가) 플레이어와 '일반 접촉' 시작.");
                damageTimer = DAMAGE_INTERVAL;
            }
        }
        else
        {
            Log.Info(Log.LogCategory.AI_Behavior, $"[진단] {name}이(가) 플레이어와 접촉 해제.");
        }
    }
    private void ApplyContactDamage()
    {
        if (playerTransform != null && playerTransform.TryGetComponent<CharacterStats>(out var playerStats))
        {
            Log.Info(Log.LogCategory.AI_Behavior, $"[진단] {name}이(가) 플레이어에게 접촉 피해({monsterStats.FinalContactDamage})를 입히려 합니다.");
            playerStats.TakeDamage(monsterStats.FinalContactDamage);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Player))
        {
            Log.Info(Log.LogCategory.AI_Behavior, $"[진단] OnCollisionEnter2D 충돌 감지: {name} -> {collision.gameObject.name}");
            isTouchingPlayer = true;
            damageTimer = DAMAGE_INTERVAL; 

            if (currentBehavior is ChargeBehavior && chargeState == 2)
            {
                Log.Info(Log.LogCategory.AI_Behavior, $"{name}이(가) 플레이어와 충돌하여 자폭합니다!");
                monsterStats.HandleDeath().Forget(); 
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Tags.Player))
        {
            Log.Info(Log.LogCategory.AI_Behavior, $"[진단] OnCollisionExit2D 충돌 해제: {name} -> {collision.gameObject.name}");
            isTouchingPlayer = false;
        }
    }

    // 트리거 콜라이더를 사용할 경우를 대비한 로직
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Player))
        {
            Log.Info(Log.LogCategory.AI_Behavior, $"[진단] OnTriggerEnter2D 충돌 감지: {name} -> {other.name}");
            isTouchingPlayer = true;
            damageTimer = DAMAGE_INTERVAL;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Player))
        {
            isTouchingPlayer = false;
        }
    }
    #endregion

    #region AI Core & Public API
    public void ChangeBehavior(MonsterBehavior newBehavior)
    {
        if (currentBehavior == newBehavior) return;

        string oldBehaviorName = (currentBehavior != null) ? currentBehavior.name : "None";
        string newBehaviorName = (newBehavior != null) ? newBehavior.name : "None";
        Log.Info(Log.LogCategory.AI_Transition, $"Monster: '{gameObject.name}', Behavior Changed: '{oldBehaviorName}' -> '{newBehaviorName}'");

        if (currentBehavior != null)
            currentBehavior.OnExit(this);

        currentBehavior = newBehavior;
        stateTimer = 0f;

        if (currentBehavior != null)
            currentBehavior.OnEnter(this);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        OnMonsterDied?.Invoke(this);

        // 실제 오브젝트 풀 반환은 PoolManager가 OnDisable을 통해 처리하도록 유도
        gameObject.SetActive(false);
    }

    // 다른 스크립트들이 필요로 하는 공개 기능들
    public void TakeDamage(float damage) => monsterStats.TakeDamage(damage);
    public float GetCurrentHealth() => monsterStats.CurrentHealth;
    public void SetVelocity(Vector2 newVelocity) { rb.velocity = newVelocity; }
    public bool RegisterHitByShot(string shotID, bool allowMultipleHits)
    {
        // 이 기능은 몬스터별로 고유해야 하므로 Controller가 담당
        // (코드는 기존과 동일하여 생략)
        return true;
    }
    public void SetInvulnerable(float duration) => StartCoroutine(InvulnerableRoutine(duration));
    private IEnumerator InvulnerableRoutine(float duration)
    {
        // (코드는 기존과 동일하여 생략)
        yield return null;
    }
    public void ApplySelfStatusEffect(MonsterStatusEffectSO effectData) => monsterStats.ApplySelfStatusEffect(effectData);
    public void RemoveSelfStatusEffect(string effectId) => monsterStats.RemoveSelfStatusEffect(effectId);

    public void SetLayer(string layerName) { gameObject.layer = LayerMask.NameToLayer(layerName); }
    public void ResetLayer() { gameObject.layer = _originalLayer; }
    #endregion
}