using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 메타 프로그레션(도전 과제, 보스 최초 처치 보상, 영구 재화 등) 데이터를 관리하고,
/// 관련 시스템(룰렛, 도감 등)에 데이터를 제공하는 중앙 관리자입니다.
/// 이 클래스는 게임의 영구적인 성장 요소를 총괄하며, 싱글톤으로 구현됩니다.
/// </summary>
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    // --- 영구 데이터 필드 --- //
    // 실제 게임에서는 이 데이터들을 PlayerPrefs, JSON 파일 등으로 저장하고 불러와야 합니다.

    // 데이터 증강 모듈 (도감 힌트 구매 등에 사용되는 재화)
    public int KnowledgeShards { get; private set; }
    
    // 유전자 증폭제 포인트 (캐릭터 영구 능력치 강화용 재화)
    public int GenePoints { get; private set; }

    // 도전 과제 달성 여부를 저장하는 딕셔너리 (Key: 도전 과제 ID, Value: 달성 여부)
    private Dictionary<string, bool> achievementsUnlocked = new Dictionary<string, bool>();

    // 보스 최초 처치 여부를 저장하는 딕셔너리 (Key: 보스 ID, Value: 처치 여부)
    private Dictionary<string, bool> bossFirstKills = new Dictionary<string, bool>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // TODO: 실제 게임에서는 여기서 LoadData()를 호출해야 합니다.
        LoadDummyData(); // 임시 더미 데이터 로드
    }

    /// <summary>
    /// 영구 재화를 추가합니다.
    /// </summary>
    /// <param name="type">재화 종류</param>
    /// <param name="amount">추가할 양</param>
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
        // TODO: SaveData() 호출
    }

    /// <summary>
    /// 영구 재화를 사용합니다.
    /// </summary>
    /// <param name="type">재화 종류</param>
    /// <param name="amount">사용할 양</param>
    /// <returns>사용 성공 여부</returns>
    public bool SpendCurrency(MetaCurrencyType type, int amount)
    {
        if (amount < 0) 
        {
            Debug.LogError("재화는 음수 값을 사용할 수 없습니다.");
            return false;
        }

        switch (type)
        {
            case MetaCurrencyType.KnowledgeShards:
                if (KnowledgeShards >= amount)
                {
                    KnowledgeShards -= amount;
                    Debug.Log($"지식의 파편 {amount} 사용. 현재: {KnowledgeShards}");
                    // TODO: SaveData() 호출
                    return true;
                }
                break;
            case MetaCurrencyType.GenePoints:
                if (GenePoints >= amount)
                {
                    GenePoints -= amount;
                    Debug.Log($"유전자 증폭제 포인트 {amount} 사용. 현재: {GenePoints}");
                    // TODO: SaveData() 호출
                    return true;
                }
                break;
        }

        Debug.LogWarning($"{type} 재화가 부족하여 {amount}를 사용할 수 없습니다.");
        return false;
    }

    /// <summary>
    /// 도전 과제 달성을 기록하고 보상을 지급합니다.
    /// </summary>
    /// <param name="achievementID">달성한 도전 과제의 고유 ID</param>
    public void TrackAchievement(string achievementID)
    {
        if (!achievementsUnlocked.ContainsKey(achievementID) || achievementsUnlocked[achievementID] == false)
        {
            achievementsUnlocked[achievementID] = true;
            Debug.Log($"도전 과제 달성: {achievementID}");

            // TODO: 도전 과제 데이터(SO)를 참조하여 보상 지급 로직 구현
            // AchievementData data = DataManager.Instance.GetAchievement(achievementID);
            // AddCurrency(data.rewardType, data.rewardAmount);

            // TODO: SaveData() 호출
        }
    }

    /// <summary>
    /// 보스 최초 처치를 기록하고 보상을 지급합니다.
    /// </summary>
    /// <param name="bossID">처치한 보스의 고유 ID</param>
    public void RegisterBossFirstKill(string bossID)
    {
        if (!bossFirstKills.ContainsKey(bossID) || bossFirstKills[bossID] == false)
        {
            bossFirstKills[bossID] = true;
            Debug.Log($"보스 최초 처치: {bossID}");

            // TODO: 보스 데이터(SO)를 참조하여 보상 지급 로직 구현
            // BossData data = DataManager.Instance.GetBoss(bossID);
            // AddCurrency(data.firstKillRewardType, data.firstKillRewardAmount);

            // TODO: SaveData() 호출
        }
    }

    // --- 데이터 저장/로드 (실제 구현 시 확장 필요) ---
    private void LoadDummyData()
    {
        KnowledgeShards = 100;
        GenePoints = 50;
        Debug.Log("임시 더미 메타 데이터를 로드했습니다.");
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
