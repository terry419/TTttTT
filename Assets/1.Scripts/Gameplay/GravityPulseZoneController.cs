// 파일 경로: Assets/1/Scripts/Gameplay/GravityPulseZoneController.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CircleCollider2D))]
public class GravityPulseZoneController : MonoBehaviour
{
    [Header("내부 참조")]
    [SerializeField] private Transform visualsTransform;

    private float lifeDuration;
    private float pullRadius;
    private float pullForce;
    private float damage;
    private float minPulseScaleRatio;
    private float effectTickInterval;

    private float lifeTimer;
    private float effectTickTimer;
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
        this.effectTickInterval = tickInterval > 0 ? tickInterval : 0.5f;

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

        float pulseProgress = Mathf.Clamp01(effectTickTimer / effectTickInterval);
        float targetDiameter = Mathf.Lerp(pullRadius * minPulseScaleRatio * 2f, pullRadius * 2f, pulseProgress);
        float requiredScale = targetDiameter / baseSpriteDiameter;

        if (visualsTransform != null)
        {
            visualsTransform.localScale = new Vector3(requiredScale, requiredScale, 1f);
        }

        if (effectTickTimer >= effectTickInterval)
        {
            ApplyPullForce();
            ApplyDamageInZone();
            effectTickTimer = 0f;
        }
    }

    private void ApplyPullForce()
    {
        if (pullForce <= 0) return;

        monstersInZone.RemoveWhere(rb => rb == null || !rb.gameObject.activeInHierarchy);


        foreach (var monsterRb in monstersInZone)
        {
            if (monsterRb.TryGetComponent<MonsterController>(out var monsterController))
            {
                Vector2 directionToCenter = (transform.position - monsterRb.transform.position).normalized;
                Vector2 newVelocity = directionToCenter * pullForce;
                monsterController.SetVelocity(newVelocity);

            }
        }
    }

    private void ApplyDamageInZone()
    {
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
                bool wasRemoved = monstersInZone.Remove(monsterRb);
                if (wasRemoved)
                {
                }
            }
        }
    }

    void OnDisable()
    {
        monstersInZone.Clear();
    }
}