using UnityEngine;
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
        if (cardData == null || playerController == null || playerStats == null) return;

        if (cardData.triggerType == TriggerType.OnHit)
        {
            if (cardData.lifestealPercentage > 0 && actualDamageDealt > 0)
            {
                playerController.Heal(actualDamageDealt * cardData.lifestealPercentage);
            }
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
        }
    }

    private void ExecuteSingleShot(CardDataSO cardData, Transform target)
    {
        GameObject bulletPrefab = dataManager.GetBulletPrefab("Bullet_Base");
        if (bulletPrefab == null) return;

        Vector2 direction = (target != null) ? ((Vector3)target.position - playerController.firePoint.position).normalized : playerController.firePoint.right;

        GameObject bulletGO = poolManager.Get(bulletPrefab);
        bulletGO.transform.position = playerController.firePoint.position;
        bulletGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        if (bulletGO.TryGetComponent<BulletController>(out var bullet))
        {
            float totalDamage = playerStats.finalDamage * cardData.damageMultiplier;
            bullet.Initialize(direction, 10f, totalDamage);
        }
    }

    private void ExecuteSplitShot(CardDataSO cardData)
    {
        GameObject bulletPrefab = dataManager.GetBulletPrefab("Bullet_Base");
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

            if (bulletGO.TryGetComponent<BulletController>(out var bullet))
            {
                bullet.Initialize(direction, 10f, totalDamage);
            }
        }
    }
}