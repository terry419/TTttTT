// 파일 경로: Assets/1.Scripts/Gameplay/GravityPulseZoneController.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CircleCollider2D))]
public class GravityPulseZoneController : MonoBehaviour
{
    [Header("내부 참조")]
    [SerializeField] private Transform visualsTransform;

    // --- 모듈로부터 초기화될 값 ---
    private float lifeDuration;
    private float pullRadius;
    private float pullForce;
    private float damage;
    private float minPulseScaleRatio;
    private float effectTickInterval; // 이제 이 변수가 모든 효과의 주기를 제어합니다.

    // --- 내부 상태 변수 ---
    private float lifeTimer;
    private float effectTickTimer; // damageTickTimer에서 이름 변경
    private CircleCollider2D zoneCollider;
    private HashSet<Rigidbody2D> monstersInZone = new HashSet<Rigidbody2D>();
    private float baseSpriteDiameter = 1f;

    void Awake()
    {
        zoneCollider = GetComponent<CircleCollider2D>();
        zoneCollider.isTrigger = true;

        if (visualsTransform != null)
        {
            SpriteRenderer visualsRenderer = visualsTransform.GetComponent<SpriteRenderer>();
            if (visualsRenderer != null && visualsRenderer.sprite != null)
            {
                baseSpriteDiameter = visualsRenderer.sprite.bounds.size.x;
                if (baseSpriteDiameter == 0) baseSpriteDiameter = 1f;
            }
        }
    }

    public void Initialize(float duration, float radius, float force, float dmg, float minScale, float tickInterval)
    {
        this.lifeDuration = duration;
        this.pullRadius = radius;
        this.pullForce = force;
        this.damage = dmg;
        this.minPulseScaleRatio = Mathf.Clamp01(minScale);
        this.effectTickInterval = tickInterval > 0 ? tickInterval : 0.5f; // 0 이하 값 방지

        lifeTimer = 0f;
        effectTickTimer = 0f;
        zoneCollider.radius = pullRadius;
        monstersInZone.Clear();
        gameObject.SetActive(true);
    }

    void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeDuration)
        {
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
            return;
        }

        effectTickTimer += Time.deltaTime;

        // 타이머의 진행률(0.0 ~ 1.0)에 따라 크기가 선형적으로 커집니다.
        float pulseProgress = Mathf.Clamp01(effectTickTimer / effectTickInterval);
        float targetDiameter = Mathf.Lerp(pullRadius * minPulseScaleRatio * 2f, pullRadius * 2f, pulseProgress);
        float requiredScale = targetDiameter / baseSpriteDiameter;

        if (visualsTransform != null)
        {
            visualsTransform.localScale = new Vector3(requiredScale, requiredScale, 1f);
        }

        if (effectTickTimer >= effectTickInterval)
        {
            ApplyPullForce();      // 끌어당기는 힘 적용
            ApplyDamageInZone();   // 피해 적용
            effectTickTimer = 0f;  // 타이머 초기화
        }
    }


    private void ApplyPullForce()
    {
        if (pullForce <= 0) return;

        monstersInZone.RemoveWhere(rb => rb == null || !rb.gameObject.activeInHierarchy);

        foreach (var monsterRb in monstersInZone)
        {
            // 중앙으로 순간적으로 강하게 끌어당기는 느낌을 주기 위해 ForceMode2D.Impulse 사용
            Vector2 directionToCenter = (transform.position - monsterRb.transform.position).normalized;
            monsterRb.AddForce(directionToCenter * pullForce * 0.1f, ForceMode2D.Impulse); // 값을 조정하여 원하는 강도를 찾으세요
        }
    }

    private void ApplyDamageInZone()
    {
        // 피해는 항상 최대 반경에서 적용됩니다.
        if (damage <= 0) return;

        foreach (var monsterRb in monstersInZone.ToList())
        {
            if (monsterRb == null || !monsterRb.gameObject.activeInHierarchy) continue;

            if (monsterRb.TryGetComponent<MonsterController>(out var monster))
            {
                monster.TakeDamage(damage);
            }
        }
    }

    // OnTriggerEnter2D, OnTriggerExit2D, OnDisable 메서드는 변경 없습니다.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Monster))
        {
            if (other.TryGetComponent<Rigidbody2D>(out var monsterRb))
            {
                monstersInZone.Add(monsterRb);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Tags.Monster))
        {
            if (other.TryGetComponent<Rigidbody2D>(out var monsterRb))
            {
                monstersInZone.Remove(monsterRb);
            }
        }
    }

    void OnDisable()
    {
        monstersInZone.Clear();
    }
}