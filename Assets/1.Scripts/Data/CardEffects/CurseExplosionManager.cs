// 경로: ./TTttTT/Assets/1/Scripts/Gameplay/CurseExplosionManager.cs
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// [신규] 모든 몬스터의 죽음을 감지하여, 특정 '저주' 상태이상을 가진 몬스터가 죽으면 그 자리에 폭발을 일으키는 전역 관리자입니다.
/// </summary>
public class CurseExplosionManager : MonoBehaviour
{
    [Header("[ 감지할 저주 설정 ]")]
    [Tooltip("폭발을 유발하는 '저주' 효과의 Status Effect ID를 지정합니다.")]
    [SerializeField] private string curseStatusEffectID;

    [Header("[ 폭발 효과 설정 ]")]
    [Tooltip("폭발이 주변 적에게 피해를 주는 범위 (반지름)")]
    [SerializeField] private float explosionRadius = 5f;

    [Tooltip("폭발 피해량 계산 방식")]
    [SerializeField] private DamageType explosionDamageType = DamageType.Flat;

    [Tooltip("폭발의 기본 피해량")]
    [SerializeField] private float explosionDamageAmount = 50f;

    [Tooltip("체크 시, 저주를 건 플레이어의 FinalDamageBonus 스탯이 폭발 피해량에 영향을 줍니다.")]
    [SerializeField] private bool scalesWithCasterDamageBonus = false;

    [Tooltip("폭발이 일어날 때 생성될 시각 효과(VFX)의 어드레서블 주소")]
    [SerializeField] private AssetReferenceGameObject explosionVFX;

    private StatusEffectManager statusEffectManager;
    private CharacterStats playerStats;

    void Start()
    {
        // 필요한 매니저들을 미리 찾아둡니다.
        statusEffectManager = ServiceLocator.Get<StatusEffectManager>();
        var player = ServiceLocator.Get<PlayerController>();
        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
        }

        // 모든 몬스터의 사망 이벤트를 구독합니다.
        MonsterController.OnMonsterDied += HandleMonsterDied;
        Debug.Log("[CurseExplosionManager] 초기화 완료. 몬스터 사망 이벤트 구독을 시작합니다.");
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
        MonsterController.OnMonsterDied -= HandleMonsterDied;
    }

    /// <summary>
    /// 몬스터가 죽을 때마다 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void HandleMonsterDied(MonsterController deadMonster)
    {
        if (deadMonster == null) return;

        if (statusEffectManager == null || string.IsNullOrEmpty(curseStatusEffectID))
        {
            return;
        }

        // 죽은 몬스터가 지정된 ID의 저주를 가지고 있었는지 확인합니다.
        if (statusEffectManager.HasStatusEffect(deadMonster.gameObject, curseStatusEffectID))
        {
            Debug.Log($"<color=magenta>[CurseExplosionManager]</color> 저주에 걸린 '{deadMonster.name}'의 죽음을 감지! 폭발을 생성합니다.");
            CreateExplosion(deadMonster.transform.position);
        }
    }

    /// <summary>
    /// 지정된 위치에 폭발을 생성하고 주변 몬스터에게 피해를 줍니다.
    /// </summary>
    private void CreateExplosion(Vector3 position)
    {
        // 1. 폭발 피해량 계산
        float finalDamage = explosionDamageAmount;
        if (explosionDamageType == DamageType.MaxHealthPercentage)
        {
            // 현재 폭발은 적에게만 피해를 주므로, 최대 체력 비례는暂时고정 피해량으로 계산합니다.
            // 향후 기획에 따라 대상의 최대 체력을 가져와 계산하는 로직 추가 가능
        }

        if (scalesWithCasterDamageBonus && playerStats != null)
        {
            finalDamage *= (1 + playerStats.FinalDamageBonus / 100f);
        }

        // 2. 폭발 범위 내의 모든 몬스터를 찾습니다.
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<MonsterController>(out var monster))
            {
                monster.TakeDamage(finalDamage);
            }
        }

        // 3. 폭발 VFX를 재생합니다.
        PlayExplosionVFX(position);
    }

    private async void PlayExplosionVFX(Vector3 position)
    {
        if (explosionVFX == null || !explosionVFX.RuntimeKeyIsValid()) return;

        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        GameObject vfxInstance = await poolManager.GetAsync(explosionVFX.AssetGUID);
        if (vfxInstance != null)
        {
            vfxInstance.transform.position = position;
        }
    }
}