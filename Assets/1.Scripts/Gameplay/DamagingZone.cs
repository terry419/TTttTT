using UnityEngine;
using System.Collections.Generic;

public class DamagingZone : MonoBehaviour
{
    // --- 외부 설정값 ---
    private float singleHitDamage;
    private float damagePerTick;
    private float tickInterval;
    private float duration;
    private float expansionDuration;
    private float maxExpansionRadius;
    private bool isSingleHitWaveMode;
    private string shotInstanceID;

    // --- 내부 상태 변수 ---
    private HashSet<MonsterController> targetsInZone = new HashSet<MonsterController>();
    private HashSet<MonsterController> hitByWave = new HashSet<MonsterController>(); // 파동 모드에서 한 번만 맞도록 체크
    private float tickTimer;
    private float durationTimer;
    private float expansionTimer;
    private CircleCollider2D circleCollider;
    private float initialColliderRadius;

    void Awake()
    {
        Debug.Log($"[DamagingZone-DEBUG A] Awake 호출됨. 오브젝트: '{this.gameObject.name}'");
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            initialColliderRadius = circleCollider.radius;
        }
    }

    public void Initialize(float singleHitDmg, float continuousDmgPerTick, float tickInt, float totalDur, float maxRadius, float expDur, bool isWave, string shotID)
    {
        Debug.Log($"[DamagingZone-DEBUG B] Initialize 호출됨. 오브젝트: '{this.gameObject.name}'. SingleHitDmg: {singleHitDmg}, Duration: {totalDur}");

        this.singleHitDamage = singleHitDmg;
        this.damagePerTick = continuousDmgPerTick;
        this.tickInterval = tickInt;
        this.duration = totalDur;
        this.maxExpansionRadius = maxRadius;
        this.expansionDuration = expDur;
        this.isSingleHitWaveMode = isWave;
        this.shotInstanceID = shotID;

        // OnEnable 로직을 여기서 수동 호출하여 초기화
        durationTimer = duration;
        expansionTimer = 0f;
        tickTimer = 0f;
        targetsInZone.Clear();
        hitByWave.Clear();

        if (circleCollider != null)
        {
            circleCollider.radius = initialColliderRadius;
        }

        // [피드백 반영] 파동 모드일 때, 생성 즉시 범위 내 몬스터에게 피해를 줍니다.
        if (isSingleHitWaveMode)
        {
            ApplyInitialWaveDamage();
        }
    }

    void Update()
    {
        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0)
        {
            Debug.Log($"[DamagingZone-DEBUG C] 수명이 다했습니다. PoolManager.Release를 시도합니다. 오브젝트: '{this.gameObject.name}'");
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
            return;
        }

        if (expansionTimer < expansionDuration && expansionDuration > 0)
        {
            expansionTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(expansionTimer / expansionDuration);
            float newRadius = Mathf.Lerp(initialColliderRadius, maxExpansionRadius, progress);
            if (circleCollider != null)
            {
                circleCollider.radius = newRadius;
            }
        }

        if (!isSingleHitWaveMode)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                ApplyPeriodicDamageToTargets();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster") && other.TryGetComponent<MonsterController>(out var monster))
        {
            if (isSingleHitWaveMode)
            {
                if (!hitByWave.Contains(monster))
                {
                    monster.TakeDamage(this.singleHitDamage);
                    hitByWave.Add(monster);
                }
            }
            else // 장판 모드
            {
                if (!targetsInZone.Contains(monster))
                {
                    // [피드백 반영] 장판에 진입 즉시 첫 틱 피해를 입혀 피해 누락을 방지합니다.
                    monster.TakeDamage(this.damagePerTick);
                    targetsInZone.Add(monster);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isSingleHitWaveMode && other.CompareTag("Monster") && other.TryGetComponent<MonsterController>(out var monster))
        {
            targetsInZone.Remove(monster);
        }
    }

    // 장판의 주기적인 피해를 처리하는 함수
    private void ApplyPeriodicDamageToTargets()
    {
        var currentTargets = new List<MonsterController>(targetsInZone);
        foreach (var monster in currentTargets)
        {
            if (monster != null && monster.gameObject.activeInHierarchy)
            {
                monster.TakeDamage(damagePerTick);
            }
            else
            {
                targetsInZone.Remove(monster);
            }
        }
    }

    // [피드백 반영] 파동 생성 시점에 즉시 피해를 주는 함수
    private void ApplyInitialWaveDamage()
    {
        if (circleCollider == null) return;

        // 현재 콜라이더의 실제 반지름을 가져옵니다. (월드 스케일 고려)
        float currentWorldRadius = circleCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);

        Collider2D[] initialHits = Physics2D.OverlapCircleAll(transform.position, currentWorldRadius);
        foreach (var col in initialHits)
        {
            if (col.CompareTag("Monster") && col.TryGetComponent<MonsterController>(out var monster))
            {
                if (!hitByWave.Contains(monster))
                {
                    monster.TakeDamage(this.singleHitDamage);
                    hitByWave.Add(monster);
                }
            }
        }
    }
}