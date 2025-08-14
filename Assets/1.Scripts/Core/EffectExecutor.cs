// --- ���ϸ�: EffectExecutor.cs ---
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
        // [����� �α� 1] Execute �Լ��� ȣ��Ǿ����� Ȯ��
        Debug.Log($"[EffectExecutor] Execute �Լ� ȣ��. ī��: {cardData.cardName}, ȿ�� Ÿ��: {cardData.effectType}");

        if (cardData == null || playerController == null || playerStats == null)
        {
            Debug.LogError("[EffectExecutor] �ʼ� ������Ʈ(CardData, PlayerController, PlayerStats) �� �ϳ��� null�Դϴ�!");
            return;
        }

        if (cardData.triggerType == TriggerType.OnHit && cardData.lifestealPercentage > 0 && actualDamageDealt > 0)
        {
            playerController.Heal(actualDamageDealt * cardData.lifestealPercentage);
            Debug.Log($"[EffectExecutor] ���� �ߵ�! {actualDamageDealt * cardData.lifestealPercentage} ��ŭ ü�� ȸ��");
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
                // ... �ٸ� case �� ...
        }
    }

    // --- �� �Լ� ��ü�� ����� �α׿� �Բ� ��ü ---
    private void ExecuteSingleShot(CardDataSO cardData, Transform target)
    {
        // [����� �α� 2] �Ѿ� �������� ���������� Ȯ��
        Debug.Log("[EffectExecutor] ���� �߻�(SingleShot) ����. 'Bullet_Base' ������ �������� �õ�...");
        GameObject bulletPrefab = dataManager.GetBulletPrefab("Bullet_Base");

        if (bulletPrefab == null)
        {
            // [����� �α� 3-����] �������� ���� ���
            Debug.LogError("[EffectExecutor] ����: DataManager�� 'Bullet_Base'��� �̸��� �������� ��ϵ��� �ʾҽ��ϴ�! PrefabDB�� Ȯ���ϼ���.");
            return;
        }
        Debug.Log("[EffectExecutor] ����: 'Bullet_Base' ������ ã��!");

        Vector2 direction = (target != null) ? ((Vector3)target.position - playerController.firePoint.position).normalized : playerController.firePoint.right;

        // [����� �α� 4] Ǯ �Ŵ������� �Ѿ� ������Ʈ�� ���������� Ȯ��
        Debug.Log("[EffectExecutor] Ǯ �Ŵ������� �Ѿ� ���ӿ�����Ʈ �������� �õ�...");
        GameObject bulletGO = poolManager.Get(bulletPrefab);

        if (bulletGO == null)
        {
            // [����� �α� 5-����] ������Ʈ�� �� �������� ���
            Debug.LogError("[EffectExecutor] ����: Ǯ �Ŵ����� �Ѿ� ������Ʈ�� ��ȯ���� ���߽��ϴ�!");
            return;
        }
        Debug.Log("[EffectExecutor] ����: Ǯ���� ������Ʈ ������!");

        bulletGO.transform.position = playerController.firePoint.position;
        bulletGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        // [����� �α� 6] �Ѿ� ������Ʈ�� BulletController ��ũ��Ʈ�� �ִ��� Ȯ��
        Debug.Log("[EffectExecutor] BulletController ������Ʈ �������� �õ�...");
        if (bulletGO.TryGetComponent<BulletController>(out var bullet))
        {
            // [����� �α� 7-����] ��ũ��Ʈ�� ���� ���
            Debug.Log("[EffectExecutor] ����: BulletController ������Ʈ ã��! Initialize ȣ�� �õ�...");
            float totalDamage = playerStats.finalDamage;
            bullet.Initialize(direction, 10f, totalDamage);
            Debug.Log($"[EffectExecutor] �߻� ����! ����: {direction}, ������: {totalDamage}");
        }
        else
        {
            // [����� �α� 7-����] ��ũ��Ʈ�� ���� ���
            Debug.LogError("[EffectExecutor] ����: 'Bullet_Base' �����տ� BulletController.cs ��ũ��Ʈ�� �پ����� �ʽ��ϴ�!");
        }
    }

    // ... ������ �Լ����� ���� ...
    private void ExecuteSplitShot(CardDataSO cardData) { /* ... */ }
    private void ExecuteWave(CardDataSO cardData) { /* ... */ }
    private void ExecuteLightning(CardDataSO cardData, Transform initialTarget) { /* ... */ }
    private IEnumerator ExecuteSpiral(CardDataSO cardData) { /* ... */ yield return null; }
}