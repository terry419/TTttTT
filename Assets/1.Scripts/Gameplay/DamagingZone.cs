using UnityEngine;
using System.Collections.Generic;

public class DamagingZone : MonoBehaviour
{
    [Header(" ")]
    public float singleHitDamage = 0f;
    public float damagePerTick = 10f;
    [Tooltip("     ִ  (). ĵ   100 ̻ Էϼ.")]
    public float tickInterval = 1.0f;
    public float duration = 5.0f;

    [Header("ĵ/ Ȯ ")]
    public float expansionSpeed = 1.0f;
    public float expansionDuration = 2.0f;

    public string shotInstanceID;
    public bool isSingleHitWaveMode = true;

    private List<MonsterController> targets = new List<MonsterController>();
    private float tickTimer;
    private float durationTimer;
    private float expansionTimer;
    private CircleCollider2D circleCollider;
    private Vector3 initialScale;
    private float initialColliderRadius;

    void Awake()
    {
        initialScale = transform.localScale;
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            initialColliderRadius = circleCollider.radius;
        }
    }

    void OnEnable()
    {
        tickTimer = 0f;
        durationTimer = duration;
        expansionTimer = 0f;
        transform.localScale = initialScale;
        targets.Clear();
    }

    public void Initialize(float singleHitDmg, float continuousDmgPerTick, float tickInt, float totalDur, float expSpeed, float expDur, bool isWave, string shotID)
    {
        this.singleHitDamage = singleHitDmg;
        this.damagePerTick = continuousDmgPerTick;
        this.tickInterval = tickInt;
        this.duration = totalDur;
        this.expansionSpeed = expSpeed;
        this.expansionDuration = expDur;
        this.isSingleHitWaveMode = isWave;
        this.shotInstanceID = shotID;
        OnEnable();
    }

    void Update()
    {
        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0)
        {
            // --- [] PoolManager ȣ ڵ带 ϳ մϴ. ---
            var poolManager = ServiceLocator.Get<PoolManager>();
            if (poolManager != null)
                poolManager.Release(gameObject);
            else
                Destroy(gameObject);
            return;
        }

        if (expansionTimer < expansionDuration)
        {
            expansionTimer += Time.deltaTime;
            transform.localScale += Vector3.one * expansionSpeed * Time.deltaTime;
        }

        if (!isSingleHitWaveMode)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                ApplyDamageToTargets();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster == null) return;

            if (isSingleHitWaveMode)
            {
                if (!monster.hitShotIDs.Contains(this.shotInstanceID))
                {
                    monster.hitShotIDs.Add(this.shotInstanceID);
                    monster.TakeDamage(this.singleHitDamage);
                }
            }
            else
            {
                if (!targets.Contains(monster))
                {
                    targets.Add(monster);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster == null) return;

            if (!isSingleHitWaveMode)
            {
                if (targets.Contains(monster))
                {
                    targets.Remove(monster);
                }
            }
        }
    }

    private void ApplyDamageToTargets()
    {
        List<MonsterController> currentTargets = new List<MonsterController>(targets);
        foreach (var monster in currentTargets)
        {
            if (monster != null && monster.gameObject.activeInHierarchy)
            {
                monster.TakeDamage(damagePerTick);
            }
        }
    }
}