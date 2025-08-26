using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

/// <summary>
/// 지정된 여러 효과 모듈 중 하나를 무작위로 선택하여 실행하는 논리 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Random_", menuName = "GameData/v8.0/Modules/RandomEffect")]
public class RandomEffectSO : CardEffectSO
{
    [Header("랜덤 효과 풀")]
    [Tooltip("무작위로 선택될 효과 모듈 에셋의 목록입니다.")]
    public List<AssetReferenceT<CardEffectSO>> effectPool;

    /// <summary>
    /// effectPool에서 무작위로 모듈 하나를 골라 EffectExecutor에게 실행을 위임합니다.
    /// 이 모듈의 실제 실행은 EffectExecutor에서 처리해야 합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행. 효과 풀 개수: {effectPool?.Count ?? 0}개.");
        // 이 모듈은 데이터를 제공하고, 실제 실행은 EffectExecutor가 담당합니다.
        // EffectExecutor는 이 모듈을 만나면, effectPool 중 하나를 골라 다시 Execute를 호출해야 합니다.
    }
}