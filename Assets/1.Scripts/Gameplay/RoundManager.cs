// --- 파일명: RoundManager.cs (기능 복구 및 최종 수정) ---
// 경로: Assets/1.Scripts/Gameplay/RoundManager.cs
using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    // [복구] Inspector에서 직접 Preload할 프리팹을 관리하기 위한 내부 클래스와 리스트
    [System.Serializable]
    public class PreloadItem
    {
        public GameObject prefab;
        public int count;
    }

    [Header("사전 로드 설정 (수동)")]
    [Tooltip("이 라운드 시작 시 미리 생성할 몬스터 프리팹과 수량을 직접 지정합니다.")]
    [SerializeField] private List<PreloadItem> monsterPreloads;
    [Tooltip("이 라운드 시작 시 미리 생성할 총알, VFX 프리팹과 수량을 직접 지정합니다.")]
    [SerializeField] private List<PreloadItem> effectPreloads;

    [Header("참조")]
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private HUDController hudController;

    private RoundDataSO currentRoundData;
    private float remainingTime;
    private int killCount;
    private bool isRoundActive = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Start 단계에서 안전하게 다른 Manager들을 호출합니다.
        DoPreload();

        if (!isRoundActive)
        {
            StartRound();
        }
    }

    /// <summary>
    /// Inspector에 설정된 목록을 기반으로 PoolManager에 Preload를 요청합니다.
    /// </summary>
    private void DoPreload()
    {
        if (PoolManager.Instance == null)
        {
            Debug.LogError("[RoundManager] PoolManager를 찾을 수 없어 Preload를 진행할 수 없습니다. Script Execution Order를 확인하세요.");
            return;
        }

        // 몬스터 Preload
        foreach (var item in monsterPreloads)
        {
            if (item.prefab != null && item.count > 0)
            {
                PoolManager.Instance.Preload(item.prefab, item.count);
            }
        }

        // 총알 및 이펙트 Preload
        foreach (var item in effectPreloads)
        {
            if (item.prefab != null && item.count > 0)
            {
                PoolManager.Instance.Preload(item.prefab, item.count);
            }
        }
    }

    public void StartRound()
    {
        if (CampaignManager.Instance != null)
        {
            currentRoundData = CampaignManager.Instance.GetNextRoundData();
        }

        if (isRoundActive || currentRoundData == null)
        {
            if (currentRoundData == null)
            {
                Debug.LogWarning("시작할 라운드 데이터가 없습니다. 캠페인이 끝났거나 설정되지 않았습니다.");
                GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
            }
            return;
        }

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
            EndRound(true);
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
            GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
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
            if (selectableCards.Count == 0) break;

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
            EndRound(false);
        }
    }
}
