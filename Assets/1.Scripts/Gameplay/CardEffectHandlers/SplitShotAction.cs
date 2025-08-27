using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

public class SplitShotAction : ICardAction
{
    public async UniTask Execute(CardActionContext context)
    {
        var card = context.SourceCard;
        if (!card.bulletPrefabRef.RuntimeKeyIsValid()) return;

        string key = card.bulletPrefabRef.AssetGUID;
        float baseAngle = GetTargetingAngle(card.targetingType, context.Caster.transform, context.SpawnPoint);
        int projectileCount = Mathf.Max(1, (int)card.triggerValue);
        float angleStep = 360f / projectileCount;
        float totalDamage = card.baseDamage * (1 + context.Caster.FinalDamageBonus / 100f);
        string shotID = Guid.NewGuid().ToString();

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = baseAngle + (angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 direction = rotation * Vector2.right;

            GameObject bulletGO = await ServiceLocator.Get<PoolManager>().GetAsync(key);
            if (bulletGO == null) continue;

            bulletGO.transform.position = context.SpawnPoint.position;
            bulletGO.transform.rotation = rotation;

            if (bulletGO.TryGetComponent<BulletController>(out var bullet))
            {
                bullet.Initialize(direction, card.bulletSpeed, totalDamage, shotID, card, card.bulletPierceCount, context.Caster);
            }
        }
    }

    private float GetTargetingAngle(TargetingType type, Transform caster, Transform spawnPoint)
    {
        Transform target = TargetingSystem.FindTarget(type, caster);
        if (target != null)
        {
            Vector2 dir = (target.position - spawnPoint.position).normalized;
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        return caster.eulerAngles.z;
    }
}