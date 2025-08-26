using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 파동, 장판 등 광역 효과를 생성하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Area_", menuName = "GameData/v8.0/Modules/AreaEffect")]
public class AreaEffectSO : CardEffectSO
{
    [Header("광역 효과 프리팹")]
    [Tooltip("생성할 파동 또는 장판 효과의 프리팹 (DamagingZone.cs 포함)")]
    public AssetReferenceGameObject effectPrefabRef;

    [Header("파동/장판 효과 설정")]
    [Tooltip("파동/장판의 총 지속 시간 (초)")]
    public float effectDuration = 3f;

    [Tooltip("파동/장판의 확장 속도 (초당)")]
    public float effectExpansionSpeed = 1f;

    [Tooltip("파동/장판이 확장하는 시간 (초)")]
    public float effectExpansionDuration = 3.1f;

    [Tooltip("장판 모드일 때 틱 데미지 간격 (초)")]
    public float effectTickInterval = 100.0f;

    [Tooltip("장판 모드일 때 틱 당 피해량. 0보다 크면 장판, 0이면 단일 타격 파동으로 간주됩니다.")]
    public float damagePerTick = 0f;

    /// <summary>
    /// AreaEffect의 로직을 실행하여 프리팹을 생성하고 초기화합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행.");

        // 이 모듈은 현재 Execute 단계에서 직접 실행할 로직이 없습니다.
        // 프리팹 생성 및 초기화는 EffectExecutor가 담당하게 됩니다.
    }
}