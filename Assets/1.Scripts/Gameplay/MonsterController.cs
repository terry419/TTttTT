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
        // 오브젝트가 풀에서 활성화될 때 체력을 리셋합니다.
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
        if (isInvulnerable) return;
        currentHealth -= damage;

        // --- 데미지 텍스트 생성 로직 ---
        GameObject damageTextPrefab = DataManager.Instance.GetVfxPrefab("DamageTextCanvas");
        if (damageTextPrefab != null)
        {
            GameObject textGO = PoolManager.Instance.Get(damageTextPrefab);
            textGO.transform.position = transform.position + Vector3.up * 0.5f; // 몬스터 머리 위에서 시작
            textGO.GetComponent<DamageText>().ShowDamage(damage);
        }
        // ------------------------------------

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
        if (RoundManager.Instance != null) RoundManager.Instance.RegisterKill();
        PoolManager.Instance.Release(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            BulletController hitBullet = other.GetComponent<BulletController>();
            if (hitBullet != null) 
            {
                TakeDamage(hitBullet.damage);
                // 총알은 데미지를 입히고 풀로 돌아갑니다.
                PoolManager.Instance.Release(other.gameObject);
            }
        }
    }
}
