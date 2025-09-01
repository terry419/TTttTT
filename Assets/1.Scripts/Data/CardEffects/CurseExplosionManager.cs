// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/CurseExplosionManager.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

/// <summary>
/// [최종본] 저주 걸린 몬스터의 죽음을 감지하여, 그 자리에 '즉발 피해 폭발'을 일으키고 '저주를 전파하는 유도탄'을 발사하는 전역 관리자입니다.
/// </summary>
public class CurseExplosionManager : MonoBehaviour
{
    [Header("감지할 저주 ID")]
    [Tooltip("폭발 및 유도탄 발사를 유발하는 '저주' 효과의 Status Effect ID를 지정합니다.")]
    [SerializeField] private string curseStatusEffectID = "DeathMark";

    [Header("즉발 폭발 설정 (Ripple)")]
    [Tooltip("즉발 피해를 줄 RippleController 프리팹의 어드레서블 주소")]
    [SerializeField] private AssetReferenceGameObject explosionPrefab;
    [Tooltip("폭발이 피해를 주는 범위 (반지름)")]
    [SerializeField] private float explosionRadius = 5f;
    [Tooltip("폭발이 최대로 커지는 시간(초)")]
    [SerializeField] private float explosionDuration = 0.3f;
    [Tooltip("폭발의 기본 피해량")]
    [SerializeField] private float explosionDamage = 30f;

    [Header("저주 전파 유도탄 설정")]
    [Tooltip("발사할 유도탄의 로직이 담긴 ProjectileEffectSO 모듈")]
    [SerializeField] private AssetReferenceT<CardEffectSO> curseMissileModuleRef;
    [Tooltip("한 번에 발사할 유도탄의 개수")]
    [SerializeField] private int missileCount = 5;

    // --- 내부 참조 변수 ---
    private StatusEffectManager statusEffectManager;
    private CharacterStats playerStats; // 피해량 스케일링을 위한 플레이어 스탯 참조

    void Start()
    {
        statusEffectManager = ServiceLocator.Get<StatusEffectManager>();
        var player = ServiceLocator.Get<PlayerController>();
        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
        }

        MonsterController.OnMonsterDied += HandleMonsterDied;
    }

    void OnDestroy()
    {
        MonsterController.OnMonsterDied -= HandleMonsterDied;
    }

    /// <summary>
    /// 몬스터가 죽을 때마다 호출되어 저주 여부를 확인합니다.
    /// </summary>
    private void HandleMonsterDied(MonsterController deadMonster)
    {
        if (deadMonster == null || statusEffectManager == null || string.IsNullOrEmpty(curseStatusEffectID))
        {
            return;
        }

        if (statusEffectManager.HasStatusEffect(deadMonster.gameObject, curseStatusEffectID))
        {
            Debug.Log($"<color=magenta>[CurseExplosionManager]</color> 저주에 걸린 '{deadMonster.name}'의 죽음을 감지! 효과를 발동합니다.");
            // 비동기 작업이므로 UniTask의 'Forget'으로 처리
            CreateExplosion(deadMonster.transform.position).Forget();
            FireCurseMissiles(deadMonster.transform.position).Forget();
        }
    }

    /// <summary>
    /// 지정된 위치에 RippleController를 사용한 즉발성 폭발을 생성합니다.
    /// </summary>
    private async UniTaskVoid CreateExplosion(Vector3 position)
    {
        if (explosionPrefab == null || !explosionPrefab.RuntimeKeyIsValid()) return;

        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        GameObject explosionGO = await poolManager.GetAsync(explosionPrefab.AssetGUID);
        if (explosionGO != null && explosionGO.TryGetComponent<RippleController>(out var ripple))
        {
            ripple.transform.position = position;
            ripple.Initialize(playerStats, explosionRadius, explosionDuration, explosionDamage);
        }
    }

    /// <summary>
    /// 지정된 위치에서 저주를 전파하는 유도탄을 여러 발 발사합니다.
    /// </summary>
    private async UniTaskVoid FireCurseMissiles(Vector3 position)
    {
        if (curseMissileModuleRef == null || !curseMissileModuleRef.RuntimeKeyIsValid()) return;

        var resourceManager = ServiceLocator.Get<ResourceManager>();
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (resourceManager == null || poolManager == null) return;

        // 유도탄의 설계도(ProjectileEffectSO)를 로드합니다.
        ProjectileEffectSO missileModule = await resourceManager.LoadAsync<CardEffectSO>(curseMissileModuleRef.AssetGUID) as ProjectileEffectSO;
        if (missileModule == null || !missileModule.bulletPrefabReference.RuntimeKeyIsValid())
        {
            Debug.LogError("[CurseExplosionManager] 유도탄 모듈 로드에 실패했거나, 모듈에 총알 프리팹이 연결되지 않았습니다.");
            return;
        }

        Debug.Log($"[CurseExplosionManager] {missileCount}개의 저주 유도탄을 발사합니다.");

        for (int i = 0; i < missileCount; i++)
        {
            GameObject bulletGO = await poolManager.GetAsync(missileModule.bulletPrefabReference.AssetGUID);
            if (bulletGO != null && bulletGO.TryGetComponent<BulletController>(out var bullet))
            {
                bullet.transform.position = position;

                // 유도탄이 처음엔 랜덤한 방향으로 퍼져나가도록 설정
                Vector2 randomDir = Random.insideUnitCircle.normalized;

                // 유도탄 초기화. 피해량은 0으로 설정하여 '저주 전파' 역할만 하도록 함
                bullet.Initialize(randomDir, missileModule.speed * 10f, 0, System.Guid.NewGuid().ToString(), null, missileModule, playerStats, null);
            }
        }
    }
}