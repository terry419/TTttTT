// --- 파일명: CodexController.cs ---

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

    void Awake()
    {
        cardTabButton.onClick.AddListener(ShowCardCodex);
        artifactTabButton.onClick.AddListener(ShowArtifactCodex);
    }

    void OnEnable()
    {
        LoadData();
        PopulateCodex();
        ShowCardCodex();
    }

    private void LoadData()
    {
        allCards = DataManager.Instance.GetAllCards();

        // [수정] 이제 DataManager.Instance.GetAllArtifacts()가 존재하므로 정상적으로 호출돼.
        allArtifacts = DataManager.Instance.GetAllArtifacts();
    }

    private void PopulateCodex()
    {
        displayedCodexItems.Clear();

        foreach (Transform child in cardScrollRect.content)
        {
            Destroy(child.gameObject);
        }
        cardScrollRect.content.DetachChildren();

        foreach (var card in allCards)
        {
            GameObject itemUI = Instantiate(itemInfoPrefab, cardScrollRect.content);
            CodexItemDisplay display = itemUI.GetComponent<CodexItemDisplay>();
            if (display != null)
            {
                bool isUnlocked = ProgressionManager.Instance.IsCodexItemUnlocked(card.cardID);
                display.SetupForCard(card, isUnlocked);
                display.SetHintButtonClickListener(card.cardID, PurchaseHint);
                displayedCodexItems[card.cardID] = display;
            }
        }

        foreach (Transform child in artifactScrollRect.content)
        {
            Destroy(child.gameObject);
        }
        artifactScrollRect.content.DetachChildren();

        // [수정] allArtifacts가 null이 아니므로 이제 이 부분도 문제 없이 작동해.
        if (allArtifacts != null)
        {
            foreach (var artifact in allArtifacts)
            {
                GameObject itemUI = Instantiate(itemInfoPrefab, artifactScrollRect.content);
                CodexItemDisplay display = itemUI.GetComponent<CodexItemDisplay>();
                if (display != null)
                {
                    bool isUnlocked = ProgressionManager.Instance.IsCodexItemUnlocked(artifact.artifactID);
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
        if (ProgressionManager.Instance.SpendCurrency(MetaCurrencyType.KnowledgeShards, hintCost))
        {
            Debug.Log($"{itemId}의 힌트를 구매했습니다.");
            if (displayedCodexItems.TryGetValue(itemId, out CodexItemDisplay display))
            {
                display.ShowHint();
            }
            PopupController.Instance.ShowError("힌트 구매 성공!", 1.5f);
        }
        else
        {
            Debug.LogWarning("힌트 구매에 필요한 지식의 파편이 부족합니다.");
            PopupController.Instance.ShowError("지식의 파편이 부족합니다!", 1.5f);
        }
    }
}