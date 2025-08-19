// 경로: Assets/1.Scripts/UI/CardRewardUIManager.cs
// [수정됨] 다른 스크립트가 호출할 수 있는 Show/Hide 함수를 추가하고 포커스 관리를 강화했습니다.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardRewardUIManager : MonoBehaviour
{
    public static CardRewardUIManager Instance { get; private set; }

    [Header("UI 요소 및 부모")]
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardSlotsParent;

    [Header("기능 버튼")]
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button mapButton;

    [Header("팝업")]
    [SerializeField] private SynthesisPopup synthesisPopup;

    private CardDataSO selectedCard;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();
    private GameObject lastSelectedCardObject; // 마지막으로 선택했던 카드 UI 오브젝트 저장

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        acquireButton.onClick.AddListener(OnAcquireClicked);
        synthesizeButton.onClick.AddListener(OnSynthesizeClicked);
        skipButton.onClick.AddListener(OnSkipClicked);
        if (mapButton != null) { mapButton.onClick.AddListener(OnMapButtonClicked); }
    }

    void OnEnable() { RewardManager.OnCardRewardReady += Initialize; }
    void OnDisable() { RewardManager.OnCardRewardReady -= Initialize; }

    public void Initialize(List<CardDataSO> cardChoices)
    {
        foreach (Transform child in cardSlotsParent) { Destroy(child.gameObject); }
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
        selectedCard = null;
        UpdateButtonsState();
        StartCoroutine(SetInitialFocus());
    }

    private IEnumerator SetInitialFocus()
    {
        yield return null;
        if (spawnedCardDisplays.Count > 0)
        {
            lastSelectedCardObject = spawnedCardDisplays[0].gameObject;
            EventSystem.current.SetSelectedGameObject(lastSelectedCardObject);
        }
    }

    private void HandleCardSelection(CardDataSO card)
    {
        selectedCard = card;
        foreach (var display in spawnedCardDisplays)
        {
            bool isSelected = display.CurrentCard == selectedCard;
            display.SetHighlight(isSelected);
            if (isSelected) { lastSelectedCardObject = display.gameObject; }
        }
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        acquireButton.interactable = (selectedCard != null);
        bool canSynthesize = false;
        if (selectedCard != null && CardManager.Instance != null) { canSynthesize = CardManager.Instance.HasSynthesizablePair(selectedCard); }
        synthesizeButton.interactable = canSynthesize;
    }

    private void OnAcquireClicked()
    {
        if (selectedCard == null) return;
        RewardManager.Instance.CompleteRewardSelection();
        TransitionToMap();
    }

    private void OnSynthesizeClicked()
    {
        if (selectedCard == null || !synthesizeButton.interactable) return;
        List<CardDataSO> materialChoices = CardManager.Instance.GetSynthesizablePairs(selectedCard);
        if (materialChoices.Count > 0 && synthesisPopup != null)
        {
            synthesisPopup.gameObject.SetActive(true);
            synthesisPopup.Initialize(selectedCard.cardName, materialChoices, (chosenMaterial) => {
                CardManager.Instance.SynthesizeCards(selectedCard, chosenMaterial);
                RewardManager.Instance.CompleteRewardSelection();
                TransitionToMap();
            });
        }
    }

    private void OnSkipClicked()
    {
        RewardManager.Instance.CompleteRewardSelection();
        TransitionToMap();
    }

    private void OnMapButtonClicked()
    {
        TransitionToMap();
    }

    // [수정] 맵으로 전환하는 로직을 공통 함수로 분리
    private void TransitionToMap()
    {
        if (RouteSelectionController.Instance != null) { RouteSelectionController.Instance.Show(); }
        Hide();
    }

    // [추가] 외부에서 이 패널을 다시 활성화할 수 있도록 Show 함수 추가
    public void Show()
    {
        gameObject.SetActive(true);
        // 돌아왔을 때 마지막으로 선택했던 카드 또는 버튼에 다시 포커스
        if (lastSelectedCardObject != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedCardObject);
        }
        else if (spawnedCardDisplays.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(spawnedCardDisplays[0].gameObject);
        }
    }

    // [추가] 외부에서 이 패널을 비활성화할 수 있도록 Hide 함수 추가
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
