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
        // --- 카드 도감 채우기 ---
        // 기존 UI 아이템들 삭제
        foreach (Transform child in cardScrollRect.content)
        {
            Destroy(child.gameObject);
        }
        // 모든 카드에 대해 UI 아이템 생성
        foreach (var card in allCards)
        {
            GameObject itemUI = Instantiate(itemInfoPrefab, cardScrollRect.content);
            // TODO: ProgressionManager에서 해당 카드의 해금 상태를 가져와야 함
            // bool isUnlocked = ProgressionManager.Instance.IsCardUnlocked(card.cardID);
            // itemUI.GetComponent<CodexItemDisplay>().Setup(card, isUnlocked);
        }

        // --- 유물 도감 채우기 ---
        foreach (Transform child in artifactScrollRect.content)
        {
            Destroy(child.gameObject);
        }
        foreach (var artifact in allArtifacts)
        {
            GameObject itemUI = Instantiate(itemInfoPrefab, artifactScrollRect.content);
            // TODO: ProgressionManager에서 해당 유물의 해금 상태를 가져와야 함
            // bool isUnlocked = ProgressionManager.Instance.IsArtifactUnlocked(artifact.artifactID);
            // itemUI.GetComponent<CodexItemDisplay>().Setup(artifact, isUnlocked);
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
        // TODO: 탭 버튼의 비주얼을 업데이트하여 현재 선택된 탭을 표시
    }

    /// <summary>
    /// 유물 도감 탭을 활성화합니다.
    /// </summary>
    private void ShowArtifactCodex()
    {
        cardCodexPanel.SetActive(false);
        artifactCodexPanel.SetActive(true);
        // TODO: 탭 버튼의 비주얼을 업데이트하여 현재 선택된 탭을 표시
    }

    /// <summary>
    /// (CodexItemDisplay에서 호출) 잠긴 아이템의 힌트를 구매합니다.
    /// </summary>
    /// <param name="itemId">힌트를 구매할 아이템의 ID</param>
    public void PurchaseHint(string itemId)
    {
        // TODO: 힌트 구매 비용 정의 (예: 10 지식의 파편)
        int hintCost = 10;
        if (ProgressionManager.Instance.SpendCurrency(MetaCurrencyType.KnowledgeShards, hintCost))
        {
            Debug.Log($"{itemId}의 힌트를 구매했습니다.");
            // TODO: 해당 아이템 UI의 힌트를 표시하도록 업데이트
            // PopupController로 구매 성공 알림 표시
        }
        else
        { 
            Debug.LogWarning("힌트 구매에 필요한 지식의 파편이 부족합니다.");
            // PopupController로 재화 부족 알림 표시
        }
    }
}
