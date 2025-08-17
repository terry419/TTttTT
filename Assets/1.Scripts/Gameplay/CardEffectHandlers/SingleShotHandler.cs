// --- ���� ��ġ: Assets/1.Scripts/Gameplay/CardEffectHandlers/SingleShotHandler.cs ---

using UnityEngine;
using System;

/// <summary>
/// 'SingleShot' Ÿ���� ī�� ȿ���� ó���ϴ� Ŭ�����Դϴ�.
/// </summary>
public class SingleShotHandler : ICardEffectHandler
{
    public void Execute(CardDataSO cardData, EffectExecutor executor, Transform spawnPoint)
    {
        GameObject bulletPrefab = cardData.bulletPrefab;
        if (bulletPrefab == null)
        {
            Debug.LogError($"[SingleShotHandler] ����: ī�� '{cardData.cardName}'�� bulletPrefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // Ÿ���� Ÿ�Կ� ���� �߻� ������ ����մϴ�.
        float angle = executor.GetTargetingAngle(cardData.targetingType);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector2 direction = rotation * Vector2.right;

        // Ǯ �Ŵ����� ���� �Ѿ� �ν��Ͻ��� �����ɴϴ�.
        GameObject bulletGO = executor.poolManager.Get(bulletPrefab);
        if (bulletGO == null)
        {
            Debug.LogError($"[SingleShotHandler] ����: Ǯ �Ŵ������� �Ѿ� ������Ʈ�� �������� ���߽��ϴ�!");
            return;
        }

        // �߻� ��ġ�� ȸ������ �����մϴ�.
        bulletGO.transform.position = executor.playerController.firePoint.position;
        bulletGO.transform.rotation = rotation;

        if (bulletGO.TryGetComponent<BulletController>(out var bullet))
        {
            // ���� �������� ����ϰ� �Ѿ��� �ʱ�ȭ�մϴ�.
            float totalDamage = executor.CalculateTotalDamage(cardData);
            string shotID = Guid.NewGuid().ToString(); // �п�ź ��� �����ϱ� ���� ���� ID

            bullet.Initialize(direction, cardData.bulletSpeed, totalDamage, shotID, cardData);
        }
        else
        {
            Debug.LogError($"[SingleShotHandler] ����: '{bulletPrefab.name}' �����տ� BulletController.cs ��ũ��Ʈ�� �����ϴ�!");
        }
    }
}
