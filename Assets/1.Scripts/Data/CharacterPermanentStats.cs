using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 유전자 증폭제 강화를 통해 얻는 영구적인 능력치 종류를 정의합니다.
/// </summary>
public enum StatType { Attack, AttackSpeed, MoveSpeed, Health, CritMultiplier }

/// <summary>
/// 캐릭터 한 명의 영구 스탯 정보(해금 여부, 투자된 비율)를 담고 관리하는 클래스입니다.
/// 이 클래스의 인스턴스는 파일로 저장되고 로드되어 영속성을 가집니다.
/// </summary>
[System.Serializable] // 파일 저장을 위해 직렬화 가능하도록 설정
public class CharacterPermanentStats
{
    public string characterId;
    
    // 각 스탯의 해금 여부를 저장합니다.
    public Dictionary<StatType, bool> unlockedStatus = new Dictionary<StatType, bool>();
    
    // 각 스탯에 투자된 포인트로 인해 증가한 비율 값을 저장합니다.
    public Dictionary<StatType, float> investedRatios = new Dictionary<StatType, float>();

    /// <summary>
    /// 새 캐릭터를 위한 기본 생성자입니다.
    /// </summary>
    public CharacterPermanentStats(string charId)
    {
        characterId = charId;
        // 모든 StatType에 대해 초기값을 설정합니다.
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            // 기획서: 체력은 기본적으로 해금
            unlockedStatus[type] = (type == StatType.Health);
            investedRatios[type] = 0f;
        }
    }

    /// <summary>
    /// 모든 스탯이 해금되었는지 확인합니다.
    /// </summary>
    public bool AllStatsUnlocked()
    {
        return unlockedStatus.Values.All(unlocked => unlocked);
    }

    /// <summary>
    /// 아직 해금되지 않은 스탯 목록을 반환합니다.
    /// </summary>
    public List<StatType> GetLockedStats()
    {
        return unlockedStatus.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
    }

    /// <summary>
    /// 지정된 스탯의 해금 상태를 true로 변경합니다.
    /// </summary>
    public void UnlockStat(StatType stat)
    {
        if (unlockedStatus.ContainsKey(stat))
        {
            unlockedStatus[stat] = true;
        }
    }

    /// <summary>
    /// 투자된 포인트를 해금된 능력치들에 랜덤하게 배분합니다.
    /// </summary>
    public void DistributePoints(int points)
    {
        List<StatType> unlockedStats = unlockedStatus.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        if (unlockedStats.Count == 0) return;

        for (int i = 0; i < points; i++)
        {
            // 해금된 스탯 중 하나를 랜덤하게 골라 가중치를 더합니다.
            StatType targetStat = unlockedStats[Random.Range(0, unlockedStats.Count)];
            float weight = GetWeightForStat(targetStat);
            investedRatios[targetStat] += weight;
        }
    }

    /// <summary>
    /// 기획서에 명시된 스탯별 1포인트당 가중치를 반환합니다.
    /// </summary>
    private float GetWeightForStat(StatType stat)
    {
        switch (stat)
        {
            case StatType.Health:
                return 0.02f;
            case StatType.Attack:
            case StatType.AttackSpeed:
            case StatType.MoveSpeed:
            case StatType.CritMultiplier:
            default:
                return 0.01f;
        }
    }
}
