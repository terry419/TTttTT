using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// �⺻ Slider�� ��� ����� ��ӹ޽��ϴ�.
public class CustomSlider : Slider
{
    private bool isEditing = false; // '���� ���'���� Ȯ���ϴ� ����

    // ���� ����� �� �ڵ� ������ �ٲ㼭 �ð������� �˷��ݴϴ�. (���� ����)
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

    // ��Ŀ���� �Ҿ��� �� ȣ��˴ϴ�.
    public override void OnDeselect(BaseEventData eventData)
    {
        // ��Ŀ���� ������ ������ '���� ���' ����
        isEditing = false;
        if (handleImage != null) handleImage.color = normalColor;
        base.OnDeselect(eventData);
    }

    // 'Ȯ��' Ű (Enter, ��Ʈ�ѷ� A��ư ��)�� ������ �� ȣ��˴ϴ�.
    public override void OnSubmit(BaseEventData eventData)
    {
        // isEditing ���¸� ������ŵ�ϴ� (true -> false, false -> true)
        isEditing = !isEditing;

        // ���� ��� ���¿� ���� �ڵ� ���� ����
        if (handleImage != null)
        {
            handleImage.color = isEditing ? editColor : normalColor;
        }

        base.OnSubmit(eventData);
    }

    // ����Ű �Է��� ������ �� ȣ��˴ϴ�.
    public override void OnMove(AxisEventData eventData)
    {
        if (isEditing)
        {
            // '���� ���'�� ���, �⺻ Slider�� OnMove ����(�� ����)�� �����մϴ�.
            base.OnMove(eventData);
        }
        else
        {
            // '���� ���'�� �ƴ� ���, �¿� �̵��ÿ��� ���� UI�� ��Ŀ���� �ѱ�ϴ�.
            // �� ���� ������ �ǳʶٴ� ���� �ٽ��Դϴ�.
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    FindSelectableOnLeft()?.Select();
                    break;
                case MoveDirection.Right:
                    FindSelectableOnRight()?.Select();
                    break;
                default:
                    // ��/�Ʒ� �̵��� �⺻ ������ �״�� ����մϴ�.
                    base.OnMove(eventData);
                    break;
            }
        }
    }
}