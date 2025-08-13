using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 도감(Codex) UI의 표시 및 상호작용을 담당합니다.
/// DataManager로부터 모든 카드/유물 리스트를 받아오고, ProgressionManager를 통해 플레이어의
/// 해금 상태를 확인하여 UI에 반영합니다. 스크롤, 탭 전환, 힌트 구매 등의 기능을 포함합니다.
/// </summary>
public class CodexController : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private GameObject cardCodexPanel; // 카드 도감 패널
    [SerializeField] private GameObject artifactCodexPanel; // 유물 도감 패널
    [SerializeField] private Button cardTabButton; // 카드 탭 버튼
    [SerializeField] private Button artifactTabButton; // 유물 탭 버튼
    [SerializeField] private ScrollRect cardScrollRect; // 카드 목록 스크롤뷰
    [SerializeField] private ScrollRect artifactScrollRect; // 유물 목록 스크롤뷰
    [SerializeField] private GameObject itemInfoPrefab; // 도감 아이템 정보를 표시할 UI 프리팹

    private List<CardDataSO> allCards;
    private List<ArtifactDataSO> allArtifacts;

    // 힌트 구매 시 업데이트할 CodexItemDisplay 인스턴스를 저장
    private Dictionary<string, CodexItemDisplay> displayedCodexItems = new Dictionary<string, CodexItemDisplay>();

    void Awake()
    {
        // 탭 버튼 리스너 연결
        cardTabButton.onClick.AddListener(ShowCardCodex);
        artifactTabButton.onClick.AddListener(ShowArtifactCodex);
    }

    void OnEnable()
    {
        // 도감이 활성화될 때 데이터를 로드하고 UI를 채웁니다.
        LoadData();
        PopulateCodex();
        // 기본으로 카드 탭을 보여줍니다.
        ShowCardCodex();
    }

    /// <summary>
    /// DataManager로부터 모든 카드와 유물 데이터를 가져옵니다.
    /// </summary>
    private void LoadData()
    {
        allCards = DataManager.Instance.GetAllCards();
        allArtifacts = DataManager.Instance.GetAllArtifacts();
        // TODO: 정렬 로직 추가 (예: 등급, 이름순)
    }

    /// <summary>
    /// 로드된 데이터를 기반으로 도감 UI를 채웁니다.
    /// </summary>
    private void PopulateCodex()
    {
        displayedCodexItems.Clear(); // 기존에 저장된 아이템 참조 초기화

        // --- 카드 도감 채우기 ---
        // 안전하게 자식 오브젝트 삭제
        foreach (Transform child in cardScrollRect.content)
        {
            Destroy(child.gameObject);
        }
        cardScrollRect.content.DetachChildren();

        // 모든 카드에 대해 UI 아이템 생성
        foreach (var card in allCards)
        {
            GameObject itemUI = Instantiate(itemInfoPrefab, cardScrollRect.content);
            CodexItemDisplay display = itemUI.GetComponent<CodexItemDisplay>();
            if (display != null)
            {
                bool isUnlocked = ProgressionManager.Instance.IsCodexItemUnlocked(card.cardID);
                display.SetupForCard(card, isUnlocked);
                // 힌트 버튼 클릭 리스너 연결
                display.SetHintButtonClickListener(card.cardID, PurchaseHint);
                displayedCodexItems[card.cardID] = display;
            }
        }

        // --- 유물 도감 채우기 ---
        // 안전하게 자식 오브젝트 삭제
        foreach (Transform child in artifactScrollRect.content)
        {
            Destroy(child.gameObject);
        }
        artifactScrollRect.content.DetachChildren();

        foreach (var artifact in allArtifacts)
        {
            GameObject itemUI = Instantiate(itemInfoPrefab, artifactScrollRect.content);
            CodexItemDisplay display = itemUI.GetComponent<CodexItemDisplay>();
            if (display != null)
            {
                bool isUnlocked = ProgressionManager.Instance.IsCodexItemUnlocked(artifact.artifactID);
                display.SetupForArtifact(artifact, isUnlocked);
                // 힌트 버튼 클릭 리스너 연결
                display.SetHintButtonClickListener(artifact.artifactID, PurchaseHint);
                displayedCodexItems[artifact.artifactID] = display;
            }
        }
        Debug.Log("도감 데이터를 기반으로 UI를 모두 생성했습니다.");
    }

    /// <summary>
    /// 카드 도감 탭을 활성화합니다.
    /// </summary>
    private void ShowCardCodex()
    {
        cardCodexPanel.SetActive(true);
        artifactCodexPanel.SetActive(false);
        // 탭 버튼의 비주얼 업데이트
        cardTabButton.image.color = Color.white; // 선택됨
        artifactTabButton.image.color = Color.gray; // 선택 안됨
    }

    /// <summary>
    /// 유물 도감 탭을 활성화합니다.
    /// </summary>
    private void ShowArtifactCodex()
    {
        cardCodexPanel.SetActive(false);
        artifactCodexPanel.SetActive(true);
        // 탭 버튼의 비주얼 업데이트
        cardTabButton.image.color = Color.gray; // 선택 안됨
        artifactTabButton.image.color = Color.white; // 선택됨
    }

    /// <summary>
    /// (CodexItemDisplay에서 호출) 잠긴 아이템의 힌트를 구매합니다.
    /// </summary>
    /// <param name="itemId">힌트를 구매할 아이템의 ID</param>
    public void PurchaseHint(string itemId)
    {
        int hintCost = 10; // 힌트 구매 비용
        if (ProgressionManager.Instance.SpendCurrency(MetaCurrencyType.KnowledgeShards, hintCost))
        {
            Debug.Log($"{itemId}의 힌트를 구매했습니다.");
            // 해당 아이템 UI의 힌트를 표시하도록 업데이트
            if (displayedCodexItems.TryGetValue(itemId, out CodexItemDisplay display))
            {
                // CodexItemDisplay에 힌트 표시 메서드가 있다고 가정
                // display.ShowHint(); 
                // 임시로 Debug.Log로 대체
                Debug.Log($"[Codex] {itemId} 힌트 표시 요청");
            }
            PopupController.Instance.ShowError("힌트 구매 성공!", 1.5f); // 성공 알림
        }
        else
        {
            Debug.LogWarning("힌트 구매에 필요한 지식의 파편이 부족합니다.");
            PopupController.Instance.ShowError("지식의 파편이 부족합니다!", 1.5f); // 실패 알림
        }
    }
}