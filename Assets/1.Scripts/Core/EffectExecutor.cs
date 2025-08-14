using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 카드 및 유물 효과 실행을 총괄하는 클래스입니다.
/// 카드의 '조준 방식'과 '발사 형태'를 조합하여 다양한 공격을 구현합니다.
/// </summary>
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

    /// <summary>
    /// [핵심 수정] 인수 1개 버전: 데미지 정보가 없는 일반적인 효과 발동 시 사용됩니다.
    /// (예: 자동 공격, 주기적 발동)
    /// </summary>
    public void Execute(CardDataSO cardData)
    {
        // 내부적으로 인수 2개 버전을 호출하여 코드를 재사용하고, 호환성을 유지합니다.
        Execute(cardData, 0f);
    }

    /// <summary>
    /// [기존 유지] 인수 2개 버전: OnHit(적중 시) 효과처럼 실제 데미지 정보가 필요할 때 사용됩니다.
    /// </summary>
    public void Execute(CardDataSO cardData, float actualDamageDealt)
    {
        if (cardData == null || playerController == null || playerStats == null) return;
        Debug.Log($"Executing card effect: {cardData.cardName}");

        // --- OnHit 트리거 관련 로직 (데미지 정보가 있을 때만 의미 있음) ---
        if (cardData.triggerType == TriggerType.OnHit)
        {
            // 흡혈 효과
            if (cardData.lifestealPercentage > 0 && actualDamageDealt > 0)
            {
                playerController.Heal(actualDamageDealt * cardData.lifestealPercentage);
            }
        }

        // --- 발사체/이펙트 생성 로직 ---
        Transform target = TargetingSystem.FindTarget(cardData.targetingType, playerController.transform);

        switch (cardData.effectType)
        {
            case CardEffectType.None:
                ExecuteSingleShot(cardData, target);
                break;
            case CardEffectType.SplitShot:
                ExecuteSplitShot(cardData);
                break;
                // TODO: Wave, Spiral 등 다른 로직 추가
        }
    }

    private void ExecuteSingleShot(CardDataSO cardData, Transform target)
    {
        GameObject bulletPrefab = dataManager.GetEffectPrefab("Bullet_Base");
        if (bulletPrefab == null) { Debug.LogWarning("Bullet_Base 프리팹을 PrefabDB에 등록해야 합니다."); return; }

        Vector2 direction = (target != null) ? (Vector2)(target.position - playerController.firePoint.position).normalized : (Vector2)playerController.firePoint.right;

        GameObject bulletGO = poolManager.Get(bulletPrefab);
        bulletGO.transform.position = playerController.firePoint.position;
        bulletGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        BulletController bullet = bulletGO.GetComponent<BulletController>();
        if (bullet != null)
        {
            float totalDamage = playerStats.finalDamage * cardData.damageMultiplier;
            bullet.Initialize(direction, 10f, totalDamage);
        }
    }

    private void ExecuteSplitShot(CardDataSO cardData)
    {
        GameObject bulletPrefab = dataManager.GetEffectPrefab("Bullet_Base");
        if (bulletPrefab == null) return;

        int splitCount = 5;
        float totalDamage = playerStats.finalDamage * cardData.damageMultiplier;

        for (int i = 0; i < splitCount; i++)
        {
            float angle = i * (360f / splitCount);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 direction = rotation * Vector2.right;

            GameObject bulletGO = poolManager.Get(bulletPrefab);
            bulletGO.transform.position = playerController.firePoint.position;
            bulletGO.transform.rotation = rotation;

            BulletController bullet = bulletGO.GetComponent<BulletController>();
            if (bullet != null)
            {
                bullet.Initialize(direction, 10f, totalDamage);
            }
        }
    }
}