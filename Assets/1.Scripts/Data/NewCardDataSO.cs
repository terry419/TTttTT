// 경로: Assets/1.Scripts/Data/NewCardDataSO.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewCard_", menuName = "GameData/CardData/New Card Platform")]
public class NewCardDataSO : ScriptableObject
{
    [Header("[1] 기본 정보 (UI 표시용)")]
    public BasicInfo basicInfo;

    [Header("[2] 스탯 수식어 (플레이어 강화)")]
    public StatModifiers statModifiers;

    [Header("[3] 카드 효과 발동 설정")]
    [Tooltip("이 카드의 공격 발동 주기(초)입니다. 1.0은 1초에 한 번, 0.2는 1초에 5번을 의미합니다.")]
    public float attackInterval = 1.0f; 

    [Tooltip("체크 시, 하나의 발사체가 여러 몬스터를 관통하여 타격할 수 있습니다.")]
    public bool allowMultipleHits = false;

    [Tooltip("한 번에 발사하는 발사체의 수량입니다.")]
    public int projectileCount = 1;

    [Tooltip("발사체들이 퍼지는 각도입니다. 0이면 모든 발사체가 한 방향으로 나갑니다.")]
    public float spreadAngle = 0f;

    [Tooltip("이 카드가 사용하는 발사체를 미리 몇 개 생성해둘지 정합니다.")]
    public int preloadCount = 10;

    [Tooltip("카드가 발사하는 발사체의 기본 데미지입니다.")]
    public float baseDamage = 10f;

    [Tooltip("카드가 발사하는 발사체의 기본 속도입니다.")]
    public float baseSpeed = 10f;

    [Header("[4] 특수 효과 (모듈)")]
    [Tooltip("이 카드에 연결된 특수 효과(CardEffectSO) 목록입니다.")]
    public List<ModuleEntry> modules;

    [Header("[5] 기타 메타 데이터")]
    [Tooltip("카드 선택지에서 이 카드가 등장할 확률 가중치입니다.")]
    public float selectionWeight = 1f;

    [Tooltip("보상 목록에서 이 카드가 등장할 확률 가중치입니다.")]
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

    [Tooltip("연결할 CardEffectSO 에셋을 여기에 할당합니다.")]
    public AssetReferenceT<CardEffectSO> moduleReference;
}