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
        if (string.IsNullOrEmpty(shotID))
        {
            this.shotInstanceID = System.Guid.NewGuid().ToString();
            Debug.LogWarning($"[BulletController] Initialize에 유효하지 않은 shotID가 전달되어 새 ID를 생성했습니다: {this.shotInstanceID}");
        }
        else
        {
            this.shotInstanceID = shotID;
        }

        this.shotInstanceID = shotID;
        this.SourceModule = module;
        this.sourcePlatform = platform;
        this.sourceCardInstance = instance;
        this._currentPierceCount = module.pierceCount;
        this._currentRicochetCount = module.ricochetCount;
        this.isTracking = module.isTracking;
        this.trackingTarget = null;
        this._hitMonsters.Clear();
        this.casterStats = caster;

        if (module != null)
        {
            this._currentPierceCount = module.pierceCount;
            this._currentRicochetCount = module.ricochetCount;
            this.isTracking = module.isTracking;
        }
        else
        {
            // 모듈이 없으면 관통, 튕김, 추적 기능도 없습니다.
            this._currentPierceCount = 0;
            this._currentRicochetCount = 0;
            this.isTracking = false;
        }


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
            bool canHitMultiple = (sourcePlatform != null) ? sourcePlatform.allowMultipleHits : false;

            if (!monster.RegisterHitByShot(this.shotInstanceID, canHitMultiple))
            {
                // 이미 맞은 몬스터는 피해 없이 관통/튕김 로직만 처리
            }
            else
            {
                monster.TakeDamage(this.damage);
                _hitMonsters.Add(other.gameObject);

                // 몬스터가 피해를 받고 즉시 죽은 경우
                if (monster == null)
                {
                    if (TryPierce()) return;
                    Deactivate();
                    return;
                }

                // 몬스터가 살아있다면, 페이로드 효과를 실행합니다.
                _ = HandlePayloads_Async(monster.transform);
            }

            // 페이로드 효과로 몬스터가 죽었을 수 있으므로 다시 한번 확인합니다.
            if (monster == null)
            {
                if (TryPierce()) return;
                Deactivate();
                return;
            }

            // 몬스터가 살아있다면, 튕기기(Ricochet)를 시도합니다.
            if (TryRicochet(monster.transform)) return;

            // 튕기지 않았다면, 관통(Pierce)을 시도합니다.
            if (TryPierce()) return;

            // 모든 조건에 해당하지 않으면 총알은 소멸합니다.
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