// 경로: ./TTttTT/Assets/1.Scripts/Gameplay/BulletController.cs

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

            // [핵심 수정] async void 메소드로 연쇄 효과 처리
            _ = HandlePayloads_Async(monster.transform);

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

    private async UniTaskVoid HandlePayloads_Async(Transform hitTarget)
    {
        if (SourceModule == null || SourceModule.sequentialPayloads == null) return;

        // 이제 EffectExecutor 대신 ResourceManager를 직접 사용합니다.
        var resourceManager = ServiceLocator.Get<ResourceManager>();

        foreach (var payload in SourceModule.sequentialPayloads)
        {
            if (payload.onBounceNumber == _bounceCountForPayload)
            {
                // 1. 연쇄 효과 모듈(SO)을 안전하게 로드합니다.
                CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(payload.effectToTrigger.AssetGUID);
                if (module == null) continue;

                // 2. 연쇄 효과 실행에 필요한 정보(Context)를 생성합니다.
                var context = new EffectContext
                {
                    Caster = this.casterStats,
                    SpawnPoint = hitTarget,
                    Platform = this.sourcePlatform,
                    HitTarget = hitTarget.GetComponent<MonsterController>(),
                    HitPosition = hitTarget.position
                };

                // 3. 모듈의 Execute 메소드를 직접 실행합니다.
                module.Execute(context);

                // 데이터 에셋이므로 여기서 Release하지 않습니다. ResourceManager가 관리합니다.
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