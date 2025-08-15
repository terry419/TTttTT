// --- 파일명: MonsterController.cs (최종 수정본) ---
// 경로: Assets/1.Scripts/Gameplay/MonsterController.cs
using UnityEngine;
using System.Collections;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        BulletController hitBullet = other.GetComponent<BulletController>();
        if (hitBullet != null)
        {
            TakeDamage(hitBullet.damage);
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
        if (RoundManager.Instance != null) RoundManager.Instance.RegisterKill();
        PoolManager.Instance.Release(gameObject);
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