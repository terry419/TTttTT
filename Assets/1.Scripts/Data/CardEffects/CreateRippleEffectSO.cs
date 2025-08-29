// ���: Assets/1.Scripts/Data/CardEffects/CreateRippleEffectSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "Module_Ripple_", menuName = "GameData/v8.0/Modules/CreateRippleEffect")]
public class CreateRippleEffectSO : CardEffectSO, IPreloadable
{
    [Header("�ĵ� ����")]
    [Tooltip("�ĵ� ȿ���� ��Ÿ�� ������ (�ݵ�� RippleController.cs�� �����ؾ� ��)")]
    public AssetReferenceGameObject ripplePrefabRef;
    [Tooltip("�ĵ��� ���������� �ִ� �ݰ�")]
    public float maxRadius = 10f;
    [Tooltip("�ִ� �ݰ���� �����ϴ� �� �ɸ��� �ð�")]
    public float expandDuration = 0.5f;


    public CreateRippleEffectSO()
    {
        // �� ����� ī�� �߻� �� ��� ȿ���� �����մϴ�.
        trigger = EffectTrigger.OnFire;
    }

    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ����.");
        // �񵿱� ������ ���� UniTask ���
        _ = CreateRippleAsync(context);
    }

    private async UniTaskVoid CreateRippleAsync(EffectContext context)
    {
        if (!ripplePrefabRef.RuntimeKeyIsValid() || context.Caster == null) return;

        var poolManager = ServiceLocator.Get<PoolManager>();
        GameObject rippleGO = await poolManager.GetAsync(ripplePrefabRef.AssetGUID);

        if (rippleGO != null && rippleGO.TryGetComponent<RippleController>(out var rippleController))
        {
            // 1. ����� �⺻ ������ ����
            float baseDamageToUse = (context.BaseDamageOverride > 0)
                ? context.BaseDamageOverride
                : context.Platform.baseDamage;

            // 2. ī�� ��ȭ ���� ����
            int enhancementLevel = context.SourceCardInstance?.EnhancementLevel ?? 0;
            float enhancedBaseDamage = baseDamageToUse * (1f + enhancementLevel * 0.1f);

            // 3. �÷��̾� ���� ����("����ġ") ����
            float finalDamage = enhancedBaseDamage * (1 + context.Caster.FinalDamageBonus / 100f);

            rippleController.transform.position = context.Caster.transform.position;
            rippleController.Initialize(context.Caster, maxRadius, expandDuration, finalDamage);
        }
    }
    // IPreloadable �������̽� ����
    public IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload()
    {
        if (ripplePrefabRef != null && ripplePrefabRef.RuntimeKeyIsValid())
            yield return ripplePrefabRef;
    }
}
