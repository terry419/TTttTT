// 파일 경로: Assets/1.Scripts/Data/CardEffects/CreateGravityPulseZoneEffectSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "Module_CreateGravityPulseZone_", menuName = "GameData/v8.0/Modules/CreateGravityPulseZoneEffect")]
public class CreateGravityPulseZoneEffectSO : CardEffectSO
{
    [Header("--- 장판 공통 설정 ---")]
    [Tooltip("반드시 GravityPulseZoneController.cs 컴포넌트를 가진 '장판' 프리팹이어야 합니다.")]
    public AssetReferenceGameObject ZonePrefabRef;

    [Tooltip("장판 전체가 유지되는 시간 (초).")]
    public float ZoneDuration = 8f;

    [Header("--- 중력 효과 ---")]
    [Tooltip("몬스터를 끌어당기는 효과의 최대 반경입니다.")]
    public float PullRadius = 8f;

    [Tooltip("몬스터를 중심으로 끌어당기는 힘의 크기입니다.")]
    public float PullForce = 150f;

    [Header("--- 맥동 피해 효과 ---")]
    [Tooltip("커졌다 작아지는 속도입니다. 높을수록 빠릅니다.")]
    public float PulseSpeed = 5f;

    [Tooltip("최소 크기 비율입니다. (예: 0.2는 최대 반경의 20%까지 작아짐)")]
    [Range(0f, 1f)]
    public float MinPulseScaleRatio = 0.2f;

    [Tooltip("피해를 주는 주기(초)입니다. (예: 0.5는 1초에 2번 피해)")]
    public float DamageTickInterval = 0.5f;


    public override void Execute(EffectContext context)
    {
        if (ZonePrefabRef == null || !ZonePrefabRef.RuntimeKeyIsValid())
        {
            Debug.LogError($"[{name}] 장판 프리팹이 유효하게 설정되지 않았습니다!");
            return;
        }
        CreateZoneAsync(context).Forget();
    }

    private async UniTaskVoid CreateZoneAsync(EffectContext context)
    {
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        GameObject zoneGO = await poolManager.GetAsync(ZonePrefabRef.AssetGUID);
        if (zoneGO != null && zoneGO.TryGetComponent<GravityPulseZoneController>(out var zoneController))
        {
            // 착탄 위치에 생성
            Vector3 spawnPosition = context.HitPosition;
            zoneGO.transform.position = spawnPosition;

            // 최종 피해량 계산 (카드 강화 레벨, 플레이어 스탯 보너스 적용)
            float baseDamage = context.Platform.baseDamage;
            int enhancementLevel = context.SourceCardInstance?.EnhancementLevel ?? 0;
            float enhancedBaseDamage = baseDamage * (1f + enhancementLevel * 0.1f);
            float finalDamage = enhancedBaseDamage * (1 + context.Caster.FinalDamageBonus / 100f);

            zoneController.Initialize(ZoneDuration, PullRadius, PullForce, finalDamage, PulseSpeed, MinPulseScaleRatio, DamageTickInterval);
        }
    }
}