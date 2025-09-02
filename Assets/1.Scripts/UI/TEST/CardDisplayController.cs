// F:/unity/9th_/Assets/1.Scripts/UI/TEST/CardDisplayController.cs
using UnityEngine;
using UnityEngine.UIElements;
// MonoBehaviour가 아닌 일반 C# 클래스입니다.
// 이 클래스는 카드의 '상태'와 '동작'을 관리합니다.
public class CardDisplayController
{
    // --- UI 요소 참조 ---
    private VisualElement root;
    private Label cardNameLabel;
    private Label cardDescriptionLabel;
    private VisualElement cardNameWrapper;
    private VisualElement cardDescriptionWrapper;
    // --- 데이터 ---
    private string currentCardName;
    private string currentCardDescription;
    // 생성자: UIDocument의 rootVisualElement를 받아 초기화합니다.
    public CardDisplayController(VisualElement rootElement)
    {
        this.root = rootElement;
        // UXML에 정의된 name을 사용하여 각 UI 요소를 찾고 변수에 할당합니다.
        cardNameLabel = root.Q<Label>("card-name");
        cardDescriptionLabel = root.Q<Label>("card-description");
        cardNameWrapper = root.Q<VisualElement>("card-name-wrapper");
        cardDescriptionWrapper = root.Q<VisualElement>("card-description-wrapper");
        // UI의 크기나 모양이 변경될 때마다 텍스트 크기를 다시 계산하도록 이벤트를 등록합니다.
        // 이렇게 하면 카드가 처음 표시되거나 크기가 변경될 때 항상 올바른 폰트 크기를 유지할 수 있습니다.
        Debug.Log($"[CardDisplayController Init] cardNameLabel is {(cardNameLabel == null ? "NULL" : "Found")}");
        Debug.Log($"[CardDisplayController Init] cardDescriptionLabel is {(cardDescriptionLabel == null ? "NULL" : "Found")}");


        root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }
    // 카드 데이터를 UI에 채우는 메인 함수입니다.
    public void SetData(NewCardDataSO cardData)
    {
        if (cardData == null)
        {
            Debug.LogError("[Debug Flow 6/6] CardDisplayController: SetData called with NULL cardData!");
            return;
        }
        Debug.Log($"[Debug Flow 6/6] CardDisplayController: SetData received for '{cardData.name}'.");
        Debug.Log($"[Debug Flow 6/6] -> Description: '{cardData.basicInfo.effectDescription}'");
        Debug.Log($"[Debug Flow 6/6] -> Type: '{cardData.basicInfo.type.ToString()}'");

        // UXML의 다른 요소들도 여기서 데이터를 채울 수 있습니다.
        // 예: root.Q<Label>("card-level").text = $"Lv. {cardData.basicInfo.cardLevel}";
        root.Q<Label>("card-attribute").text = cardData.basicInfo.type.ToString();
        // 예: root.Q<VisualElement>("card-background").style.backgroundColor = cardData.basicInfo.rarityColor;
        // 텍스트를 변수에 저장해두고, GeometryChangedEvent에서 사용합니다.
        currentCardName = cardData.basicInfo.cardName;
        currentCardDescription = cardData.basicInfo.effectDescription;
        // 즉시 텍스트를 설정합니다.
        cardNameLabel.text = currentCardName;
        cardDescriptionLabel.text = currentCardDescription;
        // 텍스트 크기 조절 함수를 수동으로 한 번 호출해줍니다.

        if (cardNameLabel != null)
        {
            cardNameLabel.text = currentCardName;
        }
        else
        {
            Debug.LogError("[Debug Flow 6/6] CardDisplayController: cardNameLabel is NULL. Cannot set text.");
        }

        if (cardDescriptionLabel != null)
        {
            cardDescriptionLabel.text = currentCardDescription;
        }
        else
        {
            Debug.LogError("[Debug Flow 6/6] CardDisplayController: cardDescriptionLabel is NULL. Cannot set text.");
        }
        FitText();
    }
    // UI 요소의 크기나 위치가 결정/변경되었을 때 호출되는 콜백 함수입니다.
    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        FitText();
    }
    // 이름과 설명 텍스트의 크기를 조절하는 함수들을 호출합니다.
    private void FitText()
    {
        FitCardName();
        FitCardDescription();
    }
    // 카드 이름의 폰트 크기를 조절하는 함수입니다.
    // (한 줄 유지, 넘치면 축소)
    private void FitCardName()
    {
        if (string.IsNullOrEmpty(currentCardName) || cardNameWrapper.resolvedStyle.width <= 0) return;
        float initialFontSize = 16f;
        cardNameLabel.style.fontSize = initialFontSize;
        Vector2 measuredSize = cardNameLabel.MeasureTextSize(currentCardName, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
        while (measuredSize.x > cardNameWrapper.resolvedStyle.width && initialFontSize > 8f)
        {
            initialFontSize -= 0.5f;
            cardNameLabel.style.fontSize = initialFontSize;
            measuredSize = cardNameLabel.MeasureTextSize(currentCardName, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
        }
    }
    // 카드 설명의 폰트 크기를 조절하는 함수입니다.
    // (여러 줄, 넘치면 축소)
    private void FitCardDescription()
    {
        if (string.IsNullOrEmpty(currentCardDescription) || cardDescriptionWrapper.resolvedStyle.height <= 0) return;
        float initialFontSize = 12f;
        cardDescriptionLabel.style.fontSize = initialFontSize;
        Vector2 measuredSize = cardDescriptionLabel.MeasureTextSize(currentCardDescription, cardDescriptionWrapper.resolvedStyle.width, VisualElement.MeasureMode.AtMost, 0, VisualElement.MeasureMode.Undefined);
        while (measuredSize.y > cardDescriptionWrapper.resolvedStyle.height && initialFontSize > 8f)
        {
            initialFontSize -= 0.5f;
            cardDescriptionLabel.style.fontSize = initialFontSize;
            measuredSize = cardDescriptionLabel.MeasureTextSize(currentCardDescription, cardDescriptionWrapper.resolvedStyle.width, VisualElement.MeasureMode.AtMost, 0, VisualElement.MeasureMode.Undefined);
        }
    }

    public void SetScale(bool isSmall)
    {
        root.EnableInClassList("card-small", isSmall);
    }
    public void SetHighlight(bool isSelected)
    {
        // UXML/USS에 하이라이트 요소(.card-highlight-border 같은)를 추가하고 여기서 제어할 수 있습니다.
        // 예: root.Q("highlight-border").style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
