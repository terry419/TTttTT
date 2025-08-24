// 파일 경로: Assets/1/Scripts/UI/CardRewardUIManager.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// 카드 보상 씬의 UI를 총괄하는 관리자입니다.
/// RewardManager로부터 카드 목록을 받아 화면에 표시하고, 사용자의 선택(획득, 합성, 스킵)을 처리하며,
/// 선택 결과에 따라 다른 UI(맵 선택)로의 전환을 담당합니다.
/// </summary>
public class CardRewardUIManager : MonoBehaviour
{
    // --- Inspector-Visible Fields --- //
    [Header("UI 요소 및 부모")]
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardSlotsParent;
    
    // ▼▼▼ [1] 이 줄을 추가하세요. ▼▼▼
    [SerializeField] private CanvasGroup cardRewardCanvasGroup;

    [Header("버튼 참조")]
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button mapButton;

    [Header("팝업 참조")]
    [SerializeField] private SynthesisPopup synthesisPopup;

    // --- Private State Fields --- //
    private CardDisplay selectedDisplay;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();
    private GameObject lastSelectedCardObject; // UI 포커스 관리를 위해 마지막으로 선택된 오브젝트를 저장

    // --- Unity Lifecycle Methods --- //
    void Awake()
    {
        ServiceLocator.Register<CardRewardUIManager>(this);

        // 각 버튼에 대한 이벤트 리스너를 연결합니다.
        acquireButton.onClick.AddListener(OnAcquireClicked);
        synthesizeButton.onClick.AddListener(OnSynthesizeClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
        if (mapButton != null) { mapButton.onClick.AddListener(OnMapButtonClicked); }

        Debug.Log("[CardRewardUIManager] Awake: 이벤트 리스너가 성공적으로 연결되었습니다.");
    }

    void OnEnable()
    {
        // RewardManager의 static 이벤트들을 구독하여, 보상이 준비되거나 스킵될 때 적절한 함수가 호출되도록 합니다.
        RewardManager.OnCardRewardReady += Initialize;
        RewardManager.OnRewardSkipped += HandleRewardSkipped;
    }

    void OnDisable()
{
    // 오브젝트가 비활성화될 때, 메모리 누수를 방지하기 위해 구독했던 이벤트를 해제합니다.
    RewardManager.OnCardRewardReady -= Initialize;
    RewardManager.OnRewardSkipped -= HandleRewardSkipped;
}

// ▼▼▼▼▼▼▼▼▼▼▼ [이 부분 추가] ▼▼▼▼▼▼▼▼▼▼▼
/// <summary>
/// 이 UI 오브젝트가 활성화되고 난 후, 첫 프레임에 호출됩니다.
/// </summary>
void Start()
{
    // RewardManager를 찾아 다음 보상 처리를 시작하도록 명시적으로 요청합니다.
    var rewardManager = ServiceLocator.Get<RewardManager>();
    if (rewardManager != null)
    {
        Debug.Log("[CardRewardUIManager] Start: RewardManager에게 보상 처리를 요청합니다.");
        rewardManager.ProcessNextReward();
    }
    else
    {
        Debug.LogError("[CardRewardUIManager] Start에서 RewardManager를 찾을 수 없습니다!");
    }
}
// ▲▲▲▲▲▲▲▲▲▲▲ [여기까지 추가] ▲▲▲▲▲▲▲▲▲▲▲

    // --- Event Handlers --- //
    private void HandleRewardSkipped()
    {
        Debug.Log("[CardRewardUIManager] 보상 스킵 신호(OnRewardSkipped)를 감지했습니다. 맵 선택으로 즉시 이동합니다.");
        TransitionToMap();
    }

    /// <summary>
    /// RewardManager로부터 카드 목록을 받아 보상 UI를 초기화하고 화면에 표시합니다.
    /// </summary>
    public void Initialize(List<CardDataSO> cardChoices)
    {
        Debug.Log($"[CardRewardUIManager] Initialize: {cardChoices.Count}개의 카드 보상으로 UI를 초기화합니다.");

        // 이전에 생성된 카드 UI가 있다면 모두 파괴하여 초기화합니다.
        foreach (Transform child in cardSlotsParent) { Destroy(child.gameObject); }
        spawnedCardDisplays.Clear();

        // 전달받은 카드 데이터 목록을 순회하며 CardDisplay 프리팹을 생성합니다.
        foreach (var cardData in cardChoices)
        {
            GameObject cardUI = Instantiate(cardDisplayPrefab, cardSlotsParent);
            CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Setup(cardData);
                cardDisplay.OnCardSelected.AddListener(HandleCardSelection);
                spawnedCardDisplays.Add(cardDisplay);
            }
        }

        selectedDisplay = null; // 선택된 카드 없음으로 초기화
        UpdateButtonsState(); // 버튼 상태 업데이트
        // ▼▼▼ [2] 기존 SetInitialFocus 호출 부분을 변경된 이름으로 수정합니다. ▼▼▼
        StartCoroutine(SetFocusToCardCoroutine()); // 첫 번째 카드에 포커스 설정
    }

    /// <summary>
    /// 특정 카드 UI가 선택되었을 때 호출됩니다.
    /// </summary>
    private void HandleCardSelection(CardDisplay display)
    {
        selectedDisplay = display;
        Debug.Log($"[CardRewardUIManager] 카드 선택됨: {display.CurrentCard.cardName}");

        // 모든 카드 UI를 순회하며, 선택된 카드에만 하이라이트 효과를 적용합니다.
        foreach (var d in spawnedCardDisplays)
        {
            bool isSelected = (d == selectedDisplay);
            d.SetHighlight(isSelected);
            if (isSelected) { lastSelectedCardObject = d.gameObject; }
        }
        UpdateButtonsState(); // 버튼 상태 업데이트
    }

    // --- Button Click Handlers --- //
    private void OnAcquireClicked()
    {
        if (selectedDisplay == null)
        {
            Debug.LogWarning("[CardRewardUIManager] OnAcquireClicked: 선택된 카드가 없어 아무것도 하지 않습니다.");
            return;
        }

        CardDataSO selectedCardData = selectedDisplay.CurrentCard;
        Debug.Log($"[CardRewardUIManager] '획득' 버튼 클릭됨. 선택된 카드: {selectedCardData.cardName}");

        // [핵심 로직] CardManager를 통해 실제로 카드를 덱에 추가합니다.
        var cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null)
        {
            cardManager.AcquireNewCard(selectedCardData);
            Debug.Log($"[CardRewardUIManager] CardManager를 통해 '{selectedCardData.cardName}' 카드를 성공적으로 획득했습니다.");
        }
        else
        {
            Debug.LogError("[CardRewardUIManager] CRITICAL: CardManager를 찾을 수 없어 카드를 획득할 수 없습니다!");
        }

        // 보상 선택이 완료되었음을 RewardManager에 알립니다.
        ServiceLocator.Get<RewardManager>()?.CompleteRewardSelection();
        // 맵 선택 화면으로 전환합니다.
        TransitionToMap();
    }

    private void OnSynthesizeClicked()
    {
        if (selectedDisplay == null || !synthesizeButton.interactable) return;
        Debug.Log($"[CardRewardUIManager] '합성' 버튼 클릭됨. 선택된 카드: {selectedDisplay.CurrentCard.cardName}");

        var cardManager = ServiceLocator.Get<CardManager>();
        CardDataSO selectedCardData = selectedDisplay.CurrentCard;
        List<CardDataSO> materialChoices = cardManager.GetSynthesizablePairs(selectedCardData);

        if (materialChoices.Count > 0 && synthesisPopup != null)
        {
            // ▼▼▼ [2] 팝업을 띄우기 전에 뒷쪽 패널의 상호작용을 막습니다. ▼▼▼
            if (cardRewardCanvasGroup != null)
            {
                cardRewardCanvasGroup.interactable = false;
            }

            synthesisPopup.gameObject.SetActive(true);

            // ▼▼▼ [3] 팝업이 닫힐 때 상호작용을 다시 활성화하도록 콜백을 전달합니다. ▼▼▼
            synthesisPopup.Initialize(selectedCardData.cardName, materialChoices, 
            (chosenMaterial) => {
                // 확인 콜백
                Debug.Log($"[CardRewardUIManager] 합성 재료 '{chosenMaterial.cardName}' 선택됨. 합성을 실행합니다.");
                if (cardRewardCanvasGroup != null) cardRewardCanvasGroup.interactable = true;
                cardManager.SynthesizeCards(selectedCardData, chosenMaterial);
                ServiceLocator.Get<RewardManager>()?.CompleteRewardSelection();
                TransitionToMap();
            }, 
            () => {
                // 취소 콜백
                Debug.Log("[CardRewardUIManager] 합성 취소됨.");
                if (cardRewardCanvasGroup != null) cardRewardCanvasGroup.interactable = true;
                // 포커스를 마지막으로 선택했던 합성 버튼으로 되돌립니다.
                EventSystem.current.SetSelectedGameObject(synthesizeButton.gameObject);
            });
        }
    }

    private void OnSkipClicked()
    {
        Debug.Log("[CardRewardUIManager] '스킵' 버튼 클릭됨.");
        ServiceLocator.Get<RewardManager>()?.CompleteRewardSelection();
        TransitionToMap();
    }

    private void OnMapButtonClicked()
    {
        Debug.Log("[CardRewardUIManager] '맵으로' 버튼 클릭됨.");
        TransitionToMap();
    }

    // --- Helper Methods --- //
    private void UpdateButtonsState()
    {
        acquireButton.interactable = (selectedDisplay != null);

        bool canSynthesize = false;
        if (selectedDisplay != null)
        {
            var cardManager = ServiceLocator.Get<CardManager>();
            if (cardManager != null)
            {
                canSynthesize = cardManager.HasSynthesizablePair(selectedDisplay.CurrentCard);
            }
        }
        synthesizeButton.interactable = canSynthesize;
    }

    private void TransitionToMap()
    {
        Debug.Log("[CardRewardUIManager] 맵 선택 화면으로 전환을 시작합니다...");

        var routeSelectionController = ServiceLocator.Get<RouteSelectionController>();
        if (routeSelectionController != null)
        {
            // [핵심 로직] 맵 선택 UI를 활성화합니다.
            routeSelectionController.Show();
        }
        else
        {
            Debug.LogError("[CardRewardUIManager] CRITICAL: RouteSelectionController를 찾을 수 없어 맵 화면으로 전환할 수 없습니다!");
        }

        // [핵심 로직] 전환 후, 자신(카드 보상 UI)은 확실하게 비활성화합니다.
        Hide();
    }

    // ▼▼▼ [1] 기존 SetInitialFocus 코루틴의 이름을 바꾸고 내용을 보강합니다. ▼▼▼
    private IEnumerator SetFocusToCardCoroutine()
    {
        yield return null; // UI 요소가 완전히 생성/활성화될 때까지 한 프레임 대기

        // 마지막으로 선택했던 카드가 있으면 그곳에 포커스를 맞춥니다.
        if (lastSelectedCardObject != null && lastSelectedCardObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedCardObject);
        }
        // 마지막 선택 기록이 없으면, 생성된 카드 목록의 첫 번째에 포커스를 맞춥니다.
        else if (spawnedCardDisplays.Count > 0)
        {
            lastSelectedCardObject = spawnedCardDisplays[0].gameObject;
            EventSystem.current.SetSelectedGameObject(lastSelectedCardObject);
        }
    }

    public void Show() 
    { 
        gameObject.SetActive(true);
        // 맵에서 돌아왔을 때 포커스를 되찾기 위해 코루틴을 호출합니다.
        StartCoroutine(SetFocusToCardCoroutine());
    }
    public void Hide() { gameObject.SetActive(false); }
}