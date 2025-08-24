using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CodexController : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private GameObject cardCodexPanel;
    [SerializeField] private GameObject artifactCodexPanel;
    [SerializeField] private Button cardTabButton;
    [SerializeField] private Button artifactTabButton;
    [SerializeField] private ScrollRect cardScrollRect;
    [SerializeField] private ScrollRect artifactScrollRect;
    [SerializeField] private GameObject itemInfoPrefab;

    private List<CardDataSO> allCards;
    private List<ArtifactDataSO> allArtifacts;
    private Dictionary<string, CodexItemDisplay> displayedCodexItems = new Dictionary<string, CodexItemDisplay>();

    // --- [추가] 필요한 매니저들을 저장할 변수 ---
    private ProgressionManager progressionManager;
    private PopupController popupController;
    private DataManager dataManager;

    void Awake()
    {
        cardTabButton.onClick.AddListener(ShowCardCodex);
        artifactTabButton.onClick.AddListener(ShowArtifactCodex);

        // --- [추가] Awake에서 매니저들을 미리 찾아옵니다. ---
        progressionManager = ServiceLocator.Get<ProgressionManager>();
        popupController = ServiceLocator.Get<PopupController>();
        dataManager = ServiceLocator.Get<DataManager>();
    }

    void OnEnable()
    {
        LoadData();
        PopulateCodex();
        ShowCardCodex();
    }

    private void LoadData()
    {
        allCards = dataManager.GetAllCards();
        allArtifacts = dataManager.GetAllArtifacts();
    }

    private void PopulateCodex()
    {
        displayedCodexItems.Clear();

        foreach (Transform child in cardScrollRect.content) Destroy(child.gameObject);
        cardScrollRect.content.DetachChildren();

        foreach (var card in allCards)
        {
            GameObject itemUI = Instantiate(itemInfoPrefab, cardScrollRect.content);
            CodexItemDisplay display = itemUI.GetComponent<CodexItemDisplay>();
            if (display != null)
            {
                bool isUnlocked = progressionManager.IsCodexItemUnlocked(card.cardID);
                display.SetupForCard(card, isUnlocked);
                display.SetHintButtonClickListener(card.cardID, PurchaseHint);
                displayedCodexItems[card.cardID] = display;
            }
        }

        foreach (Transform child in artifactScrollRect.content) Destroy(child.gameObject);
        artifactScrollRect.content.DetachChildren();

        if (allArtifacts != null)
        {
            foreach (var artifact in allArtifacts)
            {
                GameObject itemUI = Instantiate(itemInfoPrefab, artifactScrollRect.content);
                CodexItemDisplay display = itemUI.GetComponent<CodexItemDisplay>();
                if (display != null)
                {
                    bool isUnlocked = progressionManager.IsCodexItemUnlocked(artifact.artifactID);
                    display.SetupForArtifact(artifact, isUnlocked);
                    display.SetHintButtonClickListener(artifact.artifactID, PurchaseHint);
                    displayedCodexItems[artifact.artifactID] = display;
                }
            }
        }
        Debug.Log("도감 데이터를 기반으로 UI를 모두 생성했습니다.");
    }

    private void ShowCardCodex()
    {
        cardCodexPanel.SetActive(true);
        artifactCodexPanel.SetActive(false);
        cardTabButton.image.color = Color.white;
        artifactTabButton.image.color = Color.gray;
    }

    private void ShowArtifactCodex()
    {
        cardCodexPanel.SetActive(false);
        artifactCodexPanel.SetActive(true);
        cardTabButton.image.color = Color.gray;
        artifactTabButton.image.color = Color.white;
    }

    public void PurchaseHint(string itemId)
    {
        int hintCost = 10;
        if (progressionManager.SpendCurrency(MetaCurrencyType.KnowledgeShards, hintCost))
        {
            Debug.Log($"{itemId}의 힌트를 구매했습니다.");
            if (displayedCodexItems.TryGetValue(itemId, out CodexItemDisplay display))
            {
                display.ShowHint();
            }
            if (popupController != null) popupController.ShowError("힌트 구매 성공!", 1.5f);
        }
        else
        {
            Debug.LogWarning("힌트 구매에 필요한 지식의 파편이 부족합니다.");
            if (popupController != null) popupController.ShowError("지식의 파편이 부족합니다!", 1.5f);
        }
    }
}