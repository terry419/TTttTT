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
    public string iconPath; // Sprite를 대체할 리소스 경로
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

    public string effectType; // CardEffectType enum을 string으로 매핑
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
    public string iconPath; // Sprite를 대체할 리소스 경로
    public string rarity; // CardRarity enum을 string으로 매핑

    public float attackBoostRatio;
    public float healthBoostRatio;
    public float moveSpeedBoostRatio;
    public float critChanceBoostRatio;
    public float critDamageBoostRatio;
    public float lifestealBoostRatio;
    
    // ArtifactDataSO에 추가된 필드들
    public int ownedCardSlotBonus;
    public float specificCardTriggerChanceBonus;
}

// ProgressionManager의 영구 데이터를 저장하기 위한 클래스
[Serializable]
public class ProgressionData
{
    public int knowledgeShards;
    public int genePoints;

    // JsonUtility는 Dictionary를 직접 직렬화할 수 없으므로 List 두 개로 변환하여 저장합니다.
    public List<string> achievementIDs = new List<string>();
    public List<bool> achievementStates = new List<bool>();

    public List<string> bossKillIDs = new List<string>();
    public List<bool> bossKillStates = new List<bool>();

    // 캐릭터별 영구 스탯 데이터
    public List<CharacterPermanentStats> characterPermanentStats = new List<CharacterPermanentStats>();
}
