// --- 파일명: MonsterController.cs (수정 제안) ---
// 경로: Assets/1.Scripts/Gameplay/MonsterController.cs
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

    // [추가] 몬스터의 사망 상태를 추적하는 플래그
    private bool isDead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        hitShotIDs.Clear();
        isDead = false;
        if (monsterData != null)
        {
            currentHealth = monsterData.maxHealth;
        }
        isInvulnerable = false;

        // 게임 상태 변경 이벤트를 구독합니다.
        if (ServiceLocator.Get<GameManager>() != null)
        {
            ServiceLocator.Get<GameManager>().OnGameStateChanged += HandleGameStateChanged;
        }
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독을 해제합니다.
        if (ServiceLocator.Get<GameManager>() != null)
        {
            ServiceLocator.Get<GameManager>().OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    /// <summary>
    /// 게임 상태 변경을 감지하여 처리합니다.
    /// </summary>
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        // 리워드 상태가 되면 몬스터를 비활성화하여 다음 씬으로 넘어가지 않도록 합니다.
        if (newState == GameManager.GameState.Reward)
        {
            if(gameObject.activeInHierarchy)
            {
                ServiceLocator.Get<PoolManager>().Release(gameObject);
            }
        }
    }


    void Start()
    {
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] PlayerController 인스턴스를 찾을 수 없습니다! 스크립트를 비활성화합니다.");
            this.enabled = false;
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
        // [디버그 1-1] 메서드가 시작되었고, 누구와 부딪혔는지 기록합니다.
        Debug.Log($"[디버그 1-1] OnTriggerEnter2D 시작. 충돌 대상: {other.name}");

        if (isDead)
        {
            Debug.LogWarning("[디버그] 몬스터가 이미 죽은 상태라 아무것도 처리하지 않음.");
            return;
        }

        if (other.TryGetComponent<BulletController>(out var hitBullet))
        {
            // [디버그 1-2] 부딪힌 것이 총알임을 확인했습니다.
            Debug.Log("[디버그 1-2] 총알 확인 완료.");

            if (hitShotIDs.Contains(hitBullet.shotInstanceID))
            {
                ServiceLocator.Get<PoolManager>().Release(other.gameObject);
                return;
            }

            hitShotIDs.Add(hitBullet.shotInstanceID);
            TakeDamage(hitBullet.damage);

            if (hitBullet.SourceCard != null && hitBullet.SourceCard.statusEffectToApply != null)
            {
                // [디버그 1-3] 총알이 적용할 상태 효과를 가지고 있음을 확인했습니다.
                Debug.Log($"[디버그 1-3] 총알이 적용할 상태 효과({hitBullet.SourceCard.statusEffectToApply.name})를 가지고 있음.");

                // [디버그 1-4] StatusEffectManager에게 효과 적용을 요청하기 직전입니다.
                Debug.Log("[디버그 1-4] StatusEffectManager에 효과 적용 요청 시작...");
                ServiceLocator.Get<StatusEffectManager>().ApplyStatusEffect(this.gameObject, hitBullet.SourceCard.statusEffectToApply);
            }

            if (hitBullet.SourceCard != null && hitBullet.SourceCard.secondaryEffect != null)
            {
                ServiceLocator.Get<EffectExecutor>().Execute(hitBullet.SourceCard.secondaryEffect, this.transform);
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
        if (isInvulnerable) return;
        currentHealth -= damage;

        Debug.Log($"[데미지] {name}: {damage} 피해. 현재 체력: {currentHealth}/{maxHealth}");

        // 기존의 데미지 텍스트 생성 코드를 삭제하고, 아래의 이벤트 발생 코드로 대체합니다.
        OnMonsterDamaged?.Invoke(damage, transform.position);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        // [수정] 몬스터가 여러 번 죽는 것을 방지하기 위해 isDead 플래그로 확인합니다.
        if (!isDead)
        {
            isDead = true;
            Debug.Log($"[{name}] 사망. OnMonsterDied 이벤트를 발생시킵니다.");
            OnMonsterDied?.Invoke(this);
        }
        else
        {
            // 이미 죽은 상태라면, 중복 처리하지 않고 함수를 종료합니다.
            return;
        }

        // [주석] 가이드에 폭발 로직이 있었으나, 현재 HandleExplosion() 함수가 없어 컴파일 오류가 발생하므로 주석 처리합니다.
        // [주석] 가이드에 폭발 로직이 있었으나, 현재 HandleExplosion() 함수가 없어 컴파일 오류가 발생하므로 주석 처리합니다.
        // if (monsterData != null && monsterData.behaviorType == MonsterBehaviorType.ExplodeOnDeath)
        // {
        //     // HandleExplosion();
        //     Debug.LogWarning($"[{name}] 폭발 로직(HandleExplosion)이 필요하지만, 함수가 정의되지 않아 호출을 생략합니다.");
        // }
        
        // [수정] PoolManager.Instance 대신 ServiceLocator를 통해 오브젝트 풀에 반환합니다.
        Debug.Log($"[{name}] 오브젝트 풀에 반환을 요청합니다.");
        ServiceLocator.Get<PoolManager>().Release(gameObject);
    }

    public void SetInvulnerable(float duration)
    {
        // [로그 추가] 이 함수가 언제, 누구에 의해 호출되는지 정확히 기록합니다.
        // StackTraceUtility.ExtractStackTrace()는 호출 스택을 문자열로 보여주어
        // 이 함수를 부른 코드가 무엇인지 역추적할 수 있게 해줍니다.

        StopCoroutine("InvulnerableRoutine");
        StartCoroutine(InvulnerableRoutine(duration));
    }



    private IEnumerator InvulnerableRoutine(float duration)
    {
        // [로그 추가 1] 코루틴이 시작되고, 무적 상태가 되는 시점을 기록합니다.
        isInvulnerable = true;

        yield return new WaitForSeconds(duration);

        // [로그 추가 2] 대기가 끝난 후, 무적 상태가 해제되는 시점을 기록합니다.
        isInvulnerable = false;
    }
}
