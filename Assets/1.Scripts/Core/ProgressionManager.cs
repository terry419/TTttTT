using UnityEngine;
using System.Collections.Generic;
using System.IO; // 파일 입출력을 위해 추가
using System.Linq; // Linq 사용을 위해 추가

/// <summary>
/// 메타 프로그레션(도전 과제, 보스 최초 처치 보상, 영구 재화 등) 데이터를 관리하고,
/// 관련 시스템(룰렛, 도감 등)에 데이터를 제공하는 중앙 관리자입니다.
/// 이 클래스는 게임의 영구적인 성장 요소를 총괄하며, 싱글톤으로 구현됩니다.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    // --- 영구 데이터 필드 --- //
    public int KnowledgeShards { get; private set; }
    public int GenePoints { get; private set; }
    private Dictionary<string, bool> achievementsUnlocked = new Dictionary<string, bool>();
    private Dictionary<string, bool> bossFirstKills = new Dictionary<string, bool>();
    private Dictionary<string, CharacterPermanentStats> permanentStatsDict = new Dictionary<string, CharacterPermanentStats>();

    private string savePath; // 저장 파일 경로

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 저장 경로 설정
        savePath = Path.Combine(Application.persistentDataPath, "progression.json");

        LoadData(); // 데이터 로드
    }

    /// <summary>
    /// 영구 재화를 추가합니다.
    /// </summary>
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
        // TODO: UI에 재화 변경 사항 업데이트 알림 (이벤트 호출 등)
        SaveData();
    }

    /// <summary>
    /// 영구 재화를 사용합니다.
    /// </summary>
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

    /// <summary>
    /// 도전 과제 달성을 기록하고 보상을 지급합니다.
    /// </summary>
    public void TrackAchievement(string achievementID)
    {
        if (!achievementsUnlocked.ContainsKey(achievementID) || achievementsUnlocked[achievementID] == false)
        {
            achievementsUnlocked[achievementID] = true;
            Debug.Log($"도전 과제 달성: {achievementID}");

            // TODO: 도전 과제 데이터(SO)를 참조하여 보상 지급 로직 구현
            // AchievementData data = DataManager.Instance.GetAchievement(achievementID);
            // AddCurrency(data.rewardType, data.rewardAmount);

            SaveData();
        }
    }

    /// <summary>
    /// 보스 최초 처치를 기록하고 보상을 지급합니다.
    /// </summary>
    public void RegisterBossFirstKill(string bossID)
    {
        if (!bossFirstKills.ContainsKey(bossID) || bossFirstKills[bossID] == false)
        {
            bossFirstKills[bossID] = true;
            Debug.Log($"보스 최초 처치: {bossID}");

            // TODO: 보스 데이터(SO)를 참조하여 보상 지급 로직 구현
            // BossData data = DataManager.Instance.GetBoss(bossID);
            // AddCurrency(data.firstKillRewardType, data.firstKillRewardAmount);

            SaveData();
        }
    }

    /// <summary>
    /// 특정 캐릭터의 영구 스탯 데이터를 가져옵니다. 없으면 새로 생성합니다.
    /// </summary>
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


    // --- 데이터 저장/로드 ---
    
    /// <summary>
    /// 현재 진행 상황을 JSON 파일로 저장합니다.
    /// </summary>
    public void SaveData()
    {
        ProgressionData data = new ProgressionData
        {
            knowledgeShards = this.KnowledgeShards,
            genePoints = this.GenePoints,
            
            // Dictionary를 List 두 개로 변환
            achievementIDs = achievementsUnlocked.Keys.ToList(),
            achievementStates = achievementsUnlocked.Values.ToList(),
            
            bossKillIDs = bossFirstKills.Keys.ToList(),
            bossKillStates = bossFirstKills.Values.ToList(),

            // 캐릭터 영구 스탯 저장
            characterPermanentStats = permanentStatsDict.Values.ToList()
        };

        string json = JsonUtility.ToJson(data, true); // 'true' for pretty print
        File.WriteAllText(savePath, json);
        Debug.Log($"데이터 저장 완료: {savePath}");
    }

    /// <summary>
    /// JSON 파일에서 진행 상황을 불러옵니다.
    /// </summary>
    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ProgressionData data = JsonUtility.FromJson<ProgressionData>(json);

            // ... (기존 로드 로직은 유지)

            // 캐릭터 영구 스탯 로드
            permanentStatsDict = new Dictionary<string, CharacterPermanentStats>();
            foreach (var stats in data.characterPermanentStats)
            {
                // 불러온 데이터의 Dictionary를 다시 채워줍니다.
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
            // 세이브 파일이 없으면 기본값으로 시작
            Debug.Log("세이브 파일이 없어 새 게임을 시작합니다.");
            KnowledgeShards = 0;
            GenePoints = 0;
            achievementsUnlocked = new Dictionary<string, bool>();
            bossFirstKills = new Dictionary<string, bool>();
            permanentStatsDict = new Dictionary<string, CharacterPermanentStats>();
        }
    }

    // 게임 종료 시 자동 저장
    private void OnApplicationQuit()
    {
        SaveData();
    }

    /// <summary>
    /// 특정 도감 항목(카드 또는 유물)이 해금되었는지 확인합니다.
    /// </summary>
    /// <param name="itemID">도감 항목의 ID (카드 ID 또는 유물 ID)</param>
    /// <returns>해금되었으면 true, 아니면 false</returns>
    public bool IsCodexItemUnlocked(string itemID)
    {
        // 도전 과제 달성으로 해금되는 경우
        if (achievementsUnlocked.ContainsKey(itemID) && achievementsUnlocked[itemID])
        {
            return true;
        }
        // 보스 처치로 해금되는 경우
        if (bossFirstKills.ContainsKey(itemID) && bossFirstKills[itemID])
        {
            return true;
        }
        
        // TODO: 다른 해금 조건(예: 상점 구매, 특정 레벨 도달)이 있다면 여기에 추가
        
        return false;
    }
}

/// <summary>
/// 메타 재화의 종류를 정의하는 열거형
/// </summary>
public enum MetaCurrencyType
{
    KnowledgeShards, // 지식의 파편 (도감용)
    GenePoints       // 유전자 증폭제 포인트 (캐릭터 강화용)
}
