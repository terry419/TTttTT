using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

public class CardRewardUIManager : MonoBehaviour
{
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardSlotsParent;
    [SerializeField] private CanvasGroup cardRewardCanvasGroup;
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private SynthesisPopup synthesisPopup;

    private CardDisplay selectedDisplay;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();
    private GameObject lastSelectedCardObject;

    void Awake()
    {
        if (ServiceLocator.IsRegistered<CardRewardUIManager>())
        {
            Debug.LogWarning($"[{GetType().Name}] 중복 생성되어 파괴됩니다.", this.gameObject);
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Register<CardRewardUIManager>(this);

        acquireButton.onClick.AddListener(OnAcquireClicked);
        synthesizeButton.onClick.AddListener(OnSynthesizeClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
        if (mapButton != null) mapButton.onClick.AddListener(OnMapButtonClicked);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<CardRewardUIManager>(this);
    }

    void OnEnable()
    {
        RewardManager.OnCardRewardReady += Initialize;
        RewardManager.OnRewardSkipped += HandleRewardSkipped;
    }

    void OnDisable()
    {
        RewardManager.OnCardRewardReady -= Initialize;
        RewardManager.OnRewardSkipped -= HandleRewardSkipped;
    }

    void Start()
    {
        ServiceLocator.Get<RewardManager>()?.ProcessNextReward();
    }

    private void HandleRewardSkipped()
    {
        TransitionToMap();
    }

    public void Initialize(List<NewCardDataSO> cardChoices)
    {
        foreach (Transform child in cardSlotsParent) Destroy(child.gameObject);
        spawnedCardDisplays.Clear();

        foreach (var cardData in cardChoices)
        {
            GameObject cardUI = Instantiate(cardDisplayPrefab, cardSlotsParent);
            CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                CardInstance tempInstance = new CardInstance(cardData);
                cardDisplay.Setup(tempInstance);
                cardDisplay.OnCardSelected.AddListener(HandleCardSelection);
                spawnedCardDisplays.Add(cardDisplay);
            }
        }

        selectedDisplay = null;
        UpdateButtonsState();
        StartCoroutine(SetFocusToFirstCard());
    }

    private void HandleCardSelection(CardDisplay display)
    {
        selectedDisplay = display;
        foreach (var d in spawnedCardDisplays)
        {
            bool isSelected = (d == selectedDisplay);
            d.SetHighlight(isSelected);
            if (isSelected) lastSelectedCardObject = d.gameObject;
        }
        UpdateButtonsState();
    }

    private void OnAcquireClicked()
    {
        if (selectedDisplay == null) return;

        NewCardDataSO selectedCardData = selectedDisplay.CurrentCard;
        var cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null)
        {
            CardInstance instanceToEquip = cardManager.AddCard(selectedCardData);
            if (instanceToEquip != null)
            {
                cardManager.Equip(instanceToEquip);
            }
        }

        ServiceLocator.Get<RewardManager>()?.CompleteRewardSelection();
        TransitionToMap();
    }

    private void OnSynthesizeClicked()
    {
        if (selectedDisplay == null) return;

        var baseCardData = selectedDisplay.CurrentCard;
        var cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager == null || synthesisPopup == null) return;

        // 재료 카드 필터링: 등급과 속성이 같고, 자기 자신은 아닌 카드
        List<CardInstance> materialChoices = cardManager.ownedCards
            .Where(card => card.CardData.basicInfo.rarity == baseCardData.basicInfo.rarity &&
                           card.CardData.basicInfo.type == baseCardData.basicInfo.type)
            .ToList();

        if (materialChoices.Count == 0)
        {
            Debug.LogWarning("합성 재료로 사용할 수 있는 카드가 없습니다.");
            // TODO: 사용자에게 알림을 주는 UI 로직 (예: 팝업 메시지)
            return;
        }

        // 팝업 초기화 및 콜백 설정
        synthesisPopup.Initialize(baseCardData, materialChoices, HandleSynthesisConfirm, HandleSynthesisCancel);
    }

    private void HandleSynthesisConfirm(CardInstance materialCard)
    {
        var cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null && selectedDisplay != null)
        {
            cardManager.SynthesizeCard(selectedDisplay.CurrentCard, materialCard);
        }
        
        ServiceLocator.Get<RewardManager>()?.CompleteRewardSelection();
        TransitionToMap();
    }

    private void HandleSynthesisCancel()
    {
        Debug.Log("합성 취소됨");
        StartCoroutine(SetFocusToFirstCard());
    }

    private void OnSkipClicked()
    {
        ServiceLocator.Get<RewardManager>()?.CompleteRewardSelection();
        TransitionToMap();
    }

    private void OnMapButtonClicked()
    {
        TransitionToMap();
    }

    private void UpdateButtonsState()
    {
        bool canAcquire = selectedDisplay != null;
        acquireButton.interactable = canAcquire;

        bool canSynthesize = false;
        if (canAcquire)
        {
            var cardManager = ServiceLocator.Get<CardManager>();
            if (cardManager != null)
            {
                var baseCardData = selectedDisplay.CurrentCard;
                canSynthesize = cardManager.ownedCards.Any(card =>
                    card.CardData.basicInfo.rarity == baseCardData.basicInfo.rarity &&
                    card.CardData.basicInfo.type == baseCardData.basicInfo.type);
            }
        }

        synthesizeButton.gameObject.SetActive(true);
        synthesizeButton.interactable = canSynthesize;
    }
    private void TransitionToMap()
    {
        ServiceLocator.Get<RouteSelectionController>()?.Show();
        Hide();
    }

    private IEnumerator SetFocusToFirstCard()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        if (lastSelectedCardObject != null && lastSelectedCardObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedCardObject);
        }
        else if (spawnedCardDisplays.Count > 0)
        { 
            lastSelectedCardObject = spawnedCardDisplays[0].gameObject;
            EventSystem.current.SetSelectedGameObject(lastSelectedCardObject);
        }
    }

    public void Show() { gameObject.SetActive(true); StartCoroutine(SetFocusToFirstCard()); }
    public void Hide() { gameObject.SetActive(false); }
}