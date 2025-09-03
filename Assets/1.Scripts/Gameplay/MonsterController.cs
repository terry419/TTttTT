using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(MonsterStats))]
public class MonsterController : MonoBehaviour
{
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
        Debug.Log($"<color=red>[몬스터 사망 추적] 2/2: {gameObject.name}의 OnDisable()이 호출되었습니다. 이 시점에 장판의 OnTriggerExit2D가 호출될 수 있습니다.</color>");

        if (ServiceLocator.IsRegistered<MonsterManager>())
        {
            ServiceLocator.Get<MonsterManager>()?.UnregisterMonster(this);
        }
    }

    public void Initialize(MonsterDataSO data)
    {
        monsterStats.Initialize(data); // [✨ 변경] MonsterStats 초기화
        maxHealth = monsterStats.FinalMaxHealth; // 초기 maxHealth값 할당
        isDead = false;
    }


    void Update()
    {
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
        if (playerTransform == null)
        {
            var playerController = ServiceLocator.Get<PlayerController>();
            if (playerController != null) playerTransform = playerController.transform;
            else { rb.velocity = Vector2.zero; return; }
        }

        if (isInvulnerable || isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = direction * monsterStats.FinalMoveSpeed;
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
        // [추가] 실제로 들어오는 최종 피해량을 확인하기 위한 디버그 로그
        Debug.Log($"<color=red>[Damage Calculation]</color> Monster '{gameObject.name}' is about to take {damage} damage.");

        if (isDead || isInvulnerable) return;

        monsterStats.TakeDamage(damage); 

        if (monsterStats.IsDead())
        {
            Die();
        }
    }

    /// <summary>
    /// [ 신규 추가] MonsterStats로부터 피해를 받았음을 알림받고, 이벤트를 발생시키는 메서드입니다.
    /// </summary>
    public void NotifyDamageTaken(float finalDamage)
    {
        OnMonsterDamaged?.Invoke(finalDamage, transform.position);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"<color=red>[몬스터 사망 추적] 1/2: {gameObject.name}의 Die() 메서드가 호출되었습니다. OnMonsterDied 이벤트를 호출하고 PoolManager에 반환을 요청합니다.</color>");

        OnMonsterDied?.Invoke(this);
        ServiceLocator.Get<PoolManager>().Release(gameObject);
    }

    // [ 신규 추가] 다른 스크립트에서 체력 정보를 안전하게 가져갈 수 있는 통로입니다.
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