using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Linq;

public class ModuleAction : ICardAction
{
    public async UniTask Execute(CardActionContext context)
    {
        var card = context.CardInstance.CardData;

        // 이 카드에 연결된 모듈 중 발사체(Projectile) 효과를 찾아냅니다.
        ProjectileEffectSO pModule = null;
        foreach (var moduleEntry in card.modules)
        { 
            if (moduleEntry.moduleReference.RuntimeKeyIsValid())
            {
                var resourceManager = ServiceLocator.Get<ResourceManager>();
                CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(moduleEntry.moduleReference.AssetGUID);
                if (module is ProjectileEffectSO foundPModule)
                {
                    pModule = foundPModule;
                    break; // 첫 번째로 발견된 발사체 모듈을 사용합니다.
                }
            }
        }

        if (pModule == null || !pModule.bulletPrefabReference.RuntimeKeyIsValid())
        {
            Debug.LogWarning($"[{card.name}] 발사할 ProjectileEffectSO 모듈을 찾지 못했습니다.");
        }
        else
        {
            // 발사체 모듈을 찾았다면, 해당 모듈의 설정에 따라 발사체를 쏩니다.
            await FireProjectiles(pModule, card, context);
        }

        // 'OnFire'(발사 시) 트리거를 가진 다른 모듈들을 실행합니다.
        foreach (var moduleEntry in card.modules)
        {
            if (moduleEntry.moduleReference.RuntimeKeyIsValid())
            {
                var resourceManager = ServiceLocator.Get<ResourceManager>();
                CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(moduleEntry.moduleReference.AssetGUID);
                
                // 방금 발사한 발사체 모듈이 아니고, OnFire 트리거를 가진 모듈만 실행합니다.
                if (module != null && !(module is ProjectileEffectSO) && module.trigger == CardEffectSO.EffectTrigger.OnFire)
                {
                    var effectContextForModule = new EffectContext
                    {
                        Caster = context.Caster,
                        SpawnPoint = context.SpawnPoint,
                        Platform = context.CardInstance.CardData
                    };
                    module.Execute(effectContextForModule);
                }
            }
        }
    }

    private async UniTask FireProjectiles(ProjectileEffectSO pModule, NewCardDataSO platform, CardActionContext context)
    {
        var poolManager = ServiceLocator.Get<PoolManager>();
    
        float baseAngle = GetTargetingAngle(pModule.targetingType, context.Caster.transform, context.SpawnPoint);
        float totalDamage = context.CardInstance.GetFinalDamage() * (1 + context.Caster.FinalDamageBonus / 100f);

        int projectileCount = platform.projectileCount;
        float[] angles = new float[projectileCount];

        if (projectileCount > 0)
        {
            float angleStep = platform.spreadAngle / projectileCount;
            float startAngle = baseAngle - platform.spreadAngle * 0.5f + angleStep * 0.5f;

            for (int i = 0; i < projectileCount; i++)
            {
                angles[i] = startAngle + (angleStep * i);
            }
        }

        foreach (float angle in angles)
        {
            string shotID = Guid.NewGuid().ToString(); 
            
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector2 direction = rotation * Vector2.right;

            GameObject bulletGO = await poolManager.GetAsync(pModule.bulletPrefabReference.AssetGUID);
            if (bulletGO != null && bulletGO.TryGetComponent<BulletController>(out var bullet))
            {
                bullet.transform.position = context.SpawnPoint.position;
                bullet.transform.rotation = rotation;
                bullet.Initialize(direction, platform.baseSpeed * pModule.speed, totalDamage, shotID, platform, pModule, context.Caster, context.CardInstance); // 마지막에 context.CardInstance 추가
            }
        }
    }

    private float GetTargetingAngle(TargetingType targetingType, Transform casterTransform, Transform spawnPoint)
    {
        Transform target = TargetingSystem.FindTarget(targetingType, casterTransform);
        if (target != null)
        { 
            Vector2 directionToTarget = (target.position - spawnPoint.position).normalized;
            return Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        }
        return spawnPoint.eulerAngles.z;
    }
}