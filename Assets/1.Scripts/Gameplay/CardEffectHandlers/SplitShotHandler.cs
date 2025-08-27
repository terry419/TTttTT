using UnityEngine;
using System;

/// <summary>
/// 'SplitShot' 타입의 카드 효과를 처리하는 클래스입니다.
/// </summary>
public class SplitShotHandler : ICardEffectHandler
{
    public void Execute(CardDataSO cardData, EffectExecutor executor, CharacterStats casterStats, Transform spawnPoint)
    {
        GameObject bulletPrefab = cardData.bulletPrefab;
        if (bulletPrefab == null)
        {
            Debug.LogError($"[SplitShotHandler] 오류: 스플릿샷 카드 '{cardData.cardName}'에 bulletPrefab이 할당되지 않았습니다!");
            return;
        }


        // 기본 발사 각도를 계산합니다.
        float baseAngle = executor.GetTargetingAngle(cardData.targetingType, casterStats.transform, spawnPoint);

        // 발사할 총알의 개수, 각도 등을 계산합니다.
        int projectileCount = Mathf.Max(1, (int)cardData.triggerValue);
        float angleStep = 360f / projectileCount;
        float totalDamage = executor.CalculateTotalDamage(cardData, casterStats);
        string shotID = Guid.NewGuid().ToString(); // 모든 총알에 대해 동일한 관통 ID를 사용합니다.

        for (int i = 0; i < projectileCount; i++)
        {
            // 현재 총알의 발사 각도를 계산합니다.
            float currentAngle = baseAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 direction = rotation * Vector2.right;

            GameObject bulletGO = ServiceLocator.Get<PoolManager>().Get(bulletPrefab);

            if (bulletGO == null) continue;

            // 총알 위치와 회전을 설정합니다.
            bulletGO.transform.position = spawnPoint.position;
            bulletGO.transform.rotation = rotation;

            if (bulletGO.TryGetComponent<BulletController>(out var bullet))
            {
                bullet.Initialize(direction, cardData.bulletSpeed, totalDamage, shotID, cardData, cardData.bulletPierceCount, casterStats);
            }
        }
    }
}