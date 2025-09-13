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
    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public Rigidbody2D targetRigidbody;
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
        isTouchingPlayer = false;
        damageTimer = 0f;
    }


    void OnDisable()
    {
        if (ServiceLocator.IsRegistered<MonsterManager>())
        {
            ServiceLocator.Get<MonsterManager>()?.UnregisterMonster(this);
        }
    }

    public void Initialize(MonsterDataSO data, Transform target)
    {
        this.monsterData = data;
        monsterStats.Initialize(this, data);
        startPosition = transform.position;
        isDead = false;

        // 공격 대상 설정
        this.targetTransform = target;
        if (target != null)
        {
            this.targetRigidbody = target.GetComponent<Rigidbody2D>();
        }


        if (_globalModifiers != null)
            _globalModifiers.Initialize(monsterData.globalModifierRules);

        if (monsterData.useNewAI && monsterData.initialBehavior != null)
        {
            ChangeBehavior(monsterData.initialBehavior);
        }
        else
        {
            Debug.LogWarning($"<color=orange>[AI Init Debug]</color> {gameObject.name}: InitialBehavior가 설정되지 않았거나 useNewAI가 false입니다. (useNewAI: {monsterData.useNewAI}, initialBehavior: {(monsterData.initialBehavior == null ? "null" : monsterData.initialBehavior.name)})");
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
    // [수정] playerTransform -> targetTransform
    private void ApplyContactDamage()
    {
        if (targetTransform != null && targetTransform.TryGetComponent<EntityStats>(out var targetStats))
        {
            Log.Info(Log.LogCategory.AI_Behavior, $"[진단] {name}이(가) 대상({targetTransform.name})에게 접촉 피해({monsterStats.FinalContactDamage})를 입히려 합니다.");
            targetStats.TakeDamage(monsterStats.FinalContactDamage);
        }
    }
    // OnCollisionEnter2D, OnCollisionExit2D, OnTriggerEnter2D, OnTriggerExit2D 메서드는 수정할 필요 없습니다.
    // 'Player' 태그를 가진 대상과의 충돌만 감지하면 되기 때문입니다.
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

        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage) => monsterStats.TakeDamage(damage);
    public float GetCurrentHealth() => monsterStats.CurrentHealth;
    public void SetVelocity(Vector2 newVelocity) { rb.velocity = newVelocity; }
    public bool RegisterHitByShot(string shotID, bool allowMultipleHits)
    {
        return true;
    }
    public void SetInvulnerable(float duration) => StartCoroutine(InvulnerableRoutine(duration));
    private IEnumerator InvulnerableRoutine(float duration)
    {
        yield return null;
    }
    public void ApplySelfStatusEffect(MonsterStatusEffectSO effectData) => monsterStats.ApplySelfStatusEffect(effectData);
    public void RemoveSelfStatusEffect(string effectId) => monsterStats.RemoveSelfStatusEffect(effectId);

    public void SetLayer(string layerName) { gameObject.layer = LayerMask.NameToLayer(layerName); }
    public void ResetLayer() { gameObject.layer = _originalLayer; }
    #endregion
}