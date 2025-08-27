using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

public class SingleShotAction : ICardAction
{
    public async UniTask Execute(CardActionContext context)
    {
        var card = context.SourceCard;
        Debug.Log($"[{GetType().Name}-DEBUG] '{card.name}' ���� ����.");

        if (!card.bulletPrefabRef.RuntimeKeyIsValid())
        {
            Debug.LogError($"[{GetType().Name}-DEBUG] '{card.name}'�� bulletPrefabRef�� ��ȿ���� �ʽ��ϴ�.");
            return;
        }

        string key = card.bulletPrefabRef.AssetGUID;
        Debug.Log($"[{GetType().Name}-DEBUG] PoolManager.GetAsync ȣ�� ����. Key: {key}");


        float angle = GetTargetingAngle(card.targetingType, context.Caster.transform, context.SpawnPoint);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector2 direction = rotation * Vector2.right;

        GameObject bulletGO = await ServiceLocator.Get<PoolManager>().GetAsync(key);
        if (bulletGO == null)
        {
            Debug.LogError($"[{GetType().Name}-DEBUG] PoolManager�κ��� bullet�� �������µ� �����߽��ϴ�. Key: {key}");
            return;
        }
        Debug.Log($"[{GetType().Name}-DEBUG] PoolManager�κ��� bullet �������� ����.");

        bulletGO.transform.position = context.SpawnPoint.position;
        bulletGO.transform.rotation = rotation;

        if (bulletGO.TryGetComponent<BulletController>(out var bullet))
        {
            float totalDamage = card.baseDamage * (1 + context.Caster.FinalDamageBonus / 100f);
            string shotID = Guid.NewGuid().ToString();
            bullet.Initialize(direction, card.bulletSpeed, totalDamage, shotID, card, card.bulletPierceCount, context.Caster);
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