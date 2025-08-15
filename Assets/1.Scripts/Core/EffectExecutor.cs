// --- 파일명: EffectExecutor.cs (오류 수정) ---
// 경로: Assets/1.Scripts/Core/EffectExecutor.cs
using UnityEngine;
using System.Collections;

public class EffectExecutor : MonoBehaviour
{
    public static EffectExecutor Instance { get; private set; }

    private PoolManager poolManager;
    private PlayerController playerController;
    private CharacterStats playerStats;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        poolManager = PoolManager.Instance;
        playerController = PlayerController.Instance;
        if (playerController != null)
        {
            playerStats = playerController.GetComponent<CharacterStats>();
        }
    }

    public void Execute(CardDataSO cardData, float actualDamageDealt = 0f)
    {
        if (cardData == null || playerController == null || playerStats == null)
        {
            Debug.LogError("[EffectExecutor] 필수 컴포넌트(CardData, PlayerController, PlayerStats) 중 하나가 null입니다!");
            return;
        }

        if (cardData.triggerType == TriggerType.OnHit && cardData.lifestealPercentage > 0 && actualDamageDealt > 0)
        {
            playerController.Heal(actualDamageDealt * cardData.lifestealPercentage);
        }

        Transform target = TargetingSystem.FindTarget(cardData.targetingType, playerController.transform);

        switch (cardData.effectType)
        {
            case CardEffectType.SingleShot:
                ExecuteSingleShot(cardData, target);
                break;
            case CardEffectType.SplitShot:
                // ExecuteSplitShot(cardData);
                break;
        }
    }

    private void ExecuteSingleShot(CardDataSO cardData, Transform target)
    {
        // [수정] cardData.bulletPrefabName 대신 cardData.bulletPrefab을 직접 사용합니다.
        GameObject bulletPrefab = cardData.bulletPrefab;

        if (bulletPrefab == null)
        {
            Debug.LogError($"[EffectExecutor] 오류: 카드 '{cardData.cardName}'에 bulletPrefab이 할당되지 않았습니다!");
            return;
        }

        Vector2 direction = (target != null) ? ((Vector3)target.position - playerController.firePoint.position).normalized : playerController.firePoint.right;

        GameObject bulletGO = poolManager.Get(bulletPrefab);

        if (bulletGO == null)
        {
            Debug.LogError($"[EffectExecutor] 오류: 풀 매니저에서 총알 오브젝트를 가져오지 못했습니다!");
            return;
        }

        bulletGO.transform.position = playerController.firePoint.position;
        bulletGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        if (bulletGO.TryGetComponent<BulletController>(out var bullet))
        {
            float totalDamage = playerStats.finalDamage * cardData.damageMultiplier;
            bullet.Initialize(direction, 10f, totalDamage);
        }
        else
        {
            Debug.LogError($"[EffectExecutor] 오류: '{bulletPrefab.name}' 프리팹에 BulletController.cs 스크립트가 붙어있지 않습니다!");
        }
    }
}