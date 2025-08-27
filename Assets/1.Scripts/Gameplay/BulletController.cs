// 경로: ./TTttTT/Assets/1.Scripts/Gameplay/BulletController.cs

using UnityEngine;
using System.Collections.Generic;

public class BulletController : MonoBehaviour
{
    private Vector2 _direction;
    public float speed;
    public float damage;
    public string shotInstanceID;
    public float lifetime = 3f;
    public CardDataSO SourceCard { get; private set; }
    public ProjectileEffectSO SourceModule { get; private set; }

    public int _currentPierceCount;
    public HashSet<GameObject> _hitMonsters = new HashSet<GameObject>();

    private int _currentRicochetCount;
    private Rigidbody2D rb;
    private bool isTracking;
    private Transform trackingTarget;
    private float turnSpeed = 200f;
    private CharacterStats casterStats;
    private int _bounceCountForPayload = 0;

    // [추가] 이 총알을 발사한 원본 카드(플랫폼) 정보
    private NewCardDataSO sourcePlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // --- 구버전 Initialize ---
    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, CardDataSO cardData, int pierceCount, CharacterStats caster)
    {
        this._direction = direction.normalized;
        this.speed = initialSpeed;
        this.damage = damage;
        this.shotInstanceID = shotID;
        this.SourceCard = cardData;
        this.SourceModule = null;
        this.sourcePlatform = null; // 구버전에서는 플랫폼 없음
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

    // --- v8.0 Initialize (수정) ---
    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, NewCardDataSO platform, ProjectileEffectSO module, CharacterStats caster)
    {
        this._direction = direction.normalized;
        this.speed = initialSpeed;
        this.damage = damage;
        this.shotInstanceID = shotID;
        this.SourceCard = null;
        this.SourceModule = module;
        this.sourcePlatform = platform; // [추가] 플랫폼 정보 저장
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
            if (trackingTarget == null || !trackingTarget.gameObject.activeInHierarchy)
            {
                trackingTarget = TargetingSystem.FindTarget(TargetingType.Nearest, transform, _hitMonsters);
            }

            if (trackingTarget != null)
            {
                Vector2 directionToTarget = (trackingTarget.position - transform.position).normalized;
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
        ServiceLocator.Get<PoolManager>()?.Release(gameObject);
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
        _bounceCountForPayload++;

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

        foreach (var payload in SourceModule.sequentialPayloads)
        {
            if (payload.onBounceNumber == _bounceCountForPayload)
            {
                // [수정] 연쇄 효과 실행 시 원본 카드(sourcePlatform) 정보를 전달
                effectExecutor.ExecuteChainedEffect(payload.effectToTrigger, this.casterStats, hitTarget, this.sourcePlatform);
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