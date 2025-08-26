// 추천 경로: Assets/1.Scripts/Data/FiringSpec.cs
using UnityEngine.AddressableAssets;
using UnityEngine;

/// <summary>
/// NewCardDataSO에서 EffectExecutor로, 최종적으로 BulletController로 전달될
/// 투사체의 핵심 발사 사양을 정의하는 데이터 구조체입니다.
/// </summary>
public struct FiringSpec
{
    public float baseDamage;                    // 플랫폼에서 계산된 기본 피해량
    public AssetReferenceGameObject projectilePrefabRef; // 사용할 투사체 프리팹의 Addressable 참조
    // 이 외에도 투사체 속도, 크기 등 필요한 모든 사양을 여기에 추가할 수 있습니다.
}