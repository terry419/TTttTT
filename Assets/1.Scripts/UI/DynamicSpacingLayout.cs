// 파일 경로: Assets/1.Scripts/UI/DynamicSpacingLayout.cs
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic; // ToList()를 위해 추가

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class DynamicSpacingLayout : MonoBehaviour
{
    private HorizontalLayoutGroup layoutGroup;
    private RectTransform rectTransform;

    void Awake()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void RecalculateSpacing()
    {
        if (layoutGroup == null || rectTransform == null) return;

        var activeChildren = transform.Cast<Transform>().Where(t => t.gameObject.activeSelf).ToList();
        int childCount = activeChildren.Count;


        if (childCount <= 1)
        {
            layoutGroup.spacing = 0;
            return;
        }

        float containerWidth = rectTransform.rect.width - layoutGroup.padding.left - layoutGroup.padding.right;

        float totalChildrenWidth = 0f;
        int childIndex = 0;
        foreach (var child in activeChildren)
        {
            float childWidth = 0;
            var layoutElement = child.GetComponent<LayoutElement>();
            if (layoutElement != null && layoutElement.preferredWidth > 0)
            {
                childWidth = layoutElement.preferredWidth;
                totalChildrenWidth += childWidth;
            }
            else
            {
                var childRect = child.GetComponent<RectTransform>();
                childWidth = childRect.rect.width;
                totalChildrenWidth += childWidth;
            }
            childIndex++;
        }

        float remainingSpace = containerWidth - totalChildrenWidth;

        int gapCount = childCount - 1;
        float calculatedSpacing = (remainingSpace > 0) ? (remainingSpace / gapCount) : 0;

        layoutGroup.spacing = calculatedSpacing;
    }
}