// 파일 경로: ./TTttTT/Assets/1/Scripts/UI/InteractiveSlider.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractiveSlider : Slider, ISubmitHandler, ICancelHandler
{
    private enum SliderState { Normal, Editing }

    [Header("시각적 피드백")]
    [SerializeField] private Color editingColor = Color.yellow;

    private Color normalColor;
    private Image handleImage;
    private SliderState currentState = SliderState.Normal;

    protected override void Awake()
    {
        base.Awake();
        if (handleRect != null)
        {
            handleImage = handleRect.GetComponent<Image>();
            if (handleImage != null)
            {
                normalColor = handleImage.color;
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = SliderState.Normal;
        UpdateVisuals();
    }

    // [수정] 불필요한 new 키워드 제거
    public void OnSubmit(BaseEventData eventData)
    {
        currentState = (currentState == SliderState.Normal) ? SliderState.Editing : SliderState.Normal;
        UpdateVisuals();
        Debug.Log($"[InteractiveSlider] OnSubmit 호출. 상태 토글 -> {currentState}");
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (currentState == SliderState.Editing)
        {
            currentState = SliderState.Normal;
            UpdateVisuals();
            Debug.Log($"[InteractiveSlider] OnCancel 호출. 'Editing' -> 'Normal' 상태로 변경.");
        }
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (currentState == SliderState.Editing)
        {
            Debug.Log($"[InteractiveSlider] 'Editing' 상태에서 OnMove 호출: {eventData.moveDir}");
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                case MoveDirection.Up:
                    UpdateSliderValue(true);
                    break;

                case MoveDirection.Left:
                case MoveDirection.Down:
                    UpdateSliderValue(false);
                    break;
            }
        }
        else
        {
            Selectable nextSelectable = null;
            switch (eventData.moveDir)
            {
                case MoveDirection.Up:
                    nextSelectable = this.FindSelectableOnUp();
                    break;
                case MoveDirection.Down:
                    nextSelectable = this.FindSelectableOnDown();
                    break;
                case MoveDirection.Left:
                    nextSelectable = this.FindSelectableOnLeft();
                    break;
                case MoveDirection.Right:
                    nextSelectable = this.FindSelectableOnRight();
                    break;
            }

            if (nextSelectable != null)
            {
                EventSystem.current.SetSelectedGameObject(nextSelectable.gameObject, eventData);
            }
        }
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        if (currentState == SliderState.Editing)
        {
            currentState = SliderState.Normal;
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        if (handleImage == null) return;
        handleImage.color = (currentState == SliderState.Editing) ? editingColor : normalColor;
    }

    private void UpdateSliderValue(bool positive)
    {
        float step = 0.1f;
        float newValue = this.value + (positive ? step : -step);
        newValue = Mathf.Round(newValue * 10f) / 10f;
        this.value = newValue;
    }
}