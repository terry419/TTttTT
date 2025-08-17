// --- ���� ��ġ: Assets/1.Scripts/Gameplay/CardEffectHandlers/SplitShotHandler.cs ---

using UnityEngine;
using System;

/// <summary>
/// 'SplitShot' Ÿ���� ī�� ȿ���� ó���ϴ� Ŭ�����Դϴ�.
/// </summary>
public class SplitShotHandler : ICardEffectHandler
{
    public void Execute(CardDataSO cardData, EffectExecutor executor, Transform spawnPoint)
    {
        GameObject bulletPrefab = cardData.bulletPrefab;
        if (bulletPrefab == null)
        {
            Debug.LogError($"[SplitShotHandler] ����: �п� ī�� '{cardData.cardName}'�� bulletPrefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // �⺻ �߻� ������ ����մϴ�.
        float baseAngle = executor.GetTargetingAngle(cardData.targetingType);

        // �߻��� �Ѿ��� ����, ����, ������ ���� ����մϴ�.
        int projectileCount = Mathf.Max(1, (int)cardData.triggerValue);
        float angleStep = 360f / projectileCount;
        float totalDamage = executor.CalculateTotalDamage(cardData);
        string shotID = Guid.NewGuid().ToString(); // ��� �п�ź�� ������ �ǰ� ������ �����ϵ��� ID�� �����մϴ�.

        for (int i = 0; i < projectileCount; i++)
        {
            // �⺻ �������� �߻� ������ ����մϴ�.
            float currentAngle = baseAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 direction = rotation * Vector2.right;

            GameObject bulletGO = executor.poolManager.Get(cardData.bulletPrefab);
            if (bulletGO == null) continue;

            bulletGO.transform.position = executor.playerController.firePoint.position;
            bulletGO.transform.rotation = rotation;

            if (bulletGO.TryGetComponent<BulletController>(out var bullet))
            {
                bullet.Initialize(direction, cardData.bulletSpeed, totalDamage, shotID, cardData);
            }
        }
    }
}
