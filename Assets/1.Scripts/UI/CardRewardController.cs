using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 카드 보상 화면의 UI와 상호작용을 총괄하는 컨트롤러입니다.
/// RewardManager로부터 카드 선택지를 받아와 화면에 표시하고, 사용자의 선택(획득, 합성, 포기)을 처리하여
/// 그 결과를 다시 RewardManager에 알리는 역할을 담당합니다.
/// </summary>
public class CardRewardController : MonoBehaviour
{
    public static CardRewardController Instance { get; private set; }

    [Header("UI 요소 참조")]
    [SerializeField] private List<GameObject> cardSlots; // 카드가 표시될 슬롯(패널) 목록
    [SerializeField] private Button acquireButton; // 획득 버튼
    [SerializeField] private Button synthesizeButton; // 합성 버튼
    [SerializeField] private Button skipButton; // 포기 버튼
    [SerializeField] private Button mapButton; // 맵 확인 버튼
    [SerializeField] private GameObject cardInfoPrefab; // 카드 정보 UI 프리팹
    [SerializeField] private SynthesisPopup synthesisPopup; // 합성 팝업 참조

    private List<CardDataSO> currentCardChoices = new List<CardDataSO>();
    private CardDataSO selectedCard = null;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>(); // 생성된 CardDisplay 인스턴스 관리

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        acquireButton.onClick.AddListener(OnAcquireClicked);
        synthesizeButton.onClick.AddListener(OnSynthesizeClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
    }

    void Start()
    {
        gameObject.SetActive(false);
        if (synthesisPopup != null) synthesisPopup.gameObject.SetActive(false);
    }

    /// <summary>
    /// RewardManager로부터 카드 선택지를 받아 보상 화면을 초기화하고 활성화합니다.
    /// </summary>
    public void Initialize(List<CardDataSO> cardChoices)
    {
        currentCardChoices = cardChoices;
        selectedCard = null;
        gameObject.SetActive(true);

        // 기존에 생성된 카드 UI가 있다면 모두 삭제
        foreach (var display in spawnedCardDisplays)
        {
            Destroy(display.gameObject);
        }
        spawnedCardDisplays.Clear();

        // 카드 슬롯에 카드 정보 표시
        for (int i = 0; i < cardChoices.Count && i < cardSlots.Count; i++)
        {
            GameObject cardUI = Instantiate(cardInfoPrefab, cardSlots[i].transform);
            CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Setup(cardChoices[i]);
                spawnedCardDisplays.Add(cardDisplay);
            }
        }

        UpdateButtonsState();
    }

    /// <summary>
    /// UI의 카드 중 하나를 선택했을 때 CardDisplay에 의해 호출됩니다.
    /// </summary>
    public void OnCardSelected(CardDataSO card)
    {
        selectedCard = card;
        Debug.Log($"[CardReward] 카드 선택: {card.cardName}");

        // 모든 카드의 하이라이트를 끈 뒤, 선택된 카드에만 하이라이트 적용
        foreach (var display in spawnedCardDisplays)
        {
            display.SetHighlight(display.GetCurrentCard() == selectedCard);
        }

        UpdateButtonsState();
    }

    /// <summary>
    /// 현재 상태에 따라 버튼의 활성화/비활성화 상태를 업데이트합니다.
    /// </summary>
    private void UpdateButtonsState()
    {
        acquireButton.interactable = (selectedCard != null);

        // 카드가 선택되었고, CardManager에 합성 가능한 쌍이 있을 때만 합성 버튼 활성화
        bool canSynthesize = false;
        if (selectedCard != null)
        {
            canSynthesize = CardManager.Instance.HasSynthesizablePair(selectedCard);
        }
        synthesizeButton.interactable = canSynthesize;
    }

    private void OnAcquireClicked()
    {
        if (selectedCard == null) return;
        Debug.Log($"[CardReward] 획득 버튼 클릭: {selectedCard.cardName}");
        RewardManager.Instance.OnCardRewardConfirmed(selectedCard);
        gameObject.SetActive(false);
    }

    private void OnSynthesizeClicked()
    {
        if (selectedCard == null || !synthesizeButton.interactable) return;

        // 1. CardManager에서 합성 가능한 카드 목록을 가져옵니다.
        List<CardDataSO> materialChoices = CardManager.Instance.GetSynthesizablePairs(selectedCard);

        if (materialChoices.Count == 0)
        {
            PopupController.Instance.ShowError("오류: 합성 가능한 카드가 없습니다.", 2f);
            return;
        }

        // 2. 팝업을 띄우고, 콜백 함수를 등록합니다.
        synthesisPopup.Initialize(selectedCard.cardName, materialChoices, (chosenMaterial) => {
            // 3. 팝업에서 재료 카드가 선택되면, CardManager의 최종 합성 메서드를 호출합니다.
            CardManager.Instance.SynthesizeCards(selectedCard, chosenMaterial);
            
            // 4. 보상 절차를 마무리하고 다음 보상으로 넘어갑니다.
            // 강화된 카드를 획득했으므로, RewardManager의 OnCardRewardConfirmed를 호출할 수도 있지만,
            // 기획에 따라 합성은 획득과 별개이므로, 그냥 다음 보상으로 넘어가는 OnCardRewardSkipped를 호출합니다.
            RewardManager.Instance.OnCardRewardSkipped(); 
            gameObject.SetActive(false); // 카드 보상 화면 비활성화
        });
    }

    private void OnSkipClicked()
    {
        Debug.Log("[CardReward] 포기 버튼 클릭");
        RewardManager.Instance.OnCardRewardSkipped();
        gameObject.SetActive(false);
    }
}
