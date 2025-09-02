// 파일 경로: ./TTttTT/Assets/1.Scripts/UI/TEST/CardDisplayHost.cs
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
// using UnityEngine.UI; // 전체 경로를 명시할 것이므로 이 줄은 없어도 괜찮습니다.

[System.Serializable]
public class CardDisplayHostSelectedEvent : UnityEvent<CardDisplayHost> { }

// [수정] typeof(Button) -> typeof(UnityEngine.UI.Button)으로 변경하여 UGUI의 버튼임을 명시
[RequireComponent(typeof(UIDocument), typeof(UnityEngine.UI.Button), typeof(RectTransform))]
public class CardDisplayHost : MonoBehaviour
{
    [Header("카드 크기 설정 (UGUI Layout용)")]
    [Tooltip("UGUI의 Layout Group이 인식할 카드의 너비입니다.")]
    [SerializeField] private float cardWidth = 300f;

    [Tooltip("UGUI의 Layout Group이 인식할 카드의 높이입니다.")]
    [SerializeField] private float cardHeight = 420f;

    public CardDisplayHostSelectedEvent OnCardSelected;
    public NewCardDataSO CurrentCard { get; private set; }

    private UIDocument uiDocument;
    private CardDisplayController cardController;
    private UnityEngine.UI.Button uGuiButton; // [수정] Button -> UnityEngine.UI.Button으로 변경
    private RectTransform rectTransform;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        uGuiButton = GetComponent<UnityEngine.UI.Button>(); // [수정] Button -> UnityEngine.UI.Button으로 변경
        rectTransform = GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(cardWidth, cardHeight);

        if (uiDocument.rootVisualElement == null)
        {
            Debug.LogError("[CardDisplayHost] UIDocument의 rootVisualElement가 없습니다!");
            return;
        }

        cardController = new CardDisplayController(uiDocument.rootVisualElement);

        uGuiButton.onClick.AddListener(() => OnCardSelected.Invoke(this));
        uiDocument.rootVisualElement.RegisterCallback<ClickEvent>(evt => OnCardSelected.Invoke(this));
    }

    public void Setup(NewCardDataSO cardData)
    {
        if (cardData == null) { Debug.LogError("[CardDisplayHost] Setup에 전달된 cardData가 null입니다!"); return; }
        CurrentCard = cardData;
        if (cardController == null) { Debug.LogError("[CardDisplayHost] cardController가 null입니다!"); return; }
        cardController.SetData(cardData);
    }

    public void Setup(CardInstance cardInstance)
    {
        Setup(cardInstance.CardData);
    }

    public void SetHighlight(bool isSelected)
    {
        cardController.SetHighlight(isSelected);
    }

    public void SetScale(bool isSmall)
    {
        cardController.SetScale(isSmall);
    }
}