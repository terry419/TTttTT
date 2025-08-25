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
        // 1. ServiceLocator에 MonsterManager가 아직 등록되어 있는지 먼저 확인합니다.
        if (ServiceLocator.IsRegistered<MonsterManager>())
        {
            // 2. 등록되어 있을 경우에만 안전하게 Get을 호출하여 Unregister를 실행합니다.
            ServiceLocator.Get<MonsterManager>()?.UnregisterMonster(this);
        }
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

        // 1. 부딪힌 것이 BulletController 컴포넌트를 가진 오브젝트인지 확인합니다.
        if (other.TryGetComponent<BulletController>(out var bullet))
        {
            if (bullet == null || bullet.SourceCard == null) return;

            // 관통/다단히트 방지를 위해 이미 처리한 총알인지 확인
            if (hitShotIDs.Contains(bullet.shotInstanceID))
            {
                ServiceLocator.Get<PoolManager>().Release(other.gameObject); // 이미 처리한 총알도 풀로 돌려보냅니다.
                return;
            }
            hitShotIDs.Add(bullet.shotInstanceID);
            
            // 2. 총알의 데미지를 몬스터에게 적용합니다.
            TakeDamage(bullet.damage);

            // 3. ★★★ 핵심 로직: 총알을 쏜 원본 카드에 'secondaryEffect'가 있는지 확인합니다. ★★★
            CardDataSO secondaryEffectCard = bullet.SourceCard.secondaryEffect;
            if (secondaryEffectCard != null)
            {
                // 4. EffectExecutor와 시전자(Player)의 정보를 가져옵니다.
                var effectExecutor = ServiceLocator.Get<EffectExecutor>();
                var playerController = ServiceLocator.Get<PlayerController>();

                // 5. 시전자 정보가 유효한지 최종 확인합니다.
                if (effectExecutor != null && playerController != null)
                {
                    // ▼▼▼ 디버그 로그 추가 ▼▼▼
                    Debug.Log($"[디버그 1] 기본 카드 '{bullet.SourceCard.name}'가 명중. 보조 효과 '{secondaryEffectCard.name}'를 발동합니다.");
                    
                    // 6. secondaryEffect를 실행합니다.
                    //    - CardData: secondaryEffect 카드 ('Zone' 카드)
                    //    - CasterStats: 플레이어의 CharacterStats
                    //    - SpawnPoint: 몬스터 자신의 위치 (this.transform)
                    effectExecutor.Execute(secondaryEffectCard, playerController.GetComponent<CharacterStats>(), this.transform);
                }
            }
            
            // 7. 총알을 풀(Pool)로 돌려보내 비활성화시킵니다.
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