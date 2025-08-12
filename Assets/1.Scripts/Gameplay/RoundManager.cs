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
        if (allCards == null || allCards.Count < 3)
        {
            Debug.LogError("카드 데이터가 3개 미만이어서 보상을 생성할 수 없습니다.");
            return;
        }

        List<CardDataSO> rewardChoices = new List<CardDataSO>();
        List<CardDataSO> selectableCards = new List<CardDataSO>(allCards);

        for (int i = 0; i < 3; i++)
        {
            // 전체 가중치 합산
            float totalWeight = 0f;
            foreach (var card in selectableCards)
            {
                totalWeight += card.rewardAppearanceWeight;
            }

            // 랜덤 값 선택
            float randomPoint = Random.Range(0, totalWeight);
            float currentWeight = 0f;

            // 가중치에 따라 카드 선택
            CardDataSO selectedCard = null;
            foreach (var card in selectableCards)
            {
                currentWeight += card.rewardAppearanceWeight;
                if (randomPoint <= currentWeight)
                {
                    selectedCard = card;
                    break;
                }
            }

            // 만약 부동소수점 오류 등으로 선택이 안됐을 경우 마지막 카드 선택
            if (selectedCard == null)
            {
                selectedCard = selectableCards[selectableCards.Count - 1];
            }

            rewardChoices.Add(selectedCard);
            selectableCards.Remove(selectedCard); // 중복 선택 방지
        }

        // RewardManager의 큐에 "선택지"로 구성된 리스트를 전달
        RewardManager.Instance.EnqueueReward(rewardChoices);

        Debug.Log($"{rewardChoices.Count}개의 가중치 기반 카드 보상을 생성했습니다.");
    }
}
