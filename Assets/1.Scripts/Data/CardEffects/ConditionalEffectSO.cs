using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 특정 조건이 충족되었을 때만 연결된 효과를 발동시키는 특수 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Conditional_", menuName = "GameData/CardData/Modules/ConditionalEffect")]
public class ConditionalEffectSO : CardEffectSO
{
    [Header("조건부 발동 설정")]
    [Tooltip("이 효과를 발동시킬 트리거 조건입니다. (예: OnCrit)")]
    public EffectTrigger condition;

    [Tooltip("조건이 충족되었을 때 발동할 효과입니다.")]
    public AssetReferenceT<CardEffectSO> effectToTrigger;

    /// <summary>
    /// 조건부 효과 자체는 직접적인 로직을 실행하지 않습니다.
    /// EffectExecutor가 이 모듈의 존재를 인지하고, 'condition'이 충족되었을 때 'effectToTrigger'를 대신 실행해야 합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행. 조건: {condition}.");
        // 이 모듈은 데이터를 전달하는 역할만 합니다.
    }
}