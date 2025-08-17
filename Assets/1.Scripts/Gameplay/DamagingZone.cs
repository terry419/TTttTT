using UnityEngine;
using System.Collections.Generic;

public class DamagingZone : MonoBehaviour
{


    // 변경사항 1: [수정] 단일 피해와 지속 피해를 위한 변수 분리
    [Header("데미지 설정")]
    public float singleHitDamage = 0f; // 파동 형태 공격 시 한 번만 줄 데미지
    public float damagePerTick = 10f;  // 장판 형태 공격 시 틱당 줄 데미지
    [Tooltip("장판 형태 공격 시 데미지를 주는 간격 (초). 파동으로 사용할 경우 100 이상을 입력하세요.")]
    public float tickInterval = 1.0f;  // 장판 형태 공격 시 데미지를 주는 간격 (초)
    public float duration = 5.0f;     // 장판/파동의 총 지속 시간 (초)

    [Header("파동/장판 확장 설정")]
    public float expansionSpeed = 1.0f;    // 초당 커지는 속도
    public float expansionDuration = 2.0f; // 총 지속 시간 중 확장하는 데 걸리는 시간

    // 변경사항 2: [추가] 이 장판/파동 인스턴스의 고유 ID (단일 피해 모드에서 사용)
    public string shotInstanceID;

    // 변경사항 3: [추가] 이 장판이 단일 피해 파동 모드인지 지속 피해 장판 모드인지 구분하는 플래그
    public bool isSingleHitWaveMode = true;

    // 내부 타이머 및 상태 변수
    private List<MonsterController> targets = new List<MonsterController>(); // 지속 피해 모드에서 사용
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
            initialColliderRadius = circleCollider.radius; // [추가] Awake에서 초기 반지름 저장
        }
    }

    void OnEnable()
    {
        // 변경사항 4: [수정] 오브젝트가 활성화될 때마다 모든 타이머와 상태를 초기화합니다.
        tickTimer = 0f;
        durationTimer = duration; // 총 지속 시간으로 초기화
        expansionTimer = 0f;
        transform.localScale = initialScale; // 스케일을 초기 값으로 리셋
        targets.Clear(); // 지속 피해 모드를 위해 타겟 리스트 초기화
        // shotInstanceID와 isSingleHitWaveMode는 Initialize에서 설정되므로 여기서 초기화하지 않습니다.
    }

    // 변경사항 5: [수정] 외부에서 모든 파라미터를 설정하는 Initialize 메서드
    public void Initialize(float singleHitDmg, float continuousDmgPerTick, float tickInt, float totalDur, float expSpeed, float expDur, bool isWave, string shotID)
    {
        this.singleHitDamage = singleHitDmg;
        this.damagePerTick = continuousDmgPerTick;
        this.tickInterval = tickInt;
        this.duration = totalDur; // 총 지속 시간 설정
        this.expansionSpeed = expSpeed;
        this.expansionDuration = expDur;
        this.isSingleHitWaveMode = isWave;
        this.shotInstanceID = shotID;

        // Initialize 호출 시 OnEnable 로직을 다시 실행하여 상태를 초기화
        OnEnable();
    }

    void Update()
    {
        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Release(gameObject);
            else Destroy(gameObject);
            return;
        }
        // 변경사항 6: [수정] 확장 로직 (expansionDuration까지만 확장)
        if (expansionTimer < expansionDuration)
        {
            expansionTimer += Time.deltaTime;
            transform.localScale += Vector3.one * expansionSpeed * Time.deltaTime;
        }

        // 변경사항 7: [수정] 총 지속 시간 로직
        if (durationTimer <= 0)
        {
            if (PoolManager.Instance != null) PoolManager.Instance.Release(gameObject);
            else Destroy(gameObject);
            return;
        }

        // 변경사항 8: [수정] 지속 피해 모드일 때만 틱 데미지 로직 실행
        if (!isSingleHitWaveMode) // 단일 피해 파동 모드가 아닐 때 (즉, 지속 피해 장판 모드일 때)
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                ApplyDamageToTargets();
            }
        }
    }

    // 변경사항 9: [수정] OnTriggerEnter2D 로직 분기
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster == null) return;

            if (isSingleHitWaveMode) // 단일 피해 파동 모드
            {
                if (!monster.hitShotIDs.Contains(this.shotInstanceID))
                {
                    monster.hitShotIDs.Add(this.shotInstanceID);
                    monster.TakeDamage(this.singleHitDamage);
                }
            }
            else // 지속 피해 장판 모드
            {
                if (!targets.Contains(monster))
                {
                    targets.Add(monster);
                }
            }
        }
    }

    // 변경사항 10: [수정] OnTriggerExit2D 로직 분기
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster == null) return;

            if (!isSingleHitWaveMode) // 지속 피해 장판 모드일 때만 타겟 리스트에서 제거
            {
                if (targets.Contains(monster))
                {
                    targets.Remove(monster);
                }
            }
        }
    }

    // 변경사항 11: [추가] 지속 피해 모드에서 사용될 ApplyDamageToTargets 메서드
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
