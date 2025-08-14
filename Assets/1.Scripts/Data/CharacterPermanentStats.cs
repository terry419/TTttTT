// --- 파일명: CharacterPermanentStats.cs ---

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CharacterPermanentStats
{
    public string characterId;

    [Tooltip("테스트 시 해금할 스탯을 여기에 추가하세요. 실제 게임에서는 룰렛을 통해 해금됩니다.")]
    public List<StatType> defaultUnlockedStats = new List<StatType> { StatType.Health, StatType.Attack, StatType.AttackSpeed, StatType.MoveSpeed, StatType.CritMultiplier, StatType.CritRate };

    // [수정] 변수를 선언할 때 바로 new로 초기화해서 Null 참조 예외를 원천적으로 방지
    public Dictionary<StatType, bool> unlockedStatus = new Dictionary<StatType, bool>();
    public Dictionary<StatType, float> investedRatios = new Dictionary<StatType, float>();

    // JSON 저장을 위한 헬퍼 프로퍼티
    public StatDictionaryData statData
    {
        get
        {
            StatDictionaryData data = new StatDictionaryData();
            // unlockedStatus가 null이 아니므로 이제 여기서 에러가 발생하지 않아.
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

    // [수정] JsonUtility가 이 생성자를 사용하지 않으므로, 딕셔너리 초기화 코드는 선언부로 옮기고 여기서는 비워둠.
    public CharacterPermanentStats()
    {
        // 비어 있어도 괜찮아.
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

        // defaultUnlockedStats 리스트에 있는 스탯들만 '해금' 상태로 변경
        foreach (StatType type in defaultUnlockedStats)
        {
            unlockedStatus[type] = true;
        }
    }

    // ... 이하 나머지 코드는 동일 ...

    public List<StatType> GetLockedStats()
    {
        return unlockedStatus.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
    }

    public List<StatType> GetUnlockedStats()
    {
        return unlockedStatus.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
    }

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
        List<StatType> unlocked = GetUnlockedStats();
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