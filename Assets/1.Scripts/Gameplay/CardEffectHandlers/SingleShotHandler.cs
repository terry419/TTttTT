using UnityEngine;
using System;
using Cysharp.Threading.Tasks; // Added for async operations, though async void is used for now

/// <summary>
/// 'SingleShot' 타입의 카드 효과를 처리하는 클래스입니다.
/// </summary>
public class SingleShotHandler : ICardEffectHandler
{
    public async void Execute(CardDataSO cardData, EffectExecutor executor, CharacterStats casterStats, Transform spawnPoint)
    {
        if (cardData.bulletPrefab == null)
        { 
            Debug.LogError($"[SingleShotHandler] 오류: 카드 '{cardData.cardName}'에 bulletPrefab이 할당되지 않았습니다!");
            return;
        }

        string key = cardData.bulletPrefab.name;

        // 타겟팅 타입에 따라 발사 각도를 계산합니다.
        float angle = executor.GetTargetingAngle(cardData.targetingType, casterStats.transform, spawnPoint);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector2 direction = rotation * Vector2.right;

        // 풀 매니저에서 총알 인스턴스를 비동기적으로 가져옵니다.
        GameObject bulletGO = await ServiceLocator.Get<PoolManager>().GetAsync(key);
        if (bulletGO == null)
        { 
            Debug.LogError($"[SingleShotHandler] 오류: 풀 매니저에서 '{key}' 총알 오브젝트를 가져오지 못했습니다!");
            return;
        }

        // 총알 위치와 회전을 설정합니다.
        bulletGO.transform.position = spawnPoint.position;
        bulletGO.transform.rotation = rotation;

        if (bulletGO.TryGetComponent<BulletController>(out var bullet))
        { 
            // 총알을 초기화하고 발사합니다.
            float totalDamage = executor.CalculateTotalDamage(cardData, casterStats);
            string shotID = Guid.NewGuid().ToString(); // 관통 효과를 위한 고유 ID

            bullet.Initialize(direction, cardData.bulletSpeed, totalDamage, shotID, cardData, cardData.bulletPierceCount, casterStats);
        }
        else
        {
            Debug.LogError($"[SingleShotHandler] 오류: '{key}' 프리팹에 BulletController.cs 스크립트가 없습니다!");
        }
    }
}
