using UnityEngine;
using TMPro; // TextMeshPro�� ����Ѵٸ� �߰�

[ExecuteAlways] // �����Ϳ����� �ǽð����� ��������� ���� ���� �߰�
public class CardLayoutController : MonoBehaviour
{
    [Header("UI Element References")]
    public RectTransform rarityImageRect;
    public RectTransform cardNameRect;
    public RectTransform cardIconImageRect;
    public RectTransform descriptionTextRect;

    [Header("Layout Ratios")]
    [Tooltip("Card Name�� ���� ����")]
    public float nameRatio = 1.0f;
    [Tooltip("Card Icon�� ���� ����")]
    public float iconRatio = 1.5f;
    [Tooltip("Description�� ���� ����")]
    public float descriptionRatio = 3.0f;

    [Header("Padding & Margins")]
    [Tooltip("RarityImage�� ���� Ȯ���� ��� ����")]
    public float topPadding = 80f;
    [Tooltip("ī�� �ϴ� ����")]
    public float bottomPadding = 20f;
    [Tooltip("ī�� �¿� ����")]
    public float horizontalPadding = 20f;

    // ī�� �ڽ��� RectTransform
    private RectTransform selfRect;

    // ��ũ��Ʈ�� Ȱ��ȭ�ǰų�, �ν����Ϳ��� ���� ����� ������ ȣ��
    private void OnEnable()
    {
        selfRect = GetComponent<RectTransform>();
        ApplyLayout();
    }

    // �����Ϳ��� ���� ����� ������ ȣ�� (ExecuteAlways �Ӽ� �ʿ�)
    private void OnValidate()
    {
        selfRect = GetComponent<RectTransform>();
        ApplyLayout();
    }

    // ī���� ũ�Ⱑ ����� ������ �ڵ����� ȣ��Ǵ� Unity �޽���
    private void OnRectTransformDimensionsChange()
    {
        if (selfRect != null)
        {
            ApplyLayout();
        }
    }

    //  �ٽ� ����: ���̾ƿ��� ����ϰ� �����ϴ� �Լ�
    public void ApplyLayout()
    {
        if (cardNameRect == null || cardIconImageRect == null || descriptionTextRect == null)
        {
            Debug.LogWarning("UI Element References are not set in CardLayoutController.");
            return;
        }

        // =================================================================
        // ## 1�ܰ�: ���� ���̾ƿ� - ������Ʈ�� ũ��� ��ġ�� ������ ����
        // =================================================================

        // 1. ��ü ��� ���� ����
        float totalAvailableHeight = selfRect.rect.height - topPadding - bottomPadding;
        if (totalAvailableHeight <= 0) return; // ���̰� 0 ���ϸ� ��� ����

        float totalAvailableWidth = selfRect.rect.width - (horizontalPadding * 2);
        if (totalAvailableWidth <= 0) return;

        // 2. ���� ���
        float totalRatio = nameRatio + iconRatio + descriptionRatio;

        // 3. �� ����� ���̸� ������ ���� ���
        float nameHeight = (nameRatio / totalRatio) * totalAvailableHeight;
        float iconHeight = (iconRatio / totalRatio) * totalAvailableHeight;
        float descriptionHeight = (descriptionRatio / totalRatio) * totalAvailableHeight;

        // 4. �� ����� ��ġ�� ũ�⸦ ���������� ���� (Top-Down ���)
        float currentYPosition = -topPadding; // ���� ��ġ�� ��� �е� �ٷ� �Ʒ�

        // CardName ��ġ �� ũ�� ����
        SetRectTransform(cardNameRect, currentYPosition, nameHeight, totalAvailableWidth);
        currentYPosition -= nameHeight;

        // CardIconImage ��ġ �� ũ�� ����
        SetRectTransform(cardIconImageRect, currentYPosition, iconHeight, totalAvailableWidth);
        currentYPosition -= iconHeight;

        // DescriptionText ��ġ �� ũ�� ����
        SetRectTransform(descriptionTextRect, currentYPosition, descriptionHeight, totalAvailableWidth);

        // =================================================================
        // ## 2�ܰ�: ������ ������ - TextMeshPro�� ���ϰ� ���α�
        // =================================================================
        // �� �ܰ�� ������ �ڵ尡 �ʿ� �����ϴ�.
        // 1�ܰ迡�� RectTransform�� ũ�Ⱑ �̹� �����Ǿ��� ������,
        // ���� �����ӿ��� UI�� �������� �� TextMeshPro�� Auto Size �����
        // �� ������ ���� �ȿ��� ������ ��Ʈ ũ�⸦ 'ã�Ƴ�' ���ۿ� �����ϴ�.
        // ��, ��Ʈ ũ�Ⱑ ���̾ƿ��� �����ϴ� ���� �ƴ϶�,
        // ���̾ƿ��� ��Ʈ ũ�⸦ �����ϰ� �Ǵ� ������ �ڹٲ� ���Դϴ�.
    }

    // RectTransform�� �����ϴ� ����� �Լ�
    private void SetRectTransform(RectTransform targetRect, float posY, float height, float width)
    {
        // ��Ŀ�� Top-Stretch�� ����
        targetRect.anchorMin = new Vector2(0.5f, 1);
        targetRect.anchorMax = new Vector2(0.5f, 1);
        targetRect.pivot = new Vector2(0.5f, 1); // �������� ��� �߾�����

        // ��ġ�� ũ�� ����
        targetRect.anchoredPosition = new Vector2(0, posY);
        targetRect.sizeDelta = new Vector2(width, height);
    }
}