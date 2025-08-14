using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("이번 라운드 설계도")]
    [SerializeField] private RoundDataSO currentRoundData;

    [Header("참조")]
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private HUDController hudController;

    [Header("Preload 설정")]
    [SerializeField] private GameObject monsterPrefabToPreload;
    [SerializeField] private int monsterPreloadAmount = 20;
    [SerializeField] private GameObject bulletPrefabToPreload; // <--- 추가
    [SerializeField] private int bulletPreloadAmount = 50;

    private float remainingTime;
    private int killCount;
    private bool isRoundActive = false;

    void Awake()
    {
        Instance = this;

        if (PoolManager.Instance != null)
        {
            // 몬스터 준비 운동
            if (monsterPrefabToPreload != null)
            {
                PoolManager.Instance.Preload(monsterPrefabToPreload, monsterPreloadAmount);
            }
            // [추가] 총알 준비 운동
            if (bulletPrefabToPreload != null)
            {
                PoolManager.Instance.Preload(bulletPrefabToPreload, bulletPreloadAmount);
            }
        }
    }

    void Start()
    {
        // 만약 게임이 시작됐는데 라운드가 아직 활성화되지 않았다면 (씬 직접 실행 등)
        // RoundManager가 직접 라운드를 시작합니다.
        if (!isRoundActive)
        {
            StartRound();
        }
    }

    void Update()
    {
        if (!isRoundActive) return;

        remainingTime -= Time.deltaTime;
        if (hudController != null)
        {
            hudController.UpdateTimer(remainingTime);
        }

        if (remainingTime <= 0)
        {
            EndRound(false); // 시간 만료로 클리어 실패
        }
    }

    public void StartRound()
    {
        if (isRoundActive || currentRoundData == null) return;

        Debug.Log("새로운 라운드를 시작합니다.");
        remainingTime = currentRoundData.roundDuration;
        killCount = 0;
        isRoundActive = true;

        if (monsterSpawner != null)
        {
            monsterSpawner.StartSpawning(currentRoundData.waves);
        }

        if (hudController != null)
        {
            hudController.UpdateKillCount(killCount, currentRoundData.killGoal);
            hudController.UpdateTimer(remainingTime);
            hudController.gameObject.SetActive(true);
        }
    }

    public void RegisterKill()
    {
        if (!isRoundActive) return;

        killCount++;
        if (hudController != null)
        {
            hudController.UpdateKillCount(killCount, currentRoundData.killGoal);
        }

        if (killCount >= currentRoundData.killGoal)
        {
            EndRound(true); // 킬 수 달성으로 클리어 성공
        }
    }

    public void EndRound(bool wasKillGoalReached)
    {
        if (!isRoundActive) return;

        isRoundActive = false;
        Debug.Log($"라운드 종료. 킬 수 달성: {wasKillGoalReached}");

        if (monsterSpawner != null)
        {
            monsterSpawner.StopSpawning();
        }

        if (wasKillGoalReached)
        {
            GenerateCardReward();
            GameManager.Instance.ChangeState(GameManager.GameState.Reward);
        }
        else
        {
            GameManager.Instance.ChangeState(GameManager.GameState.MainMenu); // 임시
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
            float totalWeight = 0f;
            foreach (var card in selectableCards)
            {
                totalWeight += card.rewardAppearanceWeight;
            }

            float randomPoint = Random.Range(0, totalWeight);
            float currentWeight = 0f;

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
            if (selectedCard == null && selectableCards.Count > 0)
            {
                selectedCard = selectableCards[selectableCards.Count - 1];
            }

            if (selectedCard != null)
            {
                rewardChoices.Add(selectedCard);
                selectableCards.Remove(selectedCard);
            }
        }
        RewardManager.Instance.EnqueueReward(rewardChoices);
    }
}