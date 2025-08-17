using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardRewardUIManager : MonoBehaviour
{
    public static CardRewardUIManager Instance { get; private set; }

    [Header("����� UI �г�")]
    [SerializeField] private GameObject routeSelectPanel;
    [SerializeField] private Button backRewardButton;


    [Header("UI ������ �� �θ�")]
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardSlotsParent;
    [SerializeField] private HorizontalLayoutGroup cardSlotsLayoutGroup;

    [Header("��� ��ư")]
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button mapButton;

    [Header("�˾� ����")]
    [SerializeField] private SynthesisPopup synthesisPopup;

    private CardDataSO selectedCard;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();

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

    void OnEnable()
    {
        RewardManager.OnCardRewardReady += Initialize;
    }

    void OnDisable()
    {
        RewardManager.OnCardRewardReady -= Initialize;
    }

    public void Initialize(List<CardDataSO> cardChoices)
    {
        gameObject.SetActive(true);

        // �ڽ� ������Ʈ�� ��� �����Ͽ� UI�� �����ϰ� ���ϴ�.
        foreach (Transform child in cardSlotsParent)
        {
            Destroy(child.gameObject);
        }
        spawnedCardDisplays.Clear();

        // ī�� ������ ���� ������ �ڵ����� ����մϴ�.
        if (cardSlotsLayoutGroup != null && cardChoices.Count > 0)
        {
            LayoutElement cardLayoutElement = cardDisplayPrefab.GetComponent<LayoutElement>();
            if (cardLayoutElement != null)
            {
                float cardWidth = cardLayoutElement.preferredWidth;
                float spacing = cardSlotsLayoutGroup.spacing;
                float totalContentWidth = (cardWidth * cardChoices.Count) + (spacing * (cardChoices.Count - 1));
                float containerWidth = cardSlotsLayoutGroup.GetComponent<RectTransform>().rect.width;
                float remainingSpace = containerWidth - totalContentWidth;
                int sidePadding = Mathf.Max(0, (int)(remainingSpace / 2));
                cardSlotsLayoutGroup.padding.left = sidePadding;
                cardSlotsLayoutGroup.padding.right = sidePadding;
            }
        }

        // ī�� UI�� �����ϰ� �̺�Ʈ�� �����մϴ�.
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

        if (spawnedCardDisplays.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(spawnedCardDisplays[0].selectButton.gameObject);
        }
    }

    private void HandleCardSelection(CardDataSO card)
    {
        selectedCard = card;
        foreach (var display in spawnedCardDisplays)
        {
            display.SetHighlight(display.CurrentCard == selectedCard);
        }
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        acquireButton.interactable = (selectedCard != null);
        bool canSynthesize = false;
        if (selectedCard != null && CardManager.Instance != null)
        {
            canSynthesize = CardManager.Instance.HasSynthesizablePair(selectedCard);
        }
        synthesizeButton.interactable = canSynthesize;
    }

    private void OnAcquireClicked()
    {
        if (selectedCard == null) return;
        RewardManager.Instance.OnCardRewardConfirmed(selectedCard);
        gameObject.SetActive(false);
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
                RewardManager.Instance.OnCardRewardSkipped();
                gameObject.SetActive(false);
            });
        }
    }

    private void OnSkipClicked()
    {
        RewardManager.Instance.OnCardRewardSkipped();
        gameObject.SetActive(false);
    }

    public void ShowMapPanel()
    {
        // 1. ī�� ���� �г� ��Ȱ��ȭ
        GetComponent<CanvasGroup>().interactable = false;

        // 2. �� ���� �г� Ȱ��ȭ
        routeSelectPanel.SetActive(true);

        // 3. �� ���� �г��� Back ��ư���� Ŀ�� �ڵ� �̵�
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(backRewardButton.gameObject);
    }

    public void RefocusOnCardPanel()
    {
        // �� ���� �г��� ���� ��� �߰�
        routeSelectPanel.SetActive(false);

        // 1. ī�� ���� �г��� �ٽ� ��ȣ�ۿ� �����ϰ� ����ϴ�.
        GetComponent<CanvasGroup>().interactable = true;

        // ... (���� ���� RefocusOnCardPanel �Լ� ����� ����) ...
        GameObject objectToSelect = null;
        if (selectedCard != null)
        {
            foreach (var display in spawnedCardDisplays)
            {
                if (display.CurrentCard == selectedCard)
                {
                    objectToSelect = display.selectButton.gameObject;
                    break;
                }
            }
        }
        if (objectToSelect == null && spawnedCardDisplays.Count > 0)
        {
            objectToSelect = spawnedCardDisplays[0].selectButton.gameObject;
        }
        if (objectToSelect != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(objectToSelect);
        }
    }

}