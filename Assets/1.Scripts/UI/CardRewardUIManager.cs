// ���: Assets/1.Scripts/UI/CardRewardUIManager.cs
// [������] �ٸ� ��ũ��Ʈ�� ȣ���� �� �ִ� Show/Hide �Լ��� �߰��ϰ� ��Ŀ�� ������ ��ȭ�߽��ϴ�.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardRewardUIManager : MonoBehaviour
{
    public static CardRewardUIManager Instance { get; private set; }

    [Header("UI ��� �� �θ�")]
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardSlotsParent;

    [Header("��� ��ư")]
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button mapButton;

    [Header("�˾�")]
    [SerializeField] private SynthesisPopup synthesisPopup;

    private CardDataSO selectedCard;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();
    private GameObject lastSelectedCardObject; // ���������� �����ߴ� ī�� UI ������Ʈ ����

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

    // [����] ������ ��ȯ�ϴ� ������ ���� �Լ��� �и�
    private void TransitionToMap()
    {
        if (RouteSelectionController.Instance != null) { RouteSelectionController.Instance.Show(); }
        Hide();
    }

    // [�߰�] �ܺο��� �� �г��� �ٽ� Ȱ��ȭ�� �� �ֵ��� Show �Լ� �߰�
    public void Show()
    {
        gameObject.SetActive(true);
        // ���ƿ��� �� ���������� �����ߴ� ī�� �Ǵ� ��ư�� �ٽ� ��Ŀ��
        if (lastSelectedCardObject != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelectedCardObject);
        }
        else if (spawnedCardDisplays.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(spawnedCardDisplays[0].gameObject);
        }
    }

    // [�߰�] �ܺο��� �� �г��� ��Ȱ��ȭ�� �� �ֵ��� Hide �Լ� �߰�
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
