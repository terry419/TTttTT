using Cysharp.Threading.Tasks;
using UnityEngine;
using System;
using System.Linq;

public class ModuleAction : ICardAction
{
    public async UniTask Execute(CardActionContext context)
    {
        var card = context.CardInstance.CardData;

        // [ٽ ]  ModuleAction  ߻  մϴ.
        // ProjectileEffectSO  ãƼ ߻  ɴϴ.
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
                    break; // ù ° ProjectileEffectSO  
                }
            }
        }

        if (pModule == null || !pModule.bulletPrefabReference.RuntimeKeyIsValid())
        {
            Debug.LogWarning($"[{card.name}] ߻ ProjectileEffectSO  ã  ϴ.");
            // ü  ٸ OnFire ⸸   Ƿ ⼭ return ʽϴ.
        }
        else
        {
            // pModule ã, ī ÷ ߻  Ͽ ü ߻մϴ.
            await FireProjectiles(pModule, card, context);
        }

        // OnFire Ÿ ٸ   ( )
        foreach (var moduleEntry in card.modules)
        {
            if (moduleEntry.moduleReference.RuntimeKeyIsValid())
            {
                var resourceManager = ServiceLocator.Get<ResourceManager>();
                CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(moduleEntry.moduleReference.AssetGUID);
                // ü  ƴϰ, OnFire ƮŸ  ⸸ 
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
            // [수정] 사용자의 분석이 맞았습니다. N-1이 아닌 N으로 나누어 각도 간격을 계산해야 합니다.
            // 이렇게 하면 360도 분할 시 첫 발과 마지막 발이 겹치는 문제가 해결됩니다.
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
                bullet.Initialize(direction, platform.baseSpeed * pModule.speed, totalDamage, shotID, platform, pModule, context.Caster);
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