// 파일 경로: Assets/1.Scripts/Debug/CardRewardTestHarness.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// CardReward 씬을 독립적으로 테스트하기 위한 통합 테스트 클래스입니다.
/// 필수 매니저 로드 및 F5 키 입력을 모두 처리합니다.
/// </summary>
public class CardRewardTestHarness : MonoBehaviour
{
    [Header("테스트 설정")]
    [Tooltip("보상으로 제시할 카드 개수를 조절합니다.")]
    [Range(2, 4)]
    public int rewardCardCount = 3;

    [Tooltip("재료로 사용할, 이미 보유 중인 카드 개수를 조절합니다.")]
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

        Debug.LogWarning("[TestHarness] 필수 매니저가 없어 로드를 시작합니다...");
        await Addressables.InstantiateAsync(PrefabKeys.Managers).Task;
        await Addressables.InstantiateAsync(PrefabKeys.GameplaySession).Task;
        Debug.Log("[TestHarness] 필수 매니저 생성 완료.");

        //  [핵심 수정] DataManager를 찾아 데이터 로드를 직접 실행하고 기다립니다. 
        dataManager = ServiceLocator.Get<DataManager>();
        if (dataManager != null)
        {
            Debug.Log("[TestHarness] DataManager 발견. 데이터 로딩을 시작합니다...");
            await dataManager.LoadAllDataAsync().ToUniTask();
            Debug.Log("[TestHarness] 모든 데이터 로딩 완료.");
        }
        else
        {
            Debug.LogError("[TestHarness] DataManager를 찾을 수 없습니다!");
            isInitialized = true; // 오류가 있더라도 Update 루프가 멈추지 않도록 설정
            return;
        }

        // 데이터 로드 후 나머지 매니저들을 할당합니다.
        cardRewardUI = ServiceLocator.Get<CardRewardUIManager>();
        cardManager = ServiceLocator.Get<CardManager>();
        rewardGenerationService = ServiceLocator.Get<RewardGenerationService>();

        isInitialized = true;
        Debug.Log("<color=yellow>--- 테스트 준비 완료! F5 키를 눌러 테스트를 시작하세요. ---</color>");
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
        Debug.Log($"<color=lime>--- F5 키 입력 감지! 테스트 시작: 보상 카드 {rewardCardCount}장, 보유 카드 {ownedCardCount}장 ---</color>");

        cardManager.ClearAndResetDeck();

        List<NewCardDataSO> allCards = dataManager.GetAllNewCards();
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("[TestHarness] 카드 데이터를 불러올 수 없습니다! LoadAllDataAsync가 제대로 실행되었는지 확인하세요.");
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
        Debug.Log($"[TestHarness] {rewardChoices.Count}개의 보상 카드를 생성했습니다.");

        cardRewardUI.Initialize(rewardChoices);
    }
}