using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    private Vector2 _direction;
    public float speed;
    public float damage;
    public string shotInstanceID;
    public float lifetime = 3f;
    // public CardDataSO SourceCard { get; private set; } // 이 줄 삭제
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
    private NewCardDataSO sourcePlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // [수정] NewCardDataSO를 사용하는 Initialize 메소드만 남깁니다.
    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, NewCardDataSO platform, ProjectileEffectSO module, CharacterStats caster)
    {
        this._direction = direction.normalized;
        this.speed = initialSpeed;
        this.damage = damage;
        this.shotInstanceID = shotID;
        // this.SourceCard = null; // 이 줄 삭제
        this.SourceModule = module;
        this.sourcePlatform = platform;
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
        if (!other.CompareTag(Tags.Monster) || _hitMonsters.Contains(other.gameObject)) return;
        if (other.TryGetComponent<MonsterController>(out var monster))
        {
            // [추가] 샷건 다중 히트 방지 로직
            // ProjectileEffectSO에 제어 변수를 추가할 수 있지만, 지금은 기본적으로 1회만 히트하도록 합니다.
            if (!monster.RegisterHitByShot(this.shotInstanceID, sourcePlatform.allowMultipleHits))
            {
                // 피해 없이 관통/튕김 로직만 처리
            }
            else
            {
                // 처음 맞는 경우에만 데미지를 줍니다.
                monster.TakeDamage(this.damage);
                _hitMonsters.Add(other.gameObject);
                _ = HandlePayloads_Async(monster.transform);
            }

            if (TryRicochet(monster.transform)) return;
            if (TryPierce()) return;

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
            _direction = (nextTarget.position - transform.position).normalized;
            if (isTracking) trackingTarget = nextTarget;
            return true;
        }
        return false;
    }

    private async UniTaskVoid HandlePayloads_Async(Transform hitTarget)
    {
        if (SourceModule?.sequentialPayloads == null) return;
        var resourceManager = ServiceLocator.Get<ResourceManager>();

        foreach (var payload in SourceModule.sequentialPayloads)
        {
            if (payload.onBounceNumber == _bounceCountForPayload)
            {
                CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(payload.effectToTrigger.AssetGUID);
                if (module == null) continue;

                var context = new EffectContext
                {
                    Caster = this.casterStats,
                    SpawnPoint = hitTarget,
                    Platform = this.sourcePlatform,
                    HitTarget = hitTarget.GetComponent<MonsterController>(),
                    HitPosition = hitTarget.position
                };
                module.Execute(context);
            }
        }
    }

    private bool TryPierce()
    {
        if (_currentPierceCount <= 0) return false;
        _currentPierceCount--;
        return true;
    }
}