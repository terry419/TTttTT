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

    // 모든 오브젝트의 Awake()가 끝난 후에 Start()가 호출되므로,
    // 이 시점에는 PlayerController.Instance가 확실히 존재합니다.
    void Start()
    {
        currentHealth = maxHealth;
        if (PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }
        else
        {
            Debug.LogError("PlayerController 인스턴스를 찾을 수 없습니다! Hierarchy에 Player 오브젝트가 있는지 확인하세요.");
            this.enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (isInvulnerable || playerTransform == null) { rb.velocity = Vector2.zero; return; }
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;
        currentHealth -= damage;

        GameObject damageTextPrefab = DataManager.Instance.GetEffectPrefab("DamageText");
        if (damageTextPrefab != null)
        {
            GameObject textGO = PoolManager.Instance.Get(damageTextPrefab);
            // DamageText.cs 스크립트가 준비되면 아래 줄의 주석을 해제하세요.
            // textGO.GetComponent<DamageText>().ShowDamage((int)damage, transform.position);
        }

        if (currentHealth <= 0) Die();
    }

    public void SetInvulnerable(float duration) { StartCoroutine(InvulnerableRoutine(duration)); }

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
            if (hitBullet != null) TakeDamage(hitBullet.damage);
            PoolManager.Instance.Release(other.gameObject);
        }
    }
}