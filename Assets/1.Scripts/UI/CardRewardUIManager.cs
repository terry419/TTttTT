// 경로: ./TTttTT/Assets/1/Scripts/UI/CardRewardUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;

public class CardRewardUIManager : MonoBehaviour
{
    [SerializeField] private GameObject cardDisplayPrefab; // 여기에 UIToolkitCard.prefab을 할당해야 합니다.
    [SerializeField] private Transform cardSlotsParent;
    [SerializeField] private CanvasGroup cardRewardCanvasGroup;
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private SynthesisPopup synthesisPopup;

    // --- 타입을 CardDisplayHost로 완전히 변경 ---
    private CardDisplayHost selectedDisplay;
    private List<CardDisplayHost> spawnedCardDisplays = new List<CardDisplayHost>();
    private GameObject lastSelectedCardObject;

    void Awake()
    {
        ServiceLocator.Register<CardRewardUIManager>(this);
        acquireButton.onClick.AddListener(OnAcquireClicked);
        synthesizeButton.onClick.AddListener(OnSynthesizeClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
        if (mapButton != null) mapButton.onClick.AddListener(OnMapButtonClicked);
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
        // 씬 시작 시 다음 보상 처리 로직을 호출합니다.
        ServiceLocator.Get<RewardManager>()?.ProcessNextReward();
    }

    private void HandleRewardSkipped()
    {
        TransitionToMap();
    }

    public void Initialize(List<NewCardDataSO> cardChoices)
    {
        Debug.Log($"[CardReward] Initialize with {cardChoices.Count} card choices.");
        foreach (Transform child in cardSlotsParent) Destroy(child.gameObject);
        spawnedCardDisplays.Clear();

        foreach (var cardData in cardChoices)
        {
            if (cardData == null)
            {
                Debug.LogError("[CardReward] A null cardData was provided.");
                continue;
            }

            GameObject cardUI = Instantiate(cardDisplayPrefab, cardSlotsParent);

            // --- GetComponent 대상을 CardDisplayHost로 명확히 함 ---
            CardDisplayHost cardDisplayHost = cardUI.GetComponent<CardDisplayHost>();

            if (cardDisplayHost != null)
            {
                cardDisplayHost.Setup(cardData);
                cardDisplayHost.OnCardSelected.AddListener(HandleCardSelection);
                spawnedCardDisplays.Add(cardDisplayHost);
            }
            else
            {
                // 이 오류가 발생하면 1단계(프리팹 교체)가 제대로 이루어지지 않은 것입니다.
                Debug.LogError($"[CardReward] Critical Error: The instantiated prefab '{cardUI.name}' is missing the 'CardDisplayHost' component. Please ensure the correct prefab is assigned in the inspector.", cardUI);
            }
        }

        selectedDisplay = null;
        UpdateButtonsState();
        StartCoroutine(SetFocusToFirstCard());
    }

    private void HandleCardSelection(CardDisplayHost display) // 타입을 CardDisplayHost로 변경
    {
        selectedDisplay = display;
        foreach (var d in spawnedCardDisplays)
        {
            bool isSelected = (d == selectedDisplay);
            d.SetHighlight(isSelected); // CardDisplayHost의 SetHighlight 호출
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
            CardInstance newInstance = cardManager.AddCard(selectedCardData);
            if (newInstance != null)
            {
                // 기본적으로 새로 얻은 카드는 바로 장착합니다.
                cardManager.Equip(newInstance);
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

        List<CardInstance> materialChoices = cardManager.ownedCards
            .Where(card => card.CardData.basicInfo.rarity == baseCardData.basicInfo.rarity &&
                           card.CardData.basicInfo.type == baseCardData.basicInfo.type)
            .ToList();

        if (materialChoices.Count == 0)
        {
            ServiceLocator.Get<PopupController>()?.ShowError("합성에 사용할 수 있는 동일 등급, 동일 속성의 카드가 없습니다.", 2f);
            return;
        }

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
        Debug.Log("Synthesis canceled.");
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
        bool isCardSelected = selectedDisplay != null;
        acquireButton.interactable = isCardSelected;

        bool canSynthesize = false;
        if (isCardSelected)
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

        synthesizeButton.interactable = canSynthesize;
    }

    private void TransitionToMap()
    {
        ServiceLocator.Get<RouteSelectionController>()?.Show();
        // Hide(); // Hide 로직은 RouteSelectionController.Show()에서 처리하는 것이 더 안정적일 수 있습니다.
        gameObject.SetActive(false); // 임시로 비활성화
    }

    private IEnumerator SetFocusToFirstCard()
    {
        yield return null; // 한 프레임 대기
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

    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(SetFocusToFirstCard());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}