using UnityEngine;
using System.Collections;

/// <summary>
/// 개별 몬스터의 행동과 상태(체력, 이동, 죽음 등)를 관리합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MonsterController : MonoBehaviour
{
    [Header("능력치")]
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("참조")]
    [SerializeField] private GameObject monsterPrefab; // 이 몬스터의 원본 프리팹 (PoolManager.Release에 필요)
    private Transform playerTransform; // 추적할 플레이어의 위치

    private float currentHealth;
    private bool isInvulnerable = false; // 무적 상태 여부
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;

        // 플레이어를 찾아 추적 대상으로 설정합니다.
        // 성능을 위해 FindObjectOfType은 가급적 Start에서 한 번만 호출하는 것이 좋습니다.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("태그가 'Player'인 게임 오브젝트를 찾을 수 없습니다!");
            // 플레이어가 없으면 몬스터는 비활성화하거나 제자리에 머물도록 처리
            this.enabled = false;
        }
    }

    void FixedUpdate()
    {
        // 무적 상태이거나 플레이어가 없으면 움직이지 않습니다.
        if (isInvulnerable || playerTransform == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 플레이어를 향해 이동합니다.
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    /// <summary>
    /// 몬스터에게 피해를 입힙니다.
    /// </summary>
    /// <param name="damage">입힐 데미지 양</param>
    public void TakeDamage(float damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 지정된 시간 동안 몬스터를 무적 상태로 만듭니다.
    /// </summary>
    /// <param name="duration">무적 지속 시간(초)</param>
    public void SetInvulnerable(float duration)
    {
        StartCoroutine(InvulnerableRoutine(duration));
    }

    private IEnumerator InvulnerableRoutine(float duration)
    {
        isInvulnerable = true;
        // TODO: 무적 상태를 시각적으로 표시 (예: 깜빡임 효과)
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
        // TODO: 시각적 효과 제거
    }

    private void Die()
    {
        Debug.Log("몬스터가 죽었습니다.");

        // RoundManager에 킬 카운트 등록을 요청합니다.
        if (RoundManager.Instance != null)
        {
            RoundManager.Instance.RegisterKill();
        }

        PoolManager.Instance.Release(monsterPrefab, gameObject);

        // TODO: 죽음 이펙트(폭발 등)나 아이템 드랍 로직 추가
    }

    // 플레이어의 공격(총알 등)과 충돌했을 때 호출될 메서드
    void OnTriggerEnter2D(Collider2D other)
    {
        // "PlayerBullet" 태그를 가진 오브젝트와 충돌했는지 확인
        if (other.CompareTag("PlayerBullet"))
        {
            BulletController hitBullet = other.GetComponent<BulletController>();
            if (hitBullet != null)
            {
                TakeDamage(hitBullet.damage);
            }
            else
            {
                // 총알에 BulletController가 없는 경우를 대비한 기본 데미지
                TakeDamage(1);
            }

            // PoolManager를 통해 총알을 풀에 반환합니다.
            if (hitBullet != null && hitBullet.gameObject != null && hitBullet.bulletPrefab != null)
            {
                PoolManager.Instance.Release(hitBullet.bulletPrefab, hitBullet.gameObject);
            }
        }
    }
}
