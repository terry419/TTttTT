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
    [Tooltip("이 장판이 가하는 맥동의 기본 피해량입니다.")]
    public float pulseDamage = 15f;

    [Tooltip("체크 시, 플레이어의 최종 공격력 보너스(%)가 이 장판의 피해량에 영향을 줍니다.")]
    public bool scalesWithPlayerDamageBonus = true;


    [Tooltip("최소 크기 비율입니다. (예: 0.2는 최대 반경의 20%까지 작아짐)")]
    [Range(0f, 1f)]
    public float MinPulseScaleRatio = 0.2f;

    [Tooltip("끌어당김과 피해가 발생하는 주기(초)입니다. (권장: 0.5)")]
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
            Vector3 spawnPosition = context.HitPosition;
            zoneGO.transform.position = spawnPosition;

            float finalDamage = this.pulseDamage;
            if (scalesWithPlayerDamageBonus)
            {
                finalDamage *= (1 + context.Caster.FinalDamageBonus / 100f);
            }

            zoneController.Initialize(ZoneDuration, PullRadius, PullForce, finalDamage, MinPulseScaleRatio, DamageTickInterval);
        }
    }
}