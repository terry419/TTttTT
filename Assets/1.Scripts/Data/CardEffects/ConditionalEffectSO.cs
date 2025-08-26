using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 특정 조건이 만족되었을 때만 연결된 효과를 발동시키는 논리 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Conditional_", menuName = "GameData/v8.0/Modules/ConditionalEffect")]
public class ConditionalEffectSO : CardEffectSO
{
    [Header("조건부 실행 설정")]
    [Tooltip("이 모듈이 반응할 트리거 조건입니다. (예: OnCrit)")]
    public EffectTrigger condition;

    [Tooltip("조건이 만족되었을 때 실행될 효과 모듈입니다.")]
    public AssetReferenceT<CardEffectSO> effectToTrigger;

    /// <summary>
    /// 조건부 모듈 자체는 직접적인 로직을 실행하지 않습니다.
    /// EffectExecutor가 이 모듈의 존재를 인지하고, 'condition'이 만족되었을 때 'effectToTrigger'를 대신 실행시켜줘야 합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행. 조건: {condition}.");
        // 이 모듈은 데이터를 제공하는 역할만 합니다.
    }
}