using UnityEngine;
using System;
using System.Collections.Generic;

// ▼▼▼ 이 내부 클래스들을 먼저 정의합니다. ▼▼▼

[Serializable]
public class BasicInfo
{
    [Tooltip("카드의 고유 ID (예: warrior_basic_001)")]
    public string cardID;
    [Tooltip("UI에 표시될 카드 이름")]
    public string cardName;
    [Tooltip("UI에 표시될 카드 아이콘")]
    public Sprite cardIcon;
    [Tooltip("카드의 타입 (물리 또는 마법)")]
    public CardType type;
    [Tooltip("카드의 희귀도")]
    public CardRarity rarity;
    [Tooltip("카드 효과 설명 텍스트"), TextArea(3, 5)]
    public string effectDescription;
}

[Serializable]
public class StatModifiers
{
    [Tooltip("공격력에 더해지는 % 배율")]
    public float damageMultiplier;
    [Tooltip("공격 속도에 더해지는 % 배율")]
    public float attackSpeedMultiplier;
    [Tooltip("이동 속도에 더해지는 % 배율")]
    public float moveSpeedMultiplier;
    [Tooltip("최대 체력에 더해지는 % 배율")]
    public float healthMultiplier;
    [Tooltip("치명타 확률에 더해지는 % 배율")]
    public float critRateMultiplier;
    [Tooltip("치명타 피해량에 더해지는 % 배율")]
    public float critDamageMultiplier;
}

// ▲▲▲ 내부 클래스 정의 끝 ▲▲▲


[CreateAssetMenu(fileName = "CardData_", menuName = "GameData/CardData")]
public class CardDataSO : ScriptableObject
{
    [Header("1. 기본 정보")]
    public BasicInfo basicInfo;

    [Header("2. 패시브 능력치")]
    public StatModifiers statModifiers;
    
    [Header("3. 옵션 부품 조립 슬롯")]
    [Tooltip("이 카드에 장착할 '옵션(CardEffectSO)' 에셋들의 Addressable ID 목록")]
    public List<string> attachedEffectIDs;

    [Header("4. 메타 정보")]
    [Tooltip("룰렛에서 선택될 확률 가중치")]
    public float selectionWeight = 1f;
    [Tooltip("카드 보상으로 등장할 확률 가중치")]
    public float rewardAppearanceWeight;
    [Tooltip("카드 해금 조건")]
    public string unlockCondition;
}