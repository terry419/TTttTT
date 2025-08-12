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

    // 카드 정보를 표시하기 위한 프리팹 또는 UI 템플릿
    [SerializeField] private GameObject cardInfoPrefab;

    private List<CardDataSO> currentCardChoices = new List<CardDataSO>();
    private CardDataSO selectedCard = null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 버튼 이벤트 리스너 연결
        acquireButton.onClick.AddListener(OnAcquireClicked);
        synthesizeButton.onClick.AddListener(OnSynthesizeClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
    }

    void Start()
    {
        // 초기에는 UI가 보이지 않도록 비활성화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// RewardManager로부터 카드 선택지를 받아 보상 화면을 초기화하고 활성화합니다.
    /// </summary>
    /// <param name="cardChoices">플레이어에게 제시할 카드 목록</param>
    public void Initialize(List<CardDataSO> cardChoices)
    {
        currentCardChoices = cardChoices;
        selectedCard = null;
        gameObject.SetActive(true);

        // 기존에 생성된 카드 정보 UI가 있다면 모두 삭제
        foreach (var slot in cardSlots) { /* 자식 오브젝트 삭제 로직 */ }

        // 카드 슬롯에 카드 정보 표시
        for (int i = 0; i < cardChoices.Count && i < cardSlots.Count; i++)
        {
            GameObject cardUI = Instantiate(cardInfoPrefab, cardSlots[i].transform);
            // TODO: cardUI의 Text, Image 컴포넌트에 cardChoices[i]의 데이터를 채워넣는 로직
            // 예: cardUI.GetComponent<CardDisplay>().Setup(cardChoices[i]);
            // 또한, 이 UI 오브젝트에 클릭 이벤트를 추가하여 OnCardSelected를 호출해야 함
        }

        UpdateButtonsState();
    }

    /// <summary>
    /// UI의 카드 중 하나를 선택했을 때 호출됩니다.
    /// </summary>
    /// <param name="card">선택한 카드 데이터</param>
    public void OnCardSelected(CardDataSO card)
    {
        selectedCard = card;
        Debug.Log($"[CardReward] 카드 선택: {card.cardName}");
        // TODO: 선택되었음을 시각적으로 표시하는 로직 (예: 테두리 하이라이트)
        UpdateButtonsState();
    }

    /// <summary>
    /// 현재 상태에 따라 버튼의 활성화/비활성화 상태를 업데이트합니다.
    /// </summary>
    private void UpdateButtonsState()
    {
        // 카드가 선택되었을 때만 '획득' 버튼 활성화
        acquireButton.interactable = (selectedCard != null);

        // '합성' 버튼은 카드가 선택되었고, 인벤토리에 합성 가능한 카드가 있을 때만 활성화
        bool canSynthesize = false;
        if (selectedCard != null)
        { 
            // TODO: CardManager와 연동하여 합성 가능한 카드가 있는지 확인하는 로직
            // canSynthesize = CardManager.Instance.HasSynthesizableCard(selectedCard);
        }
        synthesizeButton.interactable = canSynthesize;
    }

    private void OnAcquireClicked()
    {
        if (selectedCard == null) return;
        Debug.Log($"[CardReward] 획득 버튼 클릭: {selectedCard.cardName}");
        RewardManager.Instance.OnCardRewardConfirmed(selectedCard);
        gameObject.SetActive(false); // 보상 화면 비활성화
    }

    private void OnSynthesizeClicked()
    {
        if (selectedCard == null) return;
        Debug.Log($"[CardReward] 합성 버튼 클릭: {selectedCard.cardName}");

        // TODO: 합성 UI를 띄우거나, 자동으로 합성을 진행하는 로직
        // 기획서: 동일 속성/등급 카드 2장 선택 시 합성 가능
        // 만약 조건이 맞지 않으면 PopupController를 통해 알림을 띄웁니다.
        if (!synthesizeButton.interactable) // 이중 체크
        {
            PopupController.Instance.ShowError("동일한 속성과 등급의 카드를 선택해야 합니다.", 2f);
            return;
        }

        // TODO: 합성 성공 시 강화된 카드를 RewardManager에 전달
        // CardDataSO synthesizedCard = CardManager.Instance.Synthesize(selectedCard, otherCard);
        // RewardManager.Instance.OnCardRewardConfirmed(synthesizedCard);
        gameObject.SetActive(false); // 보상 화면 비활성화
    }

    private void OnSkipClicked()
    {
        Debug.Log("[CardReward] 포기 버튼 클릭");
        RewardManager.Instance.OnCardRewardSkipped();
        gameObject.SetActive(false); // 보상 화면 비활성화
    }
}
