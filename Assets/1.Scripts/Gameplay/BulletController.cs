using UnityEngine;
using System.Collections.Generic; // Added this line

/// <summary>
/// 총알의 행동(이동, 소멸)과 데이터(데미지)를 관리합니다.
/// </summary>
public class BulletController : MonoBehaviour
{
    private Vector2 _direction; // 총알의 이동 방향
    public float speed;       // 총알의 속도 (외부에서 접근 가능하도록 public으로 변경)
    public float damage;        // 총알의 데미지 (PlayerController가 설정해 줌)
    public string shotInstanceID; // [추가] 발사 인스턴스 고유 ID
    public float lifetime = 3f; // 총알의 최대 생존 시간
    public CardDataSO SourceCard { get; private set; } // [추가] 이 총알을 발사한 카드 데이터
    public ProjectileEffectSO SourceModule { get; private set; }

    public int _currentPierceCount;
    public HashSet<GameObject> _hitMonsters = new HashSet<GameObject>();

    private int _currentRicochetCount;
    private Rigidbody2D rb;

    private bool isTracking;
    private Transform trackingTarget;
    [Tooltip("초당 회전할 수 있는 각도입니다. 높을수록 추적 성능이 좋아집니다.")]
    private float turnSpeed = 200f;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    /// <summary>
    /// 총알을 초기화하고 발사합니다.
    /// </summary>
    /// <param name="direction">총알의 이동 방향 (정규화된 벡터)</param>
    /// <param name="initialSpeed">총알의 초기 속도</param>
    /// <param name="damage">총알이 줄 데미지</param>
    /// <param name="shotID">[추가] 발사 ID 설정</param>

    private CharacterStats casterStats; // [추가] 시전자 정보를 저장할 변수
    private int _bounceCountForPayload = 0; // [추가] 연쇄 효과 발동을 위한 튕김 횟수 카운터


    // --- 구버전 Initialize ---
    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, CardDataSO cardData, int pierceCount, CharacterStats caster)
    {
        this._direction = direction.normalized;
        this.speed = initialSpeed;
        this.damage = damage;
        this.shotInstanceID = shotID;
        this.SourceCard = cardData;
        this.SourceModule = null; 
        this._currentPierceCount = pierceCount;
        this._currentRicochetCount = 0;
        this.isTracking = false;
        this.trackingTarget = null;
        this._hitMonsters.Clear();
        this.casterStats = caster;
        this._bounceCountForPayload = 0;
        
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), lifetime);
    }
    // --- v8.0 Initialize ---
    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, ProjectileEffectSO module, CharacterStats caster)
    {
        this._direction = direction.normalized;
        this.speed = initialSpeed;
        this.damage = damage;
        this.shotInstanceID = shotID;
        this.SourceCard = null; 
        this.SourceModule = module;
        this._currentPierceCount = module.pierceCount;
        this._currentRicochetCount = module.ricochetCount;
        this.isTracking = module.isTracking;
        this.trackingTarget = null;
        this._hitMonsters.Clear();
        this.casterStats = caster;
        this._bounceCountForPayload = 0;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), lifetime);
    }
    private void FixedUpdate()
    {
        if (isTracking)
        {
            // 타겟이 없거나 비활성화 상태이면 새로운 타겟을 찾습니다.
            if (trackingTarget == null || !trackingTarget.gameObject.activeInHierarchy)
            {
                trackingTarget = TargetingSystem.FindTarget(TargetingType.Nearest, transform, _hitMonsters);
            }

            if (trackingTarget != null)
            {
                // 타겟을 향하는 방향 벡터를 계산합니다.
                Vector2 directionToTarget = (trackingTarget.position - transform.position).normalized;

                // 현재 방향에서 타겟 방향으로 부드럽게 회전합니다.
                float angle = Vector2.SignedAngle(_direction, directionToTarget);
                float turnAmount = Mathf.Clamp(angle, -turnSpeed * Time.fixedDeltaTime, turnSpeed * Time.fixedDeltaTime);

                _direction = Quaternion.Euler(0, 0, turnAmount) * _direction;
                transform.rotation = Quaternion.LookRotation(Vector3.forward, _direction);
            }
        }

        rb.velocity = _direction * speed;
    }

    private void Deactivate()
    {
        ServiceLocator.Get<PoolManager>().Release(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(Tags.Monster)) return;
        if (_hitMonsters.Contains(other.gameObject)) return;

        if (other.TryGetComponent<MonsterController>(out var monster))
        {
            if (SourceCard == null && SourceModule == null) return;

            monster.TakeDamage(this.damage);
            _hitMonsters.Add(other.gameObject);

            // ★★★ [핵심 추가] 연쇄 효과(Payload) 발동 처리 ★★★
            HandlePayloads(monster.transform);

            bool ricocheted = TryRicochet(monster.transform);
            if (ricocheted) return;

            bool pierced = TryPierce();
            if (pierced) return;

            Deactivate();
        }
    }

    private bool TryRicochet(Transform lastHitTransform)
    {
        if (_currentRicochetCount <= 0) return false;
        _currentRicochetCount--;
        _bounceCountForPayload++; // [추가] 튕길 때마다 카운터 증가

        var exclusions = new HashSet<GameObject>(_hitMonsters);
        if (SourceModule != null && !SourceModule.canRicochetToSameTarget)
        {
            exclusions.Add(lastHitTransform.gameObject);
        }

        Transform nextTarget = TargetingSystem.FindTarget(TargetingType.Nearest, transform, exclusions);

        if (nextTarget != null)
        {
            Debug.Log($"<color=yellow>[Ricochet]</color> {lastHitTransform.name} -> {nextTarget.name}");
            _direction = (nextTarget.position - transform.position).normalized;

            // 튕길 때 추적 타겟을 초기화하여, 다음 프레임에 새로운 가장 가까운 적을 다시 찾도록 합니다.
            if (isTracking)
            {
                trackingTarget = nextTarget;
            }
            return true;
        }

        return false;
    }

    private void HandlePayloads(Transform hitTarget)
    {
        if (SourceModule == null || SourceModule.sequentialPayloads == null) return;

        var effectExecutor = ServiceLocator.Get<EffectExecutor>();
        if (effectExecutor == null) return;

        // sequentialPayloads 리스트에서 현재 튕김 횟수와 일치하는 모든 효과를 찾습니다.
        foreach (var payload in SourceModule.sequentialPayloads)
        {
            if (payload.onBounceNumber == _bounceCountForPayload)
            {
                // 조건이 맞으면 EffectExecutor에게 연쇄 효과 실행을 요청합니다.
                effectExecutor.ExecuteChainedEffect(payload.effectToTrigger, this.casterStats, hitTarget);
            }
        }
    }

    private bool TryPierce()
    {
        if (_currentPierceCount <= 0) return false;
        _currentPierceCount--;
        Debug.Log($"<color=lightblue>[Pierce]</color> 관통! 남은 횟수: {_currentPierceCount}");
        return true;
    }
}