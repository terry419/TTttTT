using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 기본 Slider의 모든 기능을 상속받습니다.
public class CustomSlider : Slider
{
    private bool isEditing = false; // '수정 모드'인지 확인하는 변수

    // 수정 모드일 때 핸들 색상을 바꿔서 시각적으로 알려줍니다. (선택 사항)
    public Color editColor = Color.yellow;
    private Color normalColor;
    private Image handleImage;

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

    // 포커스를 잃었을 때 호출됩니다.
    public override void OnDeselect(BaseEventData eventData)
    {
        // 포커스를 잃으면 무조건 '수정 모드' 해제
        isEditing = false;
        if (handleImage != null) handleImage.color = normalColor;
        base.OnDeselect(eventData);
    }

    // '확인' 키 (Enter, 컨트롤러 A버튼 등)를 눌렀을 때 호출됩니다.
    public override void OnSubmit(BaseEventData eventData)
    {
        // isEditing 상태를 반전시킵니다 (true -> false, false -> true)
        isEditing = !isEditing;

        // 수정 모드 상태에 따라 핸들 색상 변경
        if (handleImage != null)
        {
            handleImage.color = isEditing ? editColor : normalColor;
        }

        base.OnSubmit(eventData);
    }

    // 방향키 입력이 들어왔을 때 호출됩니다.
    public override void OnMove(AxisEventData eventData)
    {
        if (isEditing)
        {
            // '수정 모드'일 경우, 기본 Slider의 OnMove 동작(값 변경)을 실행합니다.
            base.OnMove(eventData);
        }
        else
        {
            // '수정 모드'가 아닐 경우, 좌우 이동시에만 다음 UI로 포커스를 넘깁니다.
            // 값 변경 로직을 건너뛰는 것이 핵심입니다.
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    FindSelectableOnLeft()?.Select();
                    break;
                case MoveDirection.Right:
                    FindSelectableOnRight()?.Select();
                    break;
                default:
                    // 위/아래 이동은 기본 동작을 그대로 사용합니다.
                    base.OnMove(eventData);
                    break;
            }
        }
    }
}