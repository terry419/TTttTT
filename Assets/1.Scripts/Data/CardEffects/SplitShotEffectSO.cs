using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "Module_SplitShot_", menuName = "GameData/CardData/Modules/SplitShotEffect")]
public class SplitShotEffectSO : CardEffectSO
{
    [Header("분열탄 설정")]
    [Tooltip("분열되는 총알의 개수")]
    public int splitCount = 10;
    [Tooltip("분열탄의 데미지 배율 (1.0 = 원본 데미지의 100%)")]
    public float splitDamageMultiplier = 1.0f;

    [Tooltip("분열탄으로 발사할 총알 프리팹")]
    public UnityEngine.AddressableAssets.AssetReferenceGameObject bulletPrefabReference;

    [Header("분열탄 발사 방식")]
    [Tooltip("분열탄이 퍼지는 총 각도. 0이면 모든 총알이 한 방향으로 나갑니다.")]
    public float spreadAngle = 45f;
    [Tooltip("분열탄의 유도 기능 사용 여부")]
    public bool isTracking = false;

    [Header("분열탄 자체 능력치 (덮어쓰기)")]
    [Tooltip("분열탄의 관통 횟수")]
    public int pierceCount = 0;
    [Tooltip("분열탄의 튕김 횟수")]
    public int ricochetCount = 0;

    public SplitShotEffectSO()
    {
        trigger = EffectTrigger.OnHit;
    }

    public override void Execute(EffectContext context)
    {
        if (context.HitTarget == null) return;
        SplitAndFireAsync(context).Forget();
    }

    private async UniTaskVoid SplitAndFireAsync(EffectContext context)
    {
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null || (bulletPrefabReference == null || !bulletPrefabReference.RuntimeKeyIsValid()))
        {
            Debug.LogError("[SplitShotEffectSO] PoolManager 또는 총알 프리팹이 유효하지 않습니다!");
            return;
        }

        // 1. 기준 방향 설정: 가장 가까운 적을 향하는 방향을 기본으로 합니다.
        var exclusions = new HashSet<GameObject> { context.HitTarget.gameObject };
        var primaryTarget = TargetingSystem.FindTarget(TargetingType.Nearest, context.SpawnPoint, exclusions);
        
        Vector2 centerDirection;
        if (primaryTarget != null)
        {
            centerDirection = (primaryTarget.position - context.HitPosition).normalized;
        }
        else
        {
            // 주변에 다른 적이 없으면, 원래 총알이 날아온 방향의 반대 방향으로 발사합니다.
            // (단, 현재 구조상 원본 총알의 방향을 알 수 없으므로 임시로 Caster의 앞 방향을 사용합니다.)
            centerDirection = context.Caster.transform.right; 
        }

        // 2. 즉시 피격 방지를 위한 초기 무시 목록 생성
        var initialHitSet = new HashSet<GameObject> { context.HitTarget.gameObject };
        float totalDamage = context.DamageDealt * splitDamageMultiplier;

        // 3. 부채꼴 발사 로직
        int count = splitCount;
        float startAngle = -spreadAngle / 2;
        float angleStep = (count > 1) ? spreadAngle / (count - 1) : 0;

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            Vector2 direction = Quaternion.Euler(0, 0, currentAngle) * centerDirection;

            GameObject bulletGO = await poolManager.GetAsync(bulletPrefabReference.AssetGUID);
            if (bulletGO != null && bulletGO.TryGetComponent<BulletController>(out var bullet))
            {
                bullet.transform.position = context.HitPosition;
                bullet.Initialize(direction, 15f, totalDamage, System.Guid.NewGuid().ToString(), context.Platform, null, context.Caster, null, 
                    initialHitMonsters: initialHitSet, 
                    pierceCountOverride: this.pierceCount, 
                    ricochetCountOverride: this.ricochetCount, 
                    isTrackingOverride: this.isTracking);
            }
        }
    }
}