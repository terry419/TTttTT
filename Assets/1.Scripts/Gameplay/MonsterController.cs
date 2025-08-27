using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterController : MonoBehaviour
{
    public static event System.Action<float, Vector3> OnMonsterDamaged;
    public static event System.Action<MonsterController> OnMonsterDied;

    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float contactDamage;
    [HideInInspector] public float maxHealth;
    public float currentHealth;

    private MonsterDataSO monsterData;
    private Transform playerTransform;
    private bool isInvulnerable = false;
    private Rigidbody2D rb;

    private const float DAMAGE_INTERVAL = 0.1f;
    private float damageTimer = 0f;
    private bool isTouchingPlayer = false;

    // [수정] shotID 관련 로직은 BulletController로 이전되므로 이 변수는 더 이상 필요 없습니다.
    public HashSet<string> hitShotIDs = new HashSet<string>();

    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        ServiceLocator.Get<MonsterManager>()?.RegisterMonster(this);
        isDead = false;
        if (monsterData != null)
        {
            currentHealth = monsterData.maxHealth;
        }
        isInvulnerable = false;
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
        monsterData = data;
        maxHealth = data.maxHealth;
        moveSpeed = data.moveSpeed;
        contactDamage = data.contactDamage;
        currentHealth = maxHealth;
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
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
        else
        {
            this.enabled = false;
            return;
        }

        if (isInvulnerable || playerTransform == null || isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    // [수정] OnTriggerEnter2D 삭제 -> BulletController가 처리

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[DAMAGE-DEBUG 1A/4] OnCollisionEnter2D 충돌 감지! 대상: {collision.gameObject.name}");
        CheckForPlayer(collision.gameObject);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[DAMAGE-DEBUG 1B/4] OnTriggerEnter2D 충돌 감지! 대상: {other.gameObject.name}");
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
        Debug.Log("[DAMAGE-DEBUG 3/4] ApplyContactDamage 호출됨.");
        if (playerTransform != null && playerTransform.TryGetComponent<CharacterStats>(out var playerStats))
        {
            playerStats.TakeDamage(contactDamage);
        }
    }
    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;
        currentHealth -= damage;
        OnMonsterDamaged?.Invoke(damage, transform.position);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        OnMonsterDied?.Invoke(this);
        ServiceLocator.Get<PoolManager>().Release(gameObject);
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
}