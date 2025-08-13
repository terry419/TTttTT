using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// StatType 열거형은 Enums.cs 파일로 이동/통합하는 것을 권장합니다.
// public enum StatType { ... } 

[System.Serializable]
public class CharacterPermanentStats
{
    public string characterId;

    // ✨ [1번 문제 해결] Inspector에서 테스트를 위해 해금 상태를 쉽게 제어할 수 있도록 리스트로 변경
    [Tooltip("테스트 시 해금할 스탯을 여기에 추가하세요. 실제 게임에서는 룰렛을 통해 해금됩니다.")]
    public List<StatType> defaultUnlockedStats = new List<StatType> { StatType.Health, StatType.Attack, StatType.AttackSpeed, StatType.MoveSpeed, StatType.CritMultiplier, StatType.CritRate };

    public Dictionary<StatType, bool> unlockedStatus = new Dictionary<StatType, bool>();
    public Dictionary<StatType, float> investedRatios = new Dictionary<StatType, float>();

    // JSON 저장을 위한 헬퍼 프로퍼티 (기존 로직 유지)
    public StatDictionaryData statData
    {
        get
        {
            // ... 기존 get 로직 ...
            StatDictionaryData data = new StatDictionaryData();
            foreach (var kvp in unlockedStatus)
            {
                data.statTypes.Add(kvp.Key);
                data.unlockedStatuses.Add(kvp.Value);
            }
            foreach (var kvp in investedRatios)
            {
                data.investedRatios.Add(kvp.Value);
            }
            return data;
        }
        set
        {
            // ... 기존 set 로직 ...
            unlockedStatus.Clear();
            investedRatios.Clear();
            if (value != null)
            {
                for (int i = 0; i < value.statTypes.Count; i++)
                {
                    unlockedStatus[value.statTypes[i]] = value.unlockedStatuses[i];
                    investedRatios[value.statTypes[i]] = value.investedRatios[i];
                }
            }
        }
    }

    public CharacterPermanentStats(string charId)
    {
        characterId = charId;

        // 모든 스탯을 일단 '잠금' 상태로 초기화
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            unlockedStatus[type] = false;
            investedRatios[type] = 0f;
        }

        // ✨ defaultUnlockedStats 리스트에 있는 스탯들만 '해금' 상태로 변경
        foreach (StatType type in defaultUnlockedStats)
        {
            unlockedStatus[type] = true;
        }
    }

    /// <summary>
    /// 아직 해금되지 않은 스탯 목록을 반환합니다.
    /// </summary>
    public List<StatType> GetLockedStats()
    {
        return unlockedStatus.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
    }

    /// <summary>
    /// ✨ [오류 3 해결] 해금된 스탯 목록 전체를 반환하는 public 메서드입니다.
    /// </summary>
    public List<StatType> GetUnlockedStats()
    {
        return unlockedStatus.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

    }

    // --- 나머지 메서드들은 기존 로직과 동일 ---

    public bool AllStatsUnlocked()
    {
        return unlockedStatus.Values.All(unlocked => unlocked);
    }

    public void UnlockStat(StatType stat)
    {
        if (unlockedStatus.ContainsKey(stat))
        {
            unlockedStatus[stat] = true;
        }
    }

    public void DistributePoints(int points)
    {
        List<StatType> unlocked = GetUnlockedStats(); // 새로 만든 메서드 활용
        if (unlocked.Count == 0) return;

        for (int i = 0; i < points; i++)
        {
            StatType targetStat = unlocked[Random.Range(0, unlocked.Count)];
            float weight = GetWeightForStat(targetStat);
            investedRatios[targetStat] += weight;
        }
    }

    private float GetWeightForStat(StatType stat)
    {
        switch (stat)
        {
            case StatType.Health: return 0.02f;
            default: return 0.01f;
        }
    }
}