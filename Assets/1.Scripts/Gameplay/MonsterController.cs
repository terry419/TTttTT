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
        if (isDead) return;

        if (other.TryGetComponent<BulletController>(out var bullet))
        {
            if (bullet == null || bullet.SourceCard == null) return;

            if (hitShotIDs.Contains(bullet.shotInstanceID))
            {
                ServiceLocator.Get<PoolManager>().Release(other.gameObject);
                return;
            }
            hitShotIDs.Add(bullet.shotInstanceID);

            // --- 크리티컬 로직 시작 ---
            float finalDamage = bullet.damage; // 총알로부터 기본 데미지를 가져옵니다.

            var playerController = ServiceLocator.Get<PlayerController>();
            if (playerController != null)
            {
                var playerStats = playerController.GetComponent<CharacterStats>();
                if (playerStats != null)
                {
                    // 크리티컬 확률을 계산합니다.
                    if (Random.Range(0f, 100f) < playerStats.FinalCritRate)
                    {
                        // 크리티컬 발생!
                        Debug.Log("CRITICAL HIT!");
                        finalDamage *= (1 + playerStats.FinalCritDamage / 100f);

                        // 크리티컬 이펙트를 생성합니다.
                        var prefabProvider = ServiceLocator.Get<PrefabProvider>();
                        if (prefabProvider != null && prefabProvider.critEffectPrefab != null)
                        {
                            var poolManager = ServiceLocator.Get<PoolManager>();
                            if (poolManager != null)
                            {
                                GameObject critEffect = poolManager.Get(prefabProvider.critEffectPrefab);
                                if (critEffect != null)
                                {
                                    critEffect.transform.position = transform.position;
                                    critEffect.transform.rotation = Quaternion.identity;
                                }
                            }
                        }
                    }
                }
            }
            // --- 크리티컬 로직 끝 ---

            // 최종 계산된 데미지를 몬스터에게 적용합니다.
            TakeDamage(finalDamage);

            // --- 흡혈 로직 시작 ---
            if (playerController != null)
            {
                var playerStats = playerController.GetComponent<CharacterStats>();
                if (playerStats != null && bullet.SourceCard != null && bullet.SourceCard.triggerType == TriggerType.OnHit && bullet.SourceCard.lifestealPercentage > 0 && finalDamage > 0)
                {
                    Debug.Log($"[Lifesteal Debug] 흡혈 조건 충족! 카드: {bullet.SourceCard.cardName}, 입힌 데미지: {finalDamage:F2}");
                    float lifestealRatio = bullet.SourceCard.lifestealPercentage / 100f;
                    float healAmount = finalDamage * lifestealRatio;
                    Debug.Log($"[Lifesteal Debug] 흡혈 비율: {bullet.SourceCard.lifestealPercentage}% ({lifestealRatio:P2}), 회복량: {healAmount:F2}");
                    playerStats.Heal(healAmount);
                    Debug.Log($"[Lifesteal Debug] 플레이어 체력 {healAmount:F2} 회복 요청됨.");
                }
            }
            // --- 흡혈 로직 끝 ---

            // 상태 이상 적용 로직
            if (bullet.SourceCard != null && bullet.SourceCard.statusEffectToApply != null)
            {
                Debug.Log($"[StatusEffect] {bullet.SourceCard.statusEffectToApply.effectName} 효과를 적용합니다.");
                ServiceLocator.Get<StatusEffectManager>().ApplyStatusEffect(this.gameObject, bullet.SourceCard.statusEffectToApply);
            }

            // 기존 보조 효과 로직
            CardDataSO secondaryEffectCard = bullet.SourceCard.secondaryEffect;
            if (secondaryEffectCard != null)
            {
                var effectExecutor = ServiceLocator.Get<EffectExecutor>();
                if (effectExecutor != null && playerController != null)
                {
                    Debug.Log($"[디버그 1] 기본 카드 '{bullet.SourceCard.name}'가 명중. 보조 효과 '{secondaryEffectCard.name}'를 발동합니다.");
                    effectExecutor.Execute(secondaryEffectCard, playerController.GetComponent<CharacterStats>(), this.transform, finalDamage);
                }
            }

            Debug.Log($"[MonsterController] 총알 {other.gameObject.name} (ID: {other.gameObject.GetInstanceID()}) 풀로 반환 요청.");
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