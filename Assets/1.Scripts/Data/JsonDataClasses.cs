using System;
using System.Collections.Generic;
using UnityEngine; // For Sprite, though not directly used in JSON deserialization, useful for context

// 전체 게임 데이터를 담을 JSON 구조의 루트 클래스
[Serializable]
public class GameDataJson
{
    public List<CardDataJson> cards;
    public List<ArtifactDataJson> artifacts;
}

// CardDataSO와 필드를 일치시키는 JSON용 클래스
[Serializable]
public class CardDataJson
{
    public string cardID;
    public string cardName;
    public string type; // CardType enum을 string으로 매핑
    public string rarity; // CardRarity enum을 string으로 매핑

    public float damageMultiplier;
    public float attackSpeedMultiplier;
    public float moveSpeedMultiplier;
    public float healthMultiplier;
    public float critRateMultiplier;
    public float critDamageMultiplier;
    public float lifestealPercentage;
    public string effectDescription;

    public string triggerType; // TriggerType enum을 string으로 매핑

    public float rewardAppearanceWeight;
    public string unlockCondition;
}

// ArtifactDataSO와 필드를 일치시키는 JSON용 클래스
[Serializable]
public class ArtifactDataJson
{
    public string artifactID;
    public string artifactName;
    public string rarity; // CardRarity enum을 string으로 매핑

    public float attackBoostRatio;
    public float healthBoostRatio;
    public float moveSpeedBoostRatio;
    public float critChanceBoostRatio;
    public float critDamageBoostRatio;
    public float lifestealBoostRatio;
    // TODO: project_plan.md의 유물 시스템 섹션에 있는 구체적인 유물 효과들을 반영하여 필드 추가
    // 예: 소유 카드 슬롯 증가, 장착 카드 사용 확률 증가, 상점 가격 변경 등
}
