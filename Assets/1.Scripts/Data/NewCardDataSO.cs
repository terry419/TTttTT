// 추천 경로: Assets/1.Scripts/Data/NewCardDataSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;

/// <summary>
/// v8.0 아키텍처의 핵심이 되는 '플랫폼' ScriptableObject입니다.
/// 카드의 기본 발사 사양, 패시브 스탯, 그리고 조립될 기능 모듈(부품)들의 슬롯을 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "NewCard_", menuName = "GameData/v8.0/New Card Platform")]
public class NewCardDataSO : ScriptableObject
{
    [Header("[1] 기본 정보 (UI 표시용)")]
    public BasicInfo basicInfo;

    [Header("[2] 패시브 능력치 (장착 시 적용)")]
    public StatModifiers statModifiers;

    [Header("[3] 플랫폼 기본 발사 사양")]
    [Tooltip("한 번에 발사하는 투사체의 총 개수입니다.")]
    public int projectileCount = 1;
    [Tooltip("투사체가 퍼지는 총 각도입니다. 0이면 모든 투사체가 같은 방향으로 나갑니다.")]
    public float spreadAngle = 0f;
    [Tooltip("이 카드가 사용하는 모든 프리팹을 몇 개씩 미리 로드할지에 대한 권장 수량입니다.")]
    public int preloadCount = 10;
    [Tooltip("이 카드가 발사하는 모든 효과의 기본 피해량입니다.")]
    public float baseDamage = 10f;
    [Tooltip("이 카드가 발사하는 기본 투사체의 속도입니다.")]
    public float baseSpeed = 10f;


    [Header("[4] 모듈 조립 슬롯 (기능 부품)")]
    [Tooltip("이 플랫폼에 장착할 기능 모듈(CardEffectSO)들을 여기에 등록합니다.")]
    public List<ModuleEntry> modules;


    [Header("[5] 메타 정보")]
    [Tooltip("게임 플레이 중 룰렛 등에서 이 카드가 선택될 확률 가중치입니다.")]
    public float selectionWeight = 1f;
    [Tooltip("라운드 종료 후 보상으로 등장할 확률 가중치입니다.")]
    public float rewardAppearanceWeight;
    [Tooltip("이 카드를 해금하기 위한 조건입니다. (미구현)")]
    public string unlockCondition;


    /// <summary>
    /// EffectExecutor가 발사에 필요한 기본 사양을 요청할 때 사용하는 헬퍼 메소드입니다.
    /// </summary>
    /// <returns>발사 사양을 담은 FiringSpec 구조체</returns>
    public FiringSpec GetFiringSpecs()
    {
        Log.Print($"[NewCardDataSO] '{basicInfo.cardName}'의 FiringSpec 요청. 기본 피해량: {baseDamage}");
        // 현재는 FiringSpec에 baseDamage만 담지만, 추후 투사체 프리팹 참조 등 확장될 수 있습니다.
        return new FiringSpec
        {
            baseDamage = this.baseDamage
        };
    }
}

/// <summary>
/// NewCardDataSO의 인스펙터에서 모듈을 쉽게 관리하기 위한 Serializable 클래스입니다.
/// </summary>
[Serializable]
public class ModuleEntry
{
    [Tooltip("인스펙터에서 이 모듈의 역할을 쉽게 알아볼 수 있도록 설명을 기입하세요.")]
    public string description;

    [Tooltip("실제 기능 로직을 담고 있는 CardEffectSO 에셋을 여기에 연결합니다.")]
    public AssetReferenceT<CardEffectSO> moduleReference;
}