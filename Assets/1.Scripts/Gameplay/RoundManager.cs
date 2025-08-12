using UnityEngine;
using System.Collections.Generic; // List<> 사용을 위해 추가

/// <summary>
/// 일반 라운드의 타이밍, 몬스터 스폰, 클리어 조건, 난이도 단계를 제어합니다.
/// 게임 플레이의 전체적인 흐름을 관리하는 핵심 클래스입니다.
/// </summary>
public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("라운드 설정")]
    [SerializeField] private float roundDuration = 180f; // 기본 라운드 시간 (3분)
    [SerializeField] private int killGoal = 100; // 목표 킬 수

    [Header("참조")]
    [SerializeField] private MonsterSpawner monsterSpawner; // 몬스터 스폰을 담당하는 클래스
    [SerializeField] private HUDController hudController; // UI 업데이트를 위한 HUD 컨트롤러

    private float remainingTime;
    private int killCount;
    private bool isRoundActive = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        { 
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (!isRoundActive) return;

        // 시간 경과 처리
        remainingTime -= Time.deltaTime;
        if (hudController != null) hudController.UpdateTimer(remainingTime);

        // 시간 만료로 라운드 종료 (생존 클리어)
        if (remainingTime <= 0)
        {
            EndRound(false); // 보상 없음
        }
    }

    /// <summary>
    /// 새로운 라운드를 시작합니다.
    /// </summary>
    public void StartRound()
    {
        Debug.Log("새로운 라운드를 시작합니다.");
        remainingTime = roundDuration;
        killCount = 0;
        isRoundActive = true;

        if (monsterSpawner != null)
        {
            monsterSpawner.StartSpawning(); // 몬스터 스포너 활성화
        }

        if (hudController != null)
        {
            hudController.UpdateKillCount(killCount, killGoal);
            hudController.UpdateTimer(remainingTime);
            hudController.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 라운드를 종료합니다.
    /// </summary>
    /// <param name="wasKillGoalReached">킬 수를 달성하여 종료되었는지 여부</param>
    public void EndRound(bool wasKillGoalReached)
    {
        if (!isRoundActive) return; // 중복 호출 방지

        isRoundActive = false;
        Debug.Log($"라운드 종료. 킬 수 달성: {wasKillGoalReached}");

        if (monsterSpawner != null)
        {
            monsterSpawner.StopSpawning(); // 몬스터 스폰 중지
        }

        if (wasKillGoalReached)
        { 
            // 킬 수 달성 시 보상 생성 및 상태 전환
            GenerateCardReward();
            GameManager.Instance.ChangeState(GameManager.GameState.Reward);
        }
        else
        {
            // 시간 만료 시에는 루트 선택(맵)으로 바로 가거나, 다른 로직 처리
            // 현재 기획에서는 시간 만료 시 보상이 없으므로, 바로 다음 단계로 넘어감을 가정
            // TODO: 맵 선택 씬 또는 다른 결과 씬으로 전환하는 로직 필요
            GameManager.Instance.ChangeState(GameManager.GameState.MainMenu); // 임시로 메인메뉴로 이동
        }
    }

    /// <summary>
    /// 몬스터가 죽었을 때 MonsterController에서 호출됩니다.
    /// </summary>
    public void RegisterKill()
    {
        if (!isRoundActive) return;

        killCount++;
        if (hudController != null) hudController.UpdateKillCount(killCount, killGoal);

        // 목표 킬 수 달성 시 라운드 종료
        if (killCount >= killGoal)
        {
            EndRound(true); // 보상 있음
        }
    }

    private void GenerateCardReward()
    {
        List<CardDataSO> allCards = DataManager.Instance.GetAllCards();
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("카드 데이터가 없어 보상을 생성할 수 없습니다.");
            return;
        }

        // TODO: 기획서에 따른 가중치 기반 랜덤 선택 로직 구현 필요
        // 현재는 간단하게 전체 카드 중 3장을 랜덤으로 선택합니다.
        List<CardDataSO> rewardChoices = new List<CardDataSO>();
        for (int i = 0; i < 3; i++)
        {
            if (allCards.Count > 0)
            {
                int randomIndex = Random.Range(0, allCards.Count);
                rewardChoices.Add(allCards[randomIndex]);
                // 중복 선택을 피하기 위해 리스트에서 제거 (간단한 방식)
                allCards.RemoveAt(randomIndex);
            }
        }

        // RewardManager의 큐에 보상을 추가합니다.
        // TODO: RewardManager에 List를 받는 Enqueue 메서드를 만들면 더 효율적입니다.
        foreach (var card in rewardChoices)
        {
            RewardManager.Instance.EnqueueReward(card);
        }

        Debug.Log($"{rewardChoices.Count}개의 카드 보상을 생성했습니다.");
    }
}
