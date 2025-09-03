// 파일 경로: Assets/1.Scripts/Data/NewCardDataSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewCard_", menuName = "GameData/v8.0/New Card Platform")]
public class NewCardDataSO : ScriptableObject
{
    [Header("[1] 기본 정보 (UI 표기용)")]
    public BasicInfo basicInfo;

    [Header("[2] 스탯 보너스 (플레이어 강화)")]
    public StatModifiers statModifiers;

    [Header("[3] 카드 고유 발사 스펙")]
    [Tooltip("이 카드의 고유 공격 주기(초)입니다. 1.0은 1초에 한 번, 0.2는 1초에 5번을 의미합니다.")]
    public float attackInterval = 1.0f; 

    [Tooltip("체크 시, 동일 투사체가 한 몬스터를 여러 번 타격할 수 있습니다.")]
    public bool allowMultipleHits = false;

    [Tooltip("한 번에 발사하는 투사체의 개수입니다.")]
    public int projectileCount = 1;

    [Tooltip("투사체가 퍼지는 각도입니다. 0이면 모든 투사체가 한 방향으로 나갑니다.")]
    public float spreadAngle = 0f;

    [Tooltip("이 카드가 사용하는 투사체를 미리 몇 개 생성해둘지 정합니다.")]
    public int preloadCount = 10;

    [Tooltip("카드가 발사하는 투사체의 기본 피해량입니다.")]
    public float baseDamage = 10f;

    [Tooltip("카드가 발사하는 투사체의 기본 속도입니다.")]
    public float baseSpeed = 10f;

    [Header("[4] 장착 효과 (모듈)")]
    [Tooltip("이 카드에 장착할 특수 효과(CardEffectSO) 목록입니다.")]
    public List<ModuleEntry> modules;

    [Header("[5] 기타 메타 정보")]
    [Tooltip("카드 선택지에 이 카드가 등장할 확률 가중치입니다.")]
    public float selectionWeight = 1f;

    [Tooltip("보상 목록에 이 카드가 등장할 확률 가중치입니다.")]
    public float rewardAppearanceWeight;

    [Tooltip("이 카드를 해금하기 위한 조건입니다. (미구현)")]
    public string unlockCondition;

    public ICardAction CreateAction()
    {
        return new ModuleAction();
    }
}

[Serializable]
public class ModuleEntry
{
    [Tooltip("이 모듈에 대한 설명입니다.")]
    public string description;

    [Tooltip("실행할 CardEffectSO 모듈을 여기에 연결합니다.")]
    public AssetReferenceT<CardEffectSO> moduleReference;
}