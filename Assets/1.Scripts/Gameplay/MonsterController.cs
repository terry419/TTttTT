// --- 파일명: MonsterController.cs ---

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class MonsterController : MonoBehaviour
{
    [Header("능력치")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 3f;

    public float currentHealth;
    private Transform playerTransform;
    private bool isInvulnerable = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else
        {
            Debug.LogError("PlayerController 인스턴스를 찾을 수 없습니다!");
            this.enabled = false;
        }
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
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

    public void TakeDamage(float damage)
    {
        // [디버그 로그 추가]
        Debug.Log($"[MonsterController] {gameObject.name}이(가) 데미지를 받음: {damage}. 현재 체력: {currentHealth - damage}");

        if (isInvulnerable) return;
        currentHealth -= damage;

        GameObject damageTextPrefab = DataManager.Instance.GetVfxPrefab("DamageTextCanvas");
        if (damageTextPrefab != null)
        {
            GameObject textGO = PoolManager.Instance.Get(damageTextPrefab);
            textGO.transform.position = transform.position + Vector3.up * 0.5f;
            textGO.GetComponent<DamageText>().ShowDamage(damage);
            // [디버그 로그 추가]
            Debug.Log("[MonsterController] 데미지 텍스트 생성 성공!");
        }
        else
        {
            // [디버그 로그 추가]
            Debug.LogWarning("[MonsterController] DamageTextCanvas 프리팹을 DataManager에서 찾을 수 없음!");
        }

        if (currentHealth <= 0) Die();
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

    private void Die()
    {
        // [디버그 로그 추가]
        Debug.Log($"[MonsterController] {gameObject.name} 사망!");
        if (RoundManager.Instance != null) RoundManager.Instance.RegisterKill();
        PoolManager.Instance.Release(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // [디버그 로그 추가]
        Debug.Log($"[MonsterController] {gameObject.name}에 무언가 충돌함: {other.name} (태그: {other.tag})");

        if (other.CompareTag("PlayerBullet"))
        {
            // [디버그 로그 추가]
            Debug.Log("[MonsterController] 'PlayerBullet' 태그를 가진 오브젝트와 충돌!");

            BulletController hitBullet = other.GetComponent<BulletController>();
            if (hitBullet != null)
            {
                TakeDamage(hitBullet.damage);
                PoolManager.Instance.Release(other.gameObject);
            }
        }
    }
}