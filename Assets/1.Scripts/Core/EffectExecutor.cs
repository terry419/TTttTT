// --- 파일명: EffectExecutor.cs ---
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectExecutor : MonoBehaviour
{
    public static EffectExecutor Instance { get; private set; }

    private PoolManager poolManager;
    private DataManager dataManager;
    private PlayerController playerController;
    private CharacterStats playerStats;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        poolManager = PoolManager.Instance;
        dataManager = DataManager.Instance;
        playerController = PlayerController.Instance;
        if (playerController != null)
        {
            playerStats = playerController.GetComponent<CharacterStats>();
        }
    }

    public void Execute(CardDataSO cardData, float actualDamageDealt = 0f)
    {
        // [디버그 로그 1] Execute 함수가 호출되었는지 확인
        Debug.Log($"[EffectExecutor] Execute 함수 호출. 카드: {cardData.cardName}, 효과 타입: {cardData.effectType}");

        if (cardData == null || playerController == null || playerStats == null)
        {
            Debug.LogError("[EffectExecutor] 필수 컴포넌트(CardData, PlayerController, PlayerStats) 중 하나가 null입니다!");
            return;
        }

        if (cardData.triggerType == TriggerType.OnHit && cardData.lifestealPercentage > 0 && actualDamageDealt > 0)
        {
            playerController.Heal(actualDamageDealt * cardData.lifestealPercentage);
            Debug.Log($"[EffectExecutor] 흡혈 발동! {actualDamageDealt * cardData.lifestealPercentage} 만큼 체력 회복");
        }

        Transform target = TargetingSystem.FindTarget(cardData.targetingType, playerController.transform);

        switch (cardData.effectType)
        {
            case CardEffectType.SingleShot:
                ExecuteSingleShot(cardData, target);
                break;
            case CardEffectType.SplitShot:
                ExecuteSplitShot(cardData);
                break;
                // ... 다른 case 들 ...
        }
    }

    // --- 이 함수 전체를 디버그 로그와 함께 교체 ---
    private void ExecuteSingleShot(CardDataSO cardData, Transform target)
    {
        // [디버그 로그 2] 총알 프리팹을 가져오는지 확인
        Debug.Log("[EffectExecutor] 단일 발사(SingleShot) 실행. 'Bullet_Base' 프리팹 가져오기 시도...");
        GameObject bulletPrefab = dataManager.GetBulletPrefab("Bullet_Base");

        if (bulletPrefab == null)
        {
            // [디버그 로그 3-실패] 프리팹이 없을 경우
            Debug.LogError("[EffectExecutor] 실패: DataManager에 'Bullet_Base'라는 이름의 프리팹이 등록되지 않았습니다! PrefabDB를 확인하세요.");
            return;
        }
        Debug.Log("[EffectExecutor] 성공: 'Bullet_Base' 프리팹 찾음!");

        Vector2 direction = (target != null) ? ((Vector3)target.position - playerController.firePoint.position).normalized : playerController.firePoint.right;

        // [디버그 로그 4] 풀 매니저에서 총알 오브젝트를 가져오는지 확인
        Debug.Log("[EffectExecutor] 풀 매니저에서 총알 게임오브젝트 가져오기 시도...");
        GameObject bulletGO = poolManager.Get(bulletPrefab);

        if (bulletGO == null)
        {
            // [디버그 로그 5-실패] 오브젝트를 못 가져왔을 경우
            Debug.LogError("[EffectExecutor] 실패: 풀 매니저가 총알 오브젝트를 반환하지 못했습니다!");
            return;
        }
        Debug.Log("[EffectExecutor] 성공: 풀에서 오브젝트 가져옴!");

        bulletGO.transform.position = playerController.firePoint.position;
        bulletGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        // [디버그 로그 6] 총알 오브젝트에 BulletController 스크립트가 있는지 확인
        Debug.Log("[EffectExecutor] BulletController 컴포넌트 가져오기 시도...");
        if (bulletGO.TryGetComponent<BulletController>(out var bullet))
        {
            // [디버그 로그 7-성공] 스크립트가 있을 경우
            Debug.Log("[EffectExecutor] 성공: BulletController 컴포넌트 찾음! Initialize 호출 시도...");
            float totalDamage = playerStats.finalDamage;
            bullet.Initialize(direction, 10f, totalDamage);
            Debug.Log($"[EffectExecutor] 발사 성공! 방향: {direction}, 데미지: {totalDamage}");
        }
        else
        {
            // [디버그 로그 7-실패] 스크립트가 없을 경우
            Debug.LogError("[EffectExecutor] 실패: 'Bullet_Base' 프리팹에 BulletController.cs 스크립트가 붙어있지 않습니다!");
        }
    }

    // ... 나머지 함수들은 동일 ...
    private void ExecuteSplitShot(CardDataSO cardData) { /* ... */ }
    private void ExecuteWave(CardDataSO cardData) { /* ... */ }
    private void ExecuteLightning(CardDataSO cardData, Transform initialTarget) { /* ... */ }
    private IEnumerator ExecuteSpiral(CardDataSO cardData) { /* ... */ yield return null; }
}