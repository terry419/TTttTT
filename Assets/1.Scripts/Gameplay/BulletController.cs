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
    private CardInstance sourceCardInstance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // [추가] 총알 렌더링 순서 설정
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 10;
        }
    }

    // [수정] NewCardDataSO를 사용하는 Initialize 메소드만 남깁니다.
    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, NewCardDataSO platform, ProjectileEffectSO module, CharacterStats caster, CardInstance instance)
    {
        this._direction = direction.normalized;
        this.speed = initialSpeed;
        this.damage = damage;
        this.shotInstanceID = shotID;
        // this.SourceCard = null; // 이 줄 삭제
        this.SourceModule = module;
        this.sourcePlatform = platform;
        this.sourceCardInstance = instance;
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
        if (other == null || other.gameObject == null)
        {
            return;
        }
        if (!other.CompareTag(Tags.Monster) || _hitMonsters.Contains(other.gameObject)) return;
        if (other.TryGetComponent<MonsterController>(out var monster))
        {
            if (monster == null)
            {
                Debug.LogError("[BULLET DEBUG] Stage 1: monster 객체가 이 시점에서 null(파괴됨)입니다.");
                return;
            }

            bool canHitMultiple = (sourcePlatform != null) ? sourcePlatform.allowMultipleHits : false;


            // [추가] 샷건 다중 히트 방지 로직
            // ProjectileEffectSO에 제어 변수를 추가할 수 있지만, 지금은 기본적으로 1회만 히트하도록 합니다.
            if (!monster.RegisterHitByShot(this.shotInstanceID, sourcePlatform.allowMultipleHits))
            {
                // 피해 없이 관통/튕김 로직만 처리
            }
            else
            {
                monster.TakeDamage(this.damage);
                _hitMonsters.Add(other.gameObject);

                // 2단계: monster.transform을 사용하기 직전에 확인합니다.
                if (monster.transform == null)
                {
                    Debug.LogError("[BULLET DEBUG] Stage 2: monster.transform이 null입니다! HandlePayloads_Async 호출 직전 오류 발생.");
                    return;
                }
                _ = HandlePayloads_Async(monster.transform);
            }
            if (monster.transform == null)
            {
                Debug.LogError("[BULLET DEBUG] Stage 3: monster.transform이 null입니다! TryRicochet 호출 직전 오류 발생.");
                return;
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
        Debug.Log($"[DEBUG-BULLET] HandlePayloads_Async 진입. 대상: {hitTarget?.name ?? "NULL"}");

        if (SourceModule?.sequentialPayloads == null) return;
        var resourceManager = ServiceLocator.Get<ResourceManager>();

        foreach (var payload in SourceModule.sequentialPayloads)
        {
            if (payload.onBounceNumber == _bounceCountForPayload)
            {
                Debug.Log($"[DEBUG-BULLET] Payload 효과 실행 시도. 모듈 GUID: {payload.effectToTrigger.AssetGUID}");

                CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(payload.effectToTrigger.AssetGUID);
                if (module == null) continue;

                var context = new EffectContext
                {
                    Caster = this.casterStats,
                    SpawnPoint = hitTarget,
                    Platform = this.sourcePlatform,
                    SourceCardInstance = this.sourceCardInstance,
                    HitTarget = hitTarget.GetComponent<MonsterController>(),
                    HitPosition = hitTarget.position
                };
                if (payload.overrideBaseDamage > 0 && payload.onBounceNumber > 0)
                {
                    context.BaseDamageOverride = payload.overrideBaseDamage;
                }
                module.Execute(context);
                Debug.Log($"[DEBUG-BULLET] Payload 효과 '{module.name}' 실행 완료.");
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