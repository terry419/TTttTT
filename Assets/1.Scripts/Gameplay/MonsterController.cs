// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/MonsterController.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(Rigidbody2D), typeof(MonsterStats))]
public class MonsterController : MonoBehaviour
{
    // --- 이벤트 방송 ---
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
        monsterStats.Initialize(this, data); // MonsterStats에게 자신을 알려줍니다.
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
        if (isDead || currentBehavior == null) return;

        behaviorCheckTimer += Time.deltaTime;
        stateTimer += Time.deltaTime;

        if (behaviorCheckTimer >= 0.2f)
        {
            currentBehavior.OnExecute(this);
            behaviorCheckTimer = 0f;
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

    // 이 아래 함수들은 이제 MonsterStats가 대신 처리하므로, Controller에서는 삭제됩니다.
    // public void TakeDamage, public float GetCurrentHealth 등...

    /// <summary>
    /// 몬스터가 죽었을 때 호출되는 최종 함수. 오직 이 클래스만 OnMonsterDied 이벤트를 호출합니다.
    /// </summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        OnMonsterDied?.Invoke(this); // "내가 죽었다!" 라고 게임 전체에 방송합니다.

        gameObject.SetActive(false); // 오브젝트 풀로 돌아가기 전에 비활성화합니다.
    }
    #endregion

    #region Unchanged Helper Methods
    public void SetLayer(string layerName) { gameObject.layer = LayerMask.NameToLayer(layerName); }
    public void ResetLayer() { gameObject.layer = _originalLayer; }
    // (ApplySelfStatusEffect 와 RemoveSelfStatusEffect는 다음 단계에서 MonsterStats로 이전됩니다.)
    #endregion
}