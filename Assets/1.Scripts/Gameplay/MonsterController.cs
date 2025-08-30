using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// 몬스터의 행동(이동, 충돌)을 제어하고, 능력치(MonsterStats)와 연동합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MonsterStats))] // MonsterStats 컴포넌트가 반드시 필요함을 명시
public class MonsterController : MonoBehaviour
{
    public static event System.Action<float, Vector3> OnMonsterDamaged;
    public static event System.Action<MonsterController> OnMonsterDied;

    // 몬스터의 능력치 데이터는 MonsterStats 컴포넌트가 관리합니다.
    private MonsterStats stats;
    private MonsterDataSO monsterData;
    private Transform playerTransform;
    private Rigidbody2D rb;

    private const float DAMAGE_INTERVAL = 0.1f;
    private float damageTimer = 0f;
    private bool isTouchingPlayer = false;

    private HashSet<string> _hitShotIDsThisFrame = new HashSet<string>();
    private int _lastHitFrame;
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<MonsterStats>(); // 자신의 MonsterStats 컴포넌트 참조
    }

    void OnEnable()
    {
        ServiceLocator.Get<MonsterManager>()?.RegisterMonster(this);
        isDead = false;

        if (monsterData != null)
        {
            // 재활성화 시 MonsterStats 초기화
            stats.Initialize(monsterData);
        }

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

    /// <summary>
    /// 몬스터를 초기화합니다. 능력치 설정은 MonsterStats에 위임합니다.
    /// </summary>
    public void Initialize(MonsterDataSO data)
    {
        monsterData = data;
        stats.Initialize(data); // MonsterStats 컴포넌트 초기화 호출
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
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
            else
            {
                rb.velocity = Vector2.zero;
                return;
            }
        }

        // isInvulnerable 상태를 MonsterStats에서 가져와 확인
        if (stats.isInvulnerable || isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        // moveSpeed를 MonsterStats의 최종 계산된 값에서 가져옴
        rb.velocity = direction * stats.FinalMoveSpeed;
    }

    #region 플레이어와의 충돌 처리
    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForPlayer(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        CheckForPlayer(other.gameObject);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        LeavePlayer(collision.gameObject);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        LeavePlayer(other.gameObject);
    }

    private void CheckForPlayer(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            isTouchingPlayer = true;
            damageTimer = DAMAGE_INTERVAL; // 즉시 데미지
        }
    }

    private void LeavePlayer(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            isTouchingPlayer = false;
            damageTimer = 0f;
        }
    }
    #endregion

    /// <summary>
    /// 플레이어에게 접촉 데미지를 입힙니다. 데미지 값은 MonsterStats에서 가져옵니다.
    /// </summary>
    private void ApplyContactDamage()
    {
        if (playerTransform != null && playerTransform.TryGetComponent<CharacterStats>(out var playerStats))
        {
            // baseDamage를 접촉 데미지로 사용
            playerStats.TakeDamage(stats.baseStats.baseDamage);
        }
    }

    /// <summary>
    /// MonsterStats로부터 호출되는 피해 피드백 처리용 메소드입니다.
    /// </summary>
    public void HandleDamageFeedback(float damage)
    {
        // 데미지 이벤트 호출 등, 시각/청각적 피드백 관련 로직
        OnMonsterDamaged?.Invoke(damage, transform.position);
    }

    /// <summary>
    /// MonsterStats로부터 호출되는 사망 처리 메소드입니다.
    /// </summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        OnMonsterDied?.Invoke(this);
        ServiceLocator.Get<PoolManager>().Release(gameObject);
    }

    /// <summary>
    /// 외부에서 몬스터를 일시적인 무적 상태로 만듭니다.
    /// </summary>
    public void SetInvulnerable(float duration)
    {
        StartCoroutine(InvulnerableRoutine(duration));
    }

    private IEnumerator InvulnerableRoutine(float duration)
    {
        stats.isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        stats.isInvulnerable = false;
    }

    /// <summary>
    /// 투사체로부터의 피격 등록을 처리합니다. (중복 피격 방지)
    /// </summary>
    public bool RegisterHitByShot(string shotID, bool allowMultipleHits)
    {
        if (allowMultipleHits) return true;

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