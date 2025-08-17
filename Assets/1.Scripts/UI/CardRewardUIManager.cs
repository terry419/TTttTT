using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardRewardUIManager : MonoBehaviour
{
    public static CardRewardUIManager Instance { get; private set; }

    [Header("연결된 UI 패널")]
    [SerializeField] private GameObject routeSelectPanel;
    [SerializeField] private Button backRewardButton;


    [Header("UI 프리팹 및 부모")]
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Transform cardSlotsParent;
    [SerializeField] private HorizontalLayoutGroup cardSlotsLayoutGroup;

    [Header("기능 버튼")]
    [SerializeField] private Button acquireButton;
    [SerializeField] private Button synthesizeButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button mapButton;

    [Header("팝업 참조")]
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

        // 자식 오브젝트를 모두 삭제하여 UI를 깨끗하게 비웁니다.
        foreach (Transform child in cardSlotsParent)
        {
            Destroy(child.gameObject);
        }
        spawnedCardDisplays.Clear();

        // 카드 개수에 따라 여백을 자동으로 계산합니다.
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

        // 카드 UI를 생성하고 이벤트를 연결합니다.
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
        // 1. 카드 보상 패널 비활성화
        GetComponent<CanvasGroup>().interactable = false;

        // 2. 맵 선택 패널 활성화
        routeSelectPanel.SetActive(true);

        // 3. 맵 선택 패널의 Back 버튼으로 커서 자동 이동
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(backRewardButton.gameObject);
    }

    public void RefocusOnCardPanel()
    {
        // 맵 선택 패널을 끄는 기능 추가
        routeSelectPanel.SetActive(false);

        // 1. 카드 보상 패널을 다시 상호작용 가능하게 만듭니다.
        GetComponent<CanvasGroup>().interactable = true;

        // ... (이하 기존 RefocusOnCardPanel 함수 내용과 동일) ...
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