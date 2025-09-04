// 경로: Assets/1.Scripts/Data/CardEffects/CreateRippleEffectSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "Module_Ripple_", menuName = "GameData/CardData/Modules/CreateRippleEffect")]
public class CreateRippleEffectSO : CardEffectSO, IPreloadable
{
    [Header("파동 설정")]
    [Tooltip("파동 효과를 나타낼 프리팹 (반드시 RippleController.cs를 포함해야 함)")]
    public AssetReferenceGameObject ripplePrefabRef;
    [Tooltip("파동이 퍼져나가는 최대 반경")]
    public float maxRadius = 10f;
    [Tooltip("최대 반경까지 도달하는 데 걸리는 시간")]
    public float expandDuration = 0.5f;


    public CreateRippleEffectSO()
    {
        // 이 효과는 카드 발사 시 한 번 효과를 생성합니다.
        trigger = EffectTrigger.OnFire;
    }

    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행.");
        // 비동기 생성을 위해 UniTask 사용
        _ = CreateRippleAsync(context);
    }

    private async UniTaskVoid CreateRippleAsync(EffectContext context)
    {
        if (!ripplePrefabRef.RuntimeKeyIsValid() || context.Caster == null) return;

        var poolManager = ServiceLocator.Get<PoolManager>();
        GameObject rippleGO = await poolManager.GetAsync(ripplePrefabRef.AssetGUID);

        if (rippleGO != null && rippleGO.TryGetComponent<RippleController>(out var rippleController))
        {
            // 1. 시전자의 기본 데미지 계산
            float baseDamageToUse = (context.BaseDamageOverride > 0)
                ? context.BaseDamageOverride
                : context.Platform.baseDamage;

            // 2. 카드 강화 레벨 적용
            int enhancementLevel = context.SourceCardInstance?.EnhancementLevel ?? 0;
            float enhancedBaseDamage = baseDamageToUse * (1f + enhancementLevel * 0.1f);

            // 3. 플레이어 스탯 보너스("합연산") 적용
            float finalDamage = enhancedBaseDamage * (1 + context.Caster.FinalDamageBonus / 100f);

            rippleController.transform.position = context.Caster.transform.position;
            rippleController.Initialize(context.Caster, maxRadius, expandDuration, finalDamage);
        }
    }
    // IPreloadable 인터페이스 구현
    public IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload()
    {
        if (ripplePrefabRef != null && ripplePrefabRef.RuntimeKeyIsValid())
            yield return ripplePrefabRef;
    }
}