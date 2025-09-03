//  : Assets/1.Scripts/Gameplay/GravityPulseZoneController.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CircleCollider2D))]
public class GravityPulseZoneController : MonoBehaviour
{
    [Header(" ")]
    [SerializeField] private Transform visualsTransform; // ũⰡ  ð ȿ Ʈ

    // --- κ ʱȭ  ---
    private float lifeDuration; //   ӽð
    private float pullRadius; // ͸  ִ ݰ
    private float pullForce; // ͸  
    private float damage; // ط
    private float pulseSpeed; // Ŀ ۾ ӵ
    private float minPulseScaleRatio; // ּ ũ  (0.0 ~ 1.0)
    private float damageTickInterval; // ظ ִ ֱ

    // ---    ---
    private float lifeTimer;
    private float damageTickTimer;
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
                // sprite.bounds.size는 월드 유닛 기준의 크기를 반환합니다.
                baseSpriteDiameter = visualsRenderer.sprite.bounds.size.x;
                if (baseSpriteDiameter == 0) baseSpriteDiameter = 1f; // 0일 경우의 오류 방지
            }
        }
    }

    public void Initialize(float duration, float radius, float force, float dmg, float speed, float minScale, float tickInterval)
    {
        this.lifeDuration = duration;
        this.pullRadius = radius;
        this.pullForce = force;
        this.damage = dmg;
        this.pulseSpeed = speed;
        this.minPulseScaleRatio = Mathf.Clamp01(minScale);
        this.damageTickInterval = tickInterval;

        // ʱȭ
        lifeTimer = 0f;
        damageTickTimer = 0f;
        zoneCollider.radius = pullRadius;
        monstersInZone.Clear();
        gameObject.SetActive(true);
    }

    void Update()
    {
        // 생명주기 관리
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeDuration)
        {
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
            return;
        }


        // 1. visualsTransform 참조 확인
        if (visualsTransform == null)
        {
            // 이 로그가 계속 출력된다면, 프리팹 Inspector에서 참조 연결이 안 된 것입니다.
            Debug.LogError("[중력장 디버그] CRITICAL: visualsTransform 참조가 비어있습니다! Inspector를 확인하세요.");
            return; // 참조가 없으면 더 이상 진행하지 않음
        }

        // 2. 맥동(크기 변화) 로직
        float pulseWave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float targetDiameter = Mathf.Lerp(pullRadius * minPulseScaleRatio, pullRadius, pulseWave) * 2f;
        float requiredScale = targetDiameter / baseSpriteDiameter;


        if (visualsTransform != null)
        {
            visualsTransform.localScale = new Vector3(requiredScale, requiredScale, 1f);
        }



        // 피해 주기 타이머
        damageTickTimer += Time.deltaTime;
        if (damageTickTimer >= damageTickInterval)
        {
            ApplyDamageInZone();
            damageTickTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        //  ȿ FixedUpdate ó
        ApplyPullForce();
    }

    private void ApplyPullForce()
    {
        if (pullForce <= 0) return;

        // HashSet Ͽ ȸ    
        foreach (var monsterRb in new HashSet<Rigidbody2D>(monstersInZone))
        {
            if (monsterRb == null || !monsterRb.gameObject.activeInHierarchy)
            {
                monstersInZone.Remove(monsterRb);
                continue;
            }
            Vector2 directionToCenter = (transform.position - monsterRb.transform.position).normalized;
            monsterRb.AddForce(directionToCenter * pullForce);
        }
    }

    private void ApplyDamageInZone()
    {
        //  ð ȿ ũ⸦   ݰ 
        float currentDamageRadius = (visualsTransform != null ? (visualsTransform.localScale.x * baseSpriteDiameter) / 2f : 0);
        if (damage <= 0 || currentDamageRadius <= 0) return;

        foreach (var monsterRb in monstersInZone)
        {
            if (monsterRb == null || !monsterRb.gameObject.activeInHierarchy)
            {
                monstersInZone.Remove(monsterRb);
                continue;
            }

            // Ͱ   ݰ  ִ Ȯ
            if (Vector2.Distance(transform.position, monsterRb.transform.position) <= currentDamageRadius)
            {
                if (monsterRb.TryGetComponent<MonsterController>(out var monster))
                {
                    monster.TakeDamage(damage);
                }
            }
        }
    }


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
        // Ǯ ư  ʱȭ
        monstersInZone.Clear();
    }
}