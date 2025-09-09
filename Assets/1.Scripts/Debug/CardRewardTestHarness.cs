// ���� ���: Assets/1.Scripts/Debug/CardRewardTestHarness.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

/// <summary>
/// CardReward ���� ���������� �׽�Ʈ�ϱ� ���� ���� �׽�Ʈ Ŭ�����Դϴ�.
/// �ʼ� �Ŵ��� �ε� �� F5 Ű �Է��� ��� ó���մϴ�.
/// </summary>
public class CardRewardTestHarness : MonoBehaviour
{
    [Header("�׽�Ʈ ����")]
    [Tooltip("�������� ������ ī�� ������ �����մϴ�.")]
    [Range(2, 4)]
    public int rewardCardCount = 3;

    [Tooltip("���� �����, �̹� ���� ���� ī�� ������ �����մϴ�.")]
    [Range(0, 7)]
    public int ownedCardCount = 7;

    // --- ���� ���� ---
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

        Debug.LogWarning("[TestHarness] �ʼ� �Ŵ����� ���� �ε带 �����մϴ�...");
        await Addressables.InstantiateAsync(PrefabKeys.Managers).Task;
        await Addressables.InstantiateAsync(PrefabKeys.GameplaySession).Task;
        Debug.Log("[TestHarness] �ʼ� �Ŵ��� ���� �Ϸ�.");

        //  [�ٽ� ����] DataManager�� ã�� ������ �ε带 ���� �����ϰ� ��ٸ��ϴ�. 
        dataManager = ServiceLocator.Get<DataManager>();
        if (dataManager != null)
        {
            Debug.Log("[TestHarness] DataManager �߰�. ������ �ε��� �����մϴ�...");
            await dataManager.LoadAllDataAsync().ToUniTask();
            Debug.Log("[TestHarness] ��� ������ �ε� �Ϸ�.");
        }
        else
        {
            Debug.LogError("[TestHarness] DataManager�� ã�� �� �����ϴ�!");
            isInitialized = true; // ������ �ִ��� Update ������ ������ �ʵ��� ����
            return;
        }

        // ������ �ε� �� ������ �Ŵ������� �Ҵ��մϴ�.
        cardRewardUI = ServiceLocator.Get<CardRewardUIManager>();
        cardManager = ServiceLocator.Get<CardManager>();
        rewardGenerationService = ServiceLocator.Get<RewardGenerationService>();

        isInitialized = true;
        Debug.Log("<color=yellow>--- �׽�Ʈ �غ� �Ϸ�! F5 Ű�� ���� �׽�Ʈ�� �����ϼ���. ---</color>");
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
            Debug.LogError("[TestHarness] �ʼ� �Ŵ��� �� �ϳ��� null�Դϴ�! �ε� ������ Ȯ���ϼ���.");
            return;
        }

        isTestRun = true;
        Debug.Log($"<color=lime>--- F5 Ű �Է� ����! �׽�Ʈ ����: ���� ī�� {rewardCardCount}��, ���� ī�� {ownedCardCount}�� ---</color>");

        cardManager.ClearAndResetDeck();

        List<NewCardDataSO> allCards = dataManager.GetAllNewCards();
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogError("[TestHarness] ī�� �����͸� �ҷ��� �� �����ϴ�! LoadAllDataAsync�� ����� ����Ǿ����� Ȯ���ϼ���.");
            return;
        }

        for (int i = 0; i < ownedCardCount; i++)
        {
            NewCardDataSO randomCardData = allCards[Random.Range(0, allCards.Count)];
            CardInstance instance = cardManager.AddCard(randomCardData);
            if (instance != null) cardManager.Equip(instance);
        }
        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        Debug.Log($"[TestHarness] {playerDataManager.CurrentRunData.ownedCards.Count}���� �׽�Ʈ ī�带 �����ϰ� �����߽��ϴ�.");

        List<NewCardDataSO> rewardChoices = rewardGenerationService.GenerateRewards(rewardCardCount);
        Debug.Log($"[TestHarness] {rewardChoices.Count}���� ���� ī�带 �����߽��ϴ�.");

        cardRewardUI.Initialize(rewardChoices);
    }
}