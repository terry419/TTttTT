using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardInstance
{
    public string InstanceId;
    public NewCardDataSO CardData;
    public int EnhancementLevel;

    [NonSerialized]
    private Dictionary<string, float> _modifiedValues = new Dictionary<string, float>();

    public CardInstance(NewCardDataSO cardData)
    {
        InstanceId = Guid.NewGuid().ToString();
        CardData = cardData;
        EnhancementLevel = 0;
    }

    // 강화 레벨을 고려하여 최종 스탯 보정치를 계산하는 헬퍼 메서드
    private float GetEnhancedValue(float baseValue)
    {
        if (EnhancementLevel == 0) return baseValue;

        // [사용자 제안] 절대값의 10%를 더하는 방식으로 강화 로직 통일
        float enhancementAmount = Mathf.Abs(baseValue) * 0.1f * EnhancementLevel;
        return baseValue + enhancementAmount;
    }

    // 각 스탯에 대한 최종 값을 반환하는 public 메서드들
    public float GetFinalDamageMultiplier() => GetEnhancedValue(CardData.statModifiers.damageMultiplier);
    public float GetFinalAttackSpeedMultiplier() => GetEnhancedValue(CardData.statModifiers.attackSpeedMultiplier);
    public float GetFinalMoveSpeedMultiplier() => GetEnhancedValue(CardData.statModifiers.moveSpeedMultiplier);
    public float GetFinalHealthMultiplier() => GetEnhancedValue(CardData.statModifiers.healthMultiplier);
    public float GetFinalCritRateMultiplier() => GetEnhancedValue(CardData.statModifiers.critRateMultiplier);
    public float GetFinalCritDamageMultiplier() => GetEnhancedValue(CardData.statModifiers.critDamageMultiplier);

    // 기본 데미지는 별도 계산
    public float GetFinalDamage()
    {
        float finalDamage = CardData.baseDamage * (1f + EnhancementLevel * 0.1f);
        if (_modifiedValues.TryGetValue("Damage", out float modifier))
        {
            finalDamage += modifier;
        }
        return finalDamage;
    }
}
