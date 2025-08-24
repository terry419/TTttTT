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

    public HashSet<string> hitShotIDs = new HashSet<string>();

    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        // ... 기존 코드 ...
        ServiceLocator.Get<MonsterManager>()?.RegisterMonster(this); // 활성화될 때 리스트에 추가
        hitShotIDs.Clear();
        isDead = false;
        if (monsterData != null)
        {
            currentHealth = monsterData.maxHealth;
        }
        isInvulnerable = false;
    }

    void OnDisable()
    {
        // ... 기존 코드 ...
        ServiceLocator.Get<MonsterManager>()?.UnregisterMonster(this); // 비활성화될 때 리스트에서 제거
    }

    

    public void Initialize(MonsterDataSO data)
    {
        monsterData = data;
        maxHealth = monsterData.maxHealth;
        moveSpeed = monsterData.moveSpeed;
        contactDamage = monsterData.contactDamage;
        currentHealth = maxHealth;
        hitShotIDs.Clear();
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
        // --- [수정] ServiceLocator를 통해 PlayerController를 찾아옵니다. ---
        var playerController = ServiceLocator.Get<PlayerController>();
        if (playerController != null)
        {
            playerTransform = playerController.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] PlayerController 인스턴스를 찾을 수 없습니다! 스크립트를 비활성화합니다.");
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckForPlayer(collision.gameObject);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        LeavePlayer(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead)
        {
            return;
        }

        if (other.TryGetComponent<BulletController>(out var hitBullet))
        {
            if (hitShotIDs.Contains(hitBullet.shotInstanceID))
            {
                ServiceLocator.Get<PoolManager>().Release(other.gameObject);
                return;
            }

            hitShotIDs.Add(hitBullet.shotInstanceID);
            TakeDamage(hitBullet.damage);

            if (hitBullet.SourceCard != null && hitBullet.SourceCard.statusEffectToApply != null)
            {
                ServiceLocator.Get<StatusEffectManager>().ApplyStatusEffect(this.gameObject, hitBullet.SourceCard.statusEffectToApply);
            }

            if (hitBullet.SourceCard != null && hitBullet.SourceCard.secondaryEffect != null)
            {
                ServiceLocator.Get<EffectExecutor>().Execute(hitBullet.SourceCard.secondaryEffect, GetComponent<CharacterStats>(), this.transform);
            }

            ServiceLocator.Get<PoolManager>().Release(other.gameObject);
        }
        else
        {
            CheckForPlayer(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        LeavePlayer(other.gameObject);
    }

    private void CheckForPlayer(GameObject target)
    {
        if (target.GetComponent<CharacterStats>() != null)
        {
            isTouchingPlayer = true;
            damageTimer = DAMAGE_INTERVAL;
        }
    }

    private void LeavePlayer(GameObject target)
    {
        if (target.GetComponent<CharacterStats>() != null)
        {
            isTouchingPlayer = false;
            damageTimer = 0f;
        }
    }

    private void ApplyContactDamage()
    {
        if (playerTransform != null)
        {
            CharacterStats playerStats = playerTransform.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(contactDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        OnMonsterDamaged?.Invoke(damage, transform.position);

        if (currentHealth <= 0) Die();
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
        StopCoroutine("InvulnerableRoutine");
        StartCoroutine(InvulnerableRoutine(duration));
    }

    private IEnumerator InvulnerableRoutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }
}