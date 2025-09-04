using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

/// <summary>
/// 연결된 여러 효과 중 하나를 무작위로 선택하여 실행하는 특수 효과입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Random_", menuName = "GameData/CardData/Modules/RandomEffect")]
public class RandomEffectSO : CardEffectSO
{
    [Header("랜덤 효과 풀")]
    [Tooltip("무작위로 선택될 효과 목록의 후보들입니다.")]
    public List<AssetReferenceT<CardEffectSO>> effectPool;

    /// <summary>
    /// effectPool에서 무작위로 효과 하나를 골라 EffectExecutor에게 실행을 위임합니다.
    /// 이 로직의 실제 구현은 EffectExecutor에서 처리해야 합니다.
    /// </summary>
    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행. 효과 풀 크기: {effectPool?.Count ?? 0}개.");
        // 이 로직의 구현을 생략하고, 실제 실행은 EffectExecutor에 위임합니다.
        // EffectExecutor는 이 정보를 바탕으로, effectPool 내 하나를 골라 다시 Execute를 호출해야 합니다.
    }
}