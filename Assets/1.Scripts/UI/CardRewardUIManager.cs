using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardRewardUIManager : MonoBehaviour
{
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardSlotsParent;
    [SerializeField] private CanvasGroup cardRewardCanvasGroup;
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private SynthesisPopup synthesisPopup; // 이 줄은 삭제하거나 주석 처리

    private CardDisplay selectedDisplay;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();
    private GameObject lastSelectedCardObject;

    void Awake()
    {
        ServiceLocator.Register<CardRewardUIManager>(this);
        acquireButton.onClick.AddListener(OnAcquireClicked);
        // synthesizeButton.onClick.AddListener(OnSynthesizeClicked); // 합성 기능 삭제
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
        ServiceLocator.Get<RewardManager>()?.ProcessNextReward();
    }

    private void HandleRewardSkipped()
    {
        TransitionToMap();
    }

    // [수정] NewCardDataSO 리스트를 받도록 변경
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
                cardDisplay.Setup(cardData);
                cardDisplay.OnCardSelected.AddListener(HandleCardSelection);
                spawnedCardDisplays.Add(cardDisplay);
            }
        }

        selectedDisplay = null;
        UpdateButtonsState();
        StartCoroutine(SetFocusToCardCoroutine());
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

    // CardRewardUIManager.cs 내부
    private void OnAcquireClicked()
    {
        if (selectedDisplay == null) return;

        NewCardDataSO selectedCardData = selectedDisplay.CurrentCard;
        var cardManager = ServiceLocator.Get<CardManager>();
        if (cardManager != null)
        {
            // [수정] AcquireNewCard 대신 AddCard와 Equip을 순서대로 호출합니다.
            cardManager.AddCard(selectedCardData);
            cardManager.Equip(selectedCardData);

            Debug.Log($"[CardRewardUIManager] 카드 획득 및 장착: {selectedCardData.basicInfo.cardName}");
        }

        ServiceLocator.Get<RewardManager>()?.CompleteRewardSelection();
        TransitionToMap();
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
        acquireButton.interactable = (selectedDisplay != null);
        synthesizeButton.gameObject.SetActive(false); // 합성 버튼 숨기기
    }

    private void TransitionToMap()
    {
        ServiceLocator.Get<RouteSelectionController>()?.Show();
        Hide();
    }

    private IEnumerator SetFocusToCardCoroutine()
    {
        yield return null;
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

    public void Show() { gameObject.SetActive(true); StartCoroutine(SetFocusToCardCoroutine()); }
    public void Hide() { gameObject.SetActive(false); }
}