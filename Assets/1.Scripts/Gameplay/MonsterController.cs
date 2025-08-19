// --- 파일명: MonsterController.cs (최종 수정본) ---
// 경로: Assets/1.Scripts/Gameplay/MonsterController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic; // 변경사항 1: [추가] HashSet을 사용하기 위해 추가
using System.Linq; // 변경사항 2: [추가] Linq 사용을 위해 추가 (선택 사항)

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterController : MonoBehaviour
{
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

    // 변경사항 3: [추가] 이미 피해를 입은 shotInstanceID를 저장하는 HashSet
    public HashSet<string> hitShotIDs = new HashSet<string>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 변경사항 4: [추가] 오브젝트가 활성화될 때마다 hitShotIDs를 초기화 (풀링된 오브젝트를 위해)
    void OnEnable()
    {
        hitShotIDs.Clear();
    }


    void Start()
    {
        // PlayerController는 Gameplay 씬에만 존재하므로, Start에서 찾아야 안전합니다.
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
        // 변경사항 5: [추가] 몬스터가 초기화될 때 hitShotIDs도 초기화
        hitShotIDs.Clear();

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
        if (isInvulnerable || playerTransform == null)
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
        Debug.Log($"[MonsterController] OnTriggerEnter2D 호출됨. 충돌한 오브젝트: {other.name}, 태그: {other.tag}");

        BulletController hitBullet = other.GetComponent<BulletController>();
        if (hitBullet != null)
        {
            // 변경사항 6: [추가] 동일한 shotInstanceID에 대해 한 번만 데미지 적용
            if (hitShotIDs.Contains(hitBullet.shotInstanceID))
            {
                PoolManager.Instance.Release(other.gameObject); // 총알은 풀로 반환
                return; // 이미 맞은 총알이므로 데미지 적용 안 함
            }

            TakeDamage(hitBullet.damage);

            if (hitBullet.SourceCard != null && hitBullet.SourceCard.secondaryEffect != null)
            {
                // EffectExecutor의 새 기능을 호출!
                // 2차 효과 카드(secondaryEffect)를 이 몬스터의 위치(this.transform)에서 발동
                EffectExecutor.Instance.Execute(hitBullet.SourceCard.secondaryEffect, this.transform);
            }
            PoolManager.Instance.Release(other.gameObject);
            return;
        }
        CheckForPlayer(other.gameObject);
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

        // [수정] DataManager가 아닌 PrefabProvider를 통해 데미지 텍스트 프리팹을 가져옵니다.
        if (PrefabProvider.Instance != null)
        {
            GameObject damageTextPrefab = PrefabProvider.Instance.GetPrefab("DamageTextCanvas");
            if (damageTextPrefab != null)
            {
                GameObject textGO = PoolManager.Instance.Get(damageTextPrefab);
                textGO.transform.position = transform.position + Vector3.up * 0.5f;
                textGO.GetComponent<DamageText>().ShowDamage(damage);
            }
        }

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (RoundManager.Instance != null)
        {
            Debug.Log($"<color=cyan>[가설 검증] 몬스터(ID:{GetInstanceID()})가 RoundManager(ID:{RoundManager.Instance.GetInstanceID()})에게 RegisterKill 호출을 시도합니다.</color>");
            RoundManager.Instance.RegisterKill();
        }
        else
        {
            Debug.LogError("[가설 검증] 몬스터가 RegisterKill을 호출하려 했으나 RoundManager.Instance가 null입니다!");
        }
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