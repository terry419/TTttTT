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
    public ProjectileEffectSO SourceModule { get; private set; }

    public int _currentPierceCount;
    public HashSet<GameObject> _hitMonsters = new HashSet<GameObject>();

    private int _currentRicochetCount;
    private Rigidbody2D rb;
    private bool isTracking;
    private Transform trackingTarget;
    private float turnSpeed = 200f;
    private EntityStats casterStats;
    private int _bounceCountForPayload = 0;
    private NewCardDataSO sourcePlatform;
    private CardInstance sourceCardInstance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.sortingOrder = 10;
        }
    }

    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, NewCardDataSO platform, ProjectileEffectSO module, EntityStats caster, CardInstance instance, 
        HashSet<GameObject> initialHitMonsters = null, int? pierceCountOverride = null, int? ricochetCountOverride = null, bool? isTrackingOverride = null)
    {
        this._direction = direction.normalized;
        this.speed = initialSpeed;
        this.damage = damage;

        if (string.IsNullOrEmpty(shotID))
        {
            this.shotInstanceID = System.Guid.NewGuid().ToString();
        }
        else
        {
            this.shotInstanceID = shotID;
        }

        this.SourceModule = module;
        this.sourcePlatform = platform;
        this.sourceCardInstance = instance;
        this.casterStats = caster;
        this.trackingTarget = null;

        // 1. 무시할 몬스터 목록 초기화
        _hitMonsters.Clear();
        if (initialHitMonsters != null)
        {
            foreach (var monster in initialHitMonsters)
            {
                _hitMonsters.Add(monster);
            }
        }

        // 2. 특수 능력치 설정 (오버라이드 > 모듈 > 기본값)
        this._currentPierceCount = pierceCountOverride ?? module?.pierceCount ?? 0;
        this._currentRicochetCount = ricochetCountOverride ?? module?.ricochetCount ?? 0;
        this.isTracking = isTrackingOverride ?? module?.isTracking ?? false;

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
                // 이미 맞은 몬스터
            }
            else
            {
                monster.TakeDamage(this.damage);
                _hitMonsters.Add(other.gameObject);

                Debug.Log($"[BulletController] 몬스터 '{monster.name}' 명중. OnHit 효과 실행을 시작합니다.");

                if (this.sourceCardInstance != null)
                {
                    var effectContext = new EffectContext
                    {
                        Caster = this.casterStats,
                        SpawnPoint = this.transform,
                        Platform = this.sourcePlatform,
                        SourceCardInstance = this.sourceCardInstance,
                        HitTarget = monster,
                        HitPosition = monster.transform.position,
                        DamageDealt = this.damage
                    };

                    Debug.Log($"[BulletController] 원본 카드 '{sourceCardInstance.CardData.name}'에는 {sourceCardInstance.CardData.modules.Count}개의 모듈이 있습니다.");

                    foreach (var moduleEntry in this.sourceCardInstance.CardData.modules)
                    {
                        if (moduleEntry.moduleReference.Asset is CardEffectSO effectSO)
                        {
                            Debug.Log($"[BulletController] 모듈 '{effectSO.name}' 확인 중... 트리거 타입: {effectSO.trigger}");
                            if (effectSO.trigger == CardEffectSO.EffectTrigger.OnHit)
                            {
                                Debug.Log($"<color=yellow>[BulletController] OnHit 트리거 감지! '{effectSO.name}' 모듈을 실행합니다.</color>");
                                effectSO.Execute(effectContext);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[BulletController] sourceCardInstance가 null이라 OnHit 효과를 실행할 수 없습니다.");
                }
            }

            HandlePayloads_Async(monster.transform).Forget();
            
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
        Debug.Log($"<color=orange>[HandlePayloads_Async]</color> 실행 시작. 대상: {hitTarget?.name ?? "NULL"}");

        if (SourceModule?.sequentialPayloads == null || SourceModule.sequentialPayloads.Count == 0)
        {
            Debug.LogWarning($"[HandlePayloads_Async] SourceModule이 없거나 sequentialPayloads가 비어있어 실행을 중단합니다.");
            return;
        }

        var resourceManager = ServiceLocator.Get<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogError("[HandlePayloads_Async] ResourceManager를 찾을 수 없습니다.");
            return;
        }

        Debug.Log($"[HandlePayloads_Async] 현재 Bounce 카운트({_bounceCountForPayload})와 일치하는 Payload를 찾습니다. (총 {SourceModule.sequentialPayloads.Count}개 페이로드 확인)");

        foreach (var payload in SourceModule.sequentialPayloads)
        {
            Debug.Log($"[HandlePayloads_Async] ... 확인 중인 Payload: onBounceNumber = {payload.onBounceNumber}");

            if (payload.onBounceNumber == _bounceCountForPayload)
            {
                Debug.Log($"<color=yellow>[HandlePayloads_Async] 조건 일치! onBounceNumber({payload.onBounceNumber})에서 '{payload.effectToTrigger.AssetGUID}' 효과를 실행합니다.</color>");

                CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(payload.effectToTrigger.AssetGUID);
                if (module == null)
                {
                    Debug.LogError($"[HandlePayloads_Async] Payload 효과 '{payload.effectToTrigger.AssetGUID}'를 로드하는 데 실패했습니다.");
                    continue;
                }

                var context = new EffectContext
                {
                    Caster = this.casterStats,
                    SpawnPoint = hitTarget, // 발동 위치는 맞은 대상
                    Platform = this.sourcePlatform,
                    SourceCardInstance = this.sourceCardInstance,
                    HitTarget = hitTarget.GetComponent<MonsterController>(),
                    HitPosition = hitTarget.position,
                    DamageDealt = this.damage // 원본 총알의 데미지를 전달
                };

                if (payload.overrideBaseDamage > 0)
                {
                    context.BaseDamageOverride = payload.overrideBaseDamage;
                }

                Debug.Log($"<color=lime>[HandlePayloads_Async]</color> '{module.name}' 모듈의 Execute를 호출합니다.");
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