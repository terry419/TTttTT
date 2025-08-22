using UnityEngine;
using System;

/// <summary>
/// 'SingleShot' 타입의 카드 효과를 처리하는 클래스입니다.
/// </summary>
public class SingleShotHandler : ICardEffectHandler
{
    public void Execute(CardDataSO cardData, EffectExecutor executor, Transform spawnPoint)
    {
        GameObject bulletPrefab = cardData.bulletPrefab;
        if (bulletPrefab == null)
        {
            Debug.LogError($"[SingleShotHandler] 오류: 카드 '{cardData.cardName}'에 bulletPrefab이 할당되지 않았습니다!");
            return;
        }


        // 타겟팅 타입에 따라 발사 각도를 계산합니다.
        float angle = executor.GetTargetingAngle(cardData.targetingType);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector2 direction = rotation * Vector2.right;

        // 풀 매니저에서 총알 인스턴스를 가져옵니다.
        GameObject bulletGO = ServiceLocator.Get<PoolManager>().Get(bulletPrefab);
        if (bulletGO == null)
        {
            Debug.LogError($"[SingleShotHandler] 오류: 풀 매니저에서 총알 오브젝트를 가져오지 못했습니다!");
            return;
        }

        // 총알 위치와 회전을 설정합니다.
        // [수정됨] playerController.firePoint를 사용합니다.
        bulletGO.transform.position = spawnPoint.position;
        bulletGO.transform.rotation = rotation;

        if (bulletGO.TryGetComponent<BulletController>(out var bullet))
        {
            // 총알을 초기화하고 발사합니다.
            float totalDamage = executor.CalculateTotalDamage(cardData);
            string shotID = Guid.NewGuid().ToString(); // 관통 효과를 위한 고유 ID

            bullet.Initialize(direction, cardData.bulletSpeed, totalDamage, shotID, cardData);
        }
        else
        {
            Debug.LogError($"[SingleShotHandler] 오류: '{bulletPrefab.name}' 프리팹에 BulletController.cs 스크립트가 없습니다!");
        }
    }
}