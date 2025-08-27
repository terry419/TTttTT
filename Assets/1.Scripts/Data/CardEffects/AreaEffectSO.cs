// 경로: ./TTttTT/Assets/1.Scripts/Data/CardEffects/AreaEffectSO.cs

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 파동, 장판 등 광역 효과를 생성하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Area_", menuName = "GameData/v8.0/Modules/AreaEffect")]
public class AreaEffectSO : CardEffectSO, IPreloadable
{
    [Header("광역 효과 프리팹")]
    [Tooltip("생성할 파동 또는 장판 효과의 프리팹 (DamagingZone.cs 포함)")]
    public AssetReferenceGameObject effectPrefabRef;

    [Header("모드 설정")]
    [Tooltip("체크 시: 단일 피해 파동 모드 / 해제 시: 지속 피해 장판 모드")]
    public bool isSingleHitWaveMode = true;

    [Header("파동/장판 공통 설정")]
    [Tooltip("d: 파동/장판의 총 지속 시간 (초)")]
    public float effectDuration = 3f;
    [Tooltip("a: 파동/장판이 도달할 최대 반지름. 이 크기에 도달할 때까지 확장합니다.")]
    public float maxExpansionRadius = 5f;
    [Tooltip("b: 최대 반지름(a)까지 확장하는 데 걸리는 시간(초). 이 시간이 짧을수록 확장 속도가 빠릅니다.")]
    public float effectExpansionDuration = 1f;

    [Header("파동 모드 전용 (isSingleHitWaveMode: true)")]
    [Tooltip("c: 확장하는 파동의 경계에 닿은 적이 입는 단일 피해량. 플레이어의 최종 대미지 보너스가 적용됩니다.")]
    public float singleHitDamage = 100f;

    [Header("장판 모드 전용 (isSingleHitWaveMode: false)")]
    [Tooltip("f: 장판 내의 적들이 입는 틱 당 피해량. 플레이어의 최종 대미지 보너스가 적용됩니다.")]
    public float damagePerTick = 25f;
    [Tooltip("e: 장판의 틱 데미지가 들어가는 간격(초)")]
    public float effectTickInterval = 0.25f;

    /// <summary>
    /// AreaEffect의 로직을 실행하여 프리팹을 생성하고 초기화합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행.");
        // 이 모듈은 현재 Execute 단계에서 직접 실행할 로직이 없습니다.
        // 프리팹 생성 및 초기화는 EffectExecutor가 담당하게 됩니다.
    }

    public IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload()
    {
        if (effectPrefabRef != null && effectPrefabRef.RuntimeKeyIsValid())
            yield return effectPrefabRef;
    }
}