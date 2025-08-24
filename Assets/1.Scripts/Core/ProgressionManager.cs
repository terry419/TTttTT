// 파일명: ProgressionManager.cs (리팩토링 완료)
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 메타 프로그레션(도전 과제, 영구 재화 등) 데이터를 관리하고,
/// 관련 시스템에 데이터를 제공하는 중앙 관리자입니다.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public int KnowledgeShards { get; private set; }
    public int GenePoints { get; private set; }
    private Dictionary<string, bool> achievementsUnlocked = new Dictionary<string, bool>();
    private Dictionary<string, bool> bossFirstKills = new Dictionary<string, bool>();
    private Dictionary<string, CharacterPermanentStats> permanentStatsDict = new Dictionary<string, CharacterPermanentStats>();

    private string savePath;

    void Awake()
    {
        ServiceLocator.Register<ProgressionManager>(this);
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "progression.json");
        LoadData();
    }

    public void AddCurrency(MetaCurrencyType type, int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("재화는 음수 값을 추가할 수 없습니다.");
            return;
        }

        switch (type)
        {
            case MetaCurrencyType.KnowledgeShards:
                KnowledgeShards += amount;
                Debug.Log($"지식의 파편 {amount} 획득. 현재: {KnowledgeShards}");
                break;
            case MetaCurrencyType.GenePoints:
                GenePoints += amount;
                Debug.Log($"유전자 증폭제 포인트 {amount} 획득. 현재: {GenePoints}");
                break;
        }
        SaveData();
    }

    public bool SpendCurrency(MetaCurrencyType type, int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("재화는 음수 값을 사용할 수 없습니다.");
            return false;
        }

        bool success = false;
        switch (type)
        {
            case MetaCurrencyType.KnowledgeShards:
                if (KnowledgeShards >= amount)
                {
                    KnowledgeShards -= amount;
                    Debug.Log($"지식의 파편 {amount} 사용. 현재: {KnowledgeShards}");
                    success = true;
                }
                break;
            case MetaCurrencyType.GenePoints:
                if (GenePoints >= amount)
                {
                    GenePoints -= amount;
                    Debug.Log($"유전자 증폭제 포인트 {amount} 사용. 현재: {GenePoints}");
                    success = true;
                }
                break;
        }

        if (success)
        {
            SaveData();
            return true;
        }
        else
        {
            Debug.LogWarning($"{type} 재화가 부족하여 {amount}를 사용할 수 없습니다.");
            return false;
        }
    }

    public void TrackAchievement(string achievementID)
    {
        if (!achievementsUnlocked.ContainsKey(achievementID) || achievementsUnlocked[achievementID] == false)
        {
            achievementsUnlocked[achievementID] = true;
            Debug.Log($"도전 과제 달성: {achievementID}");
            SaveData();
        }
    }

    public void RegisterBossFirstKill(string bossID)
    {
        if (!bossFirstKills.ContainsKey(bossID) || bossFirstKills[bossID] == false)
        {
            bossFirstKills[bossID] = true;
            Debug.Log($"보스 최초 처치: {bossID}");
            SaveData();
        }
    }

    public CharacterPermanentStats GetPermanentStatsFor(string characterId)
    {
        if (!permanentStatsDict.TryGetValue(characterId, out var stats))
        {
            stats = new CharacterPermanentStats(characterId);
            permanentStatsDict[characterId] = stats;
            Debug.Log($"{characterId}에 대한 새로운 영구 스탯 데이터를 생성했습니다.");
        }
        return stats;
    }

    public void SaveData()
    {
        ProgressionData data = new ProgressionData
        {
            knowledgeShards = this.KnowledgeShards,
            genePoints = this.GenePoints,
            achievementIDs = achievementsUnlocked.Keys.ToList(),
            achievementStates = achievementsUnlocked.Values.ToList(),
            bossKillIDs = bossFirstKills.Keys.ToList(),
            bossKillStates = bossFirstKills.Values.ToList(),
            characterPermanentStats = permanentStatsDict.Values.ToList()
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"데이터 저장 완료: {savePath}");
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ProgressionData data = JsonUtility.FromJson<ProgressionData>(json);

            permanentStatsDict = new Dictionary<string, CharacterPermanentStats>();
            foreach (var stats in data.characterPermanentStats)
            {
                stats.statData = new StatDictionaryData
                {
                    statTypes = stats.statData.statTypes,
                    unlockedStatuses = stats.statData.unlockedStatuses,
                    investedRatios = stats.statData.investedRatios
                };
                permanentStatsDict[stats.characterId] = stats;
            }

            Debug.Log($"데이터 로드 완료: {savePath}");
        }
        else
        {
            Debug.Log("세이브 파일이 없어 새 게임을 시작합니다.");
            KnowledgeShards = 0;
            GenePoints = 0;
            achievementsUnlocked = new Dictionary<string, bool>();
            bossFirstKills = new Dictionary<string, bool>();
            permanentStatsDict = new Dictionary<string, CharacterPermanentStats>();
        }
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    public bool IsCodexItemUnlocked(string itemID)
    {
        if (achievementsUnlocked.ContainsKey(itemID) && achievementsUnlocked[itemID])
        {
            return true;
        }
        if (bossFirstKills.ContainsKey(itemID) && bossFirstKills[itemID])
        {
            return true;
        }
        return false;
    }
}

public enum MetaCurrencyType
{
    KnowledgeShards,
    GenePoints
}