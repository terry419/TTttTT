// F:/unity/9th_/Assets/1.Scripts/UI/TEST/CardDisplayHost.cs
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

[System.Serializable]
public class CardDisplayHostSelectedEvent : UnityEvent<CardDisplayHost> { }

/// <summary>
/// UI Toolkit으로 만든 새로운 카드 프리펩에 붙는 MonoBehaviour입니다.
/// 기존 UGUI 시스템(CardRewardUIManager 등)과 새로운 UI Toolkit 카드 사이의 '소통 창구' 역할을 합니다.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class CardDisplayHost : MonoBehaviour
{
    // --- Public API (기존 CardDisplay.cs와 동일하게 구성) ---

    public CardDisplayHostSelectedEvent OnCardSelected;
    public NewCardDataSO CurrentCard { get; private set; }

    // --- 내부 참조 및 로직 ---

    private UIDocument uiDocument;
    private CardDisplayController cardController;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument.rootVisualElement == null)
        {
            Debug.LogError("[CardDisplayHost] UIDocument의 rootVisualElement가 없습니다!");
            return;
        }

        cardController = new CardDisplayController(uiDocument.rootVisualElement);

        uiDocument.rootVisualElement.RegisterCallback<ClickEvent>(evt => OnCardSelected.Invoke(this));
    }

    public void Setup(NewCardDataSO cardData)
    {
        CurrentCard = cardData;
        cardController.SetData(cardData);
    }

    public void Setup(CardInstance cardInstance)
    {
        Setup(cardInstance.CardData);
        // Controller에 레벨 표시 로직을 추가할 수 있습니다.
        // 예: cardController.SetEnhancementLevel(cardInstance.EnhancementLevel);
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
