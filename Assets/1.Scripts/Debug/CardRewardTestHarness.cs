//  : Assets/1.Scripts/Debug/CardRewardTestHarness.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// CardReward 씬을 단독으로 테스트하기 위한 테스트 클래스입니다.
/// 씬에 진입하면 자동으로, 또는 F5 키 입력으로 테스트를 실행합니다.
/// </summary>
public class CardRewardTestHarness : MonoBehaviour
{
    [Header("테스트 설정")]
    [Tooltip("보상으로 제시할 카드 수를 설정합니다.")]
    [Range(2, 4)]
    public int rewardCardCount = 3;

    [Tooltip("플레이어가 이미 소유한 것으로 가정할 카드 수를 설정합니다.")]
    [Range(0, 7)]
    public int ownedCardCount = 7;

    // --- 내부 참조 ---
    private CardRewardUIManager cardRewardUI;
    private CardManager cardManager;
    private RewardGenerationService rewardGenerationService;
    private DataManager dataManager;

    private bool isInitialized = false;
    private bool isTestRun = false;

    async void Awake()
    {
        if (ServiceLocator.IsRegistered<GameManager>())
        {
            gameObject.SetActive(false);
            return;
        }

        Debug.LogWarning("[TestHarness] 필수 매니저 로드를 시작합니다...");
        // [리팩토링] 애플리케이션 -> 게임 세션 순서로 생성합니다.
        await Addressables.InstantiateAsync(PrefabKeys.Managers).Task;
        await Addressables.InstantiateAsync(PrefabKeys.GameplaySession).Task;
        Debug.Log("[TestHarness] 필수 매니저 로드 완료.");

        // [재확인] DataManager를 찾아 데이터를 로드할 때까지 기다립니다. 
        dataManager = ServiceLocator.Get<DataManager>();
        if (dataManager != null)
        {
            Debug.Log("[TestHarness] DataManager 감지. 데이터 로드를 시작합니다...");
            await dataManager.LoadAllDataAsync().ToUniTask();
            Debug.Log("[TestHarness] 모든 데이터 로드 완료.");
        }
        else
        {
            Debug.LogError("[TestHarness] DataManager를 찾을 수 없습니다!");
            isInitialized = true; // 더 이상 진행하지 않도록 플래그 설정
            return;
        }

        // 데이터 로드 후 매니저를 할당합니다.
        cardRewardUI = FindObjectOfType<CardRewardUIManager>(); // 씬에 이미 있으므로 FindObjectOfType 사용
        cardManager = ServiceLocator.Get<CardManager>();
        rewardGenerationService = ServiceLocator.Get<RewardGenerationService>();

        isInitialized = true;
        Debug.Log("<color=yellow>--- 테스트 준비 완료! F5 키를 눌러 테스트를 실행하세요. ---</color>");
    }

    void Update()
    {
        if (isInitialized && !isTestRun && Input.GetKeyDown(KeyCode.F5))
        {
            SetupTestAndRun();
        }
    }

    public void SetupTestAndRun()
    {
        if (cardManager == null || dataManager == null || rewardGenerationService == null || cardRewardUI == null)
        {
            Debug.LogError("[TestHarness] 필수 매니저 중 하나가 null입니다! 로드 과정을 확인하세요.");
            return;
        }

        isTestRun = true;
        Debug.Log($"<color=lime>--- F5 키 입력 감지! 테스트 시작 : 보상 카드 {rewardCardCount}개, 소유 카드 {ownedCardCount}개 ---</color>");

        cardManager.ClearAndResetDeck();

        List<NewCardDataSO> allCards = dataManager.GetAllNewCards();
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("[TestHarness] 카드 데이터를 가져올 수 없습니다! LoadAllDataAsync가 제대로 호출되었는지 확인하세요.");
            return;
        }

        for (int i = 0; i < ownedCardCount; i++)
        {
            NewCardDataSO randomCardData = allCards[Random.Range(0, allCards.Count)];
            CardInstance instance = cardManager.AddCard(randomCardData);
            if (instance != null) cardManager.Equip(instance);
        }
        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        Debug.Log($"[TestHarness] {playerDataManager.CurrentRunData.ownedCards.Count}개의 테스트 카드를 생성하고 장착했습니다.");

        List<NewCardDataSO> rewardChoices = rewardGenerationService.GenerateRewards(rewardCardCount);
        Debug.Log($"[TestHarness] {rewardChoices.Count}개의 카드 보상을 생성했습니다.");

        cardRewardUI.Initialize(rewardChoices);
    }
}