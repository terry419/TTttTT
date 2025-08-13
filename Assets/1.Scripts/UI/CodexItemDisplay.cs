using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 도감의 개별 항목(카드 또는 유물) 하나의 UI 표시를 담당합니다.
/// </summary>
public class CodexItemDisplay : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText; // 카드의 속성(물리/마법) 또는 "유물" 텍스트
    [SerializeField] private TextMeshProUGUI rarityText; // 등급/희귀도
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject lockedOverlay; // 잠금 상태일 때 표시할 오버레이
    [SerializeField] private Button hintButton; // 힌트 구매 버튼 (잠금 상태일 때만 활성화)

    private string currentItemId; // 현재 표시 중인 아이템의 ID
    private bool isCurrentlyUnlocked; // 현재 아이템의 해금 상태
    private string unlockedDescription; // 해금된 경우의 실제 설명

    /// <summary>
    /// 힌트 버튼 클릭 시 호출될 콜백을 설정합니다.
    /// </summary>
    public void SetHintButtonClickListener(string itemId, Action<string> onClick)
    {
        currentItemId = itemId;
        if (hintButton != null)
        {
            hintButton.onClick.RemoveAllListeners();
            hintButton.onClick.AddListener(() => onClick?.Invoke(currentItemId));
        }
    }

    /// <summary>
    /// 카드 데이터로 UI를 설정합니다.
    /// </summary>
    public void SetupForCard(CardDataSO card, bool isUnlocked)
    {
        isCurrentlyUnlocked = isUnlocked;
        unlockedDescription = card.effectDescription; // 실제 설명 저장

        if (hintButton != null) hintButton.gameObject.SetActive(!isUnlocked);
        if (lockedOverlay != null) lockedOverlay.SetActive(!isUnlocked);

        if (isUnlocked)
        {
            nameText.text = card.cardName;
            typeText.text = card.type.ToString();
            rarityText.text = card.rarity.ToString();
            descriptionText.text = card.effectDescription;
            // if (itemIcon != null && card.icon != null) itemIcon.sprite = card.icon;
        }
        else
        {
            nameText.text = "????????";
            typeText.text = "";
            rarityText.text = "";
            descriptionText.text = "";
            // if (itemIcon != null) itemIcon.sprite = null;
        }
    }

    /// <summary>
    /// 유물 데이터로 UI를 설정합니다.
    /// </summary>
    public void SetupForArtifact(ArtifactDataSO artifact, bool isUnlocked)
    {
        isCurrentlyUnlocked = isUnlocked;
        unlockedDescription = artifact.description; // 실제 설명 저장

        if (hintButton != null) hintButton.gameObject.SetActive(!isUnlocked);
        if (lockedOverlay != null) lockedOverlay.SetActive(!isUnlocked);

        if (isUnlocked)
        {
            nameText.text = artifact.artifactName;
            typeText.text = "유물";
            rarityText.text = artifact.rarity.ToString();
            descriptionText.text = artifact.description; 
            // if (itemIcon != null && artifact.icon != null) itemIcon.sprite = artifact.icon;
        }
        else
        {
            nameText.text = "????????";
            typeText.text = "";
            rarityText.text = "";
            descriptionText.text = "";
            // if (itemIcon != null) itemIcon.sprite = null;
        }
    }

    /// <summary>
    /// 힌트를 표시합니다. (잠금 상태에서만 호출)
    /// </summary>
    public void ShowHint()
    {
        if (!isCurrentlyUnlocked) // 잠금 상태일 때만 힌트 표시
        {
            descriptionText.text = unlockedDescription; // 실제 설명 표시
            if (hintButton != null) hintButton.gameObject.SetActive(false); // 힌트 버튼 비활성화
        }
    }
}
