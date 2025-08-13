using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

/// <summary>
/// 카드 합성에 사용할 재료를 선택하는 팝업 UI를 제어합니다.
/// </summary>
public class SynthesisPopup : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private GameObject contentPanel; // 카드들이 표시될 부모 오브젝트
    [SerializeField] private GameObject cardDisplayPrefab; // 재료 카드를 표시할 UI 프리팹 (CardDisplay.cs 포함)
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI titleText;

    private Action<CardDataSO> onConfirm; // 확인 시 선택된 카드를 전달할 콜백
    private CardDataSO selectedMaterialCard;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();

    void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    /// <summary>
    /// 팝업을 초기화하고 재료 카드 목록을 표시합니다.
    /// </summary>
    /// <param name="baseCardName">합성의 기준이 되는 카드 이름</param>
    /// <param name="materialChoices">재료로 사용할 수 있는 카드 목록</param>
    /// <param name="confirmCallback">확인 버튼 클릭 시 실행될 콜백</param>
    public void Initialize(string baseCardName, List<CardDataSO> materialChoices, Action<CardDataSO> confirmCallback)
    {
        gameObject.SetActive(true);
        onConfirm = confirmCallback;
        selectedMaterialCard = null;
        UpdateConfirmButton();

        if (titleText != null) titleText.text = $"'{baseCardName}'과(와) 합성할 카드를 선택하세요";

        // 기존 UI 삭제
        foreach (var display in spawnedCardDisplays) Destroy(display.gameObject);
        spawnedCardDisplays.Clear();

        // 재료 카드 목록 UI 생성
        foreach (var card in materialChoices)
        {
            GameObject cardUI = Instantiate(cardDisplayPrefab, contentPanel.transform);
            CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Setup(card);
                // 팝업 내의 카드 클릭 시 OnMaterialCardSelected를 호출하도록 리스너 재설정
                cardDisplay.selectButton.onClick.RemoveAllListeners();
                cardDisplay.selectButton.onClick.AddListener(() => OnMaterialCardSelected(cardDisplay));
                spawnedCardDisplays.Add(cardDisplay);
            }
        }
    }

    /// <summary>
    /// 팝업 내에서 재료 카드가 선택되었을 때 호출됩니다.
    /// </summary>
    private void OnMaterialCardSelected(CardDisplay selectedDisplay)
    {
        selectedMaterialCard = selectedDisplay.GetCurrentCard();

        // 하이라이트 처리
        foreach (var display in spawnedCardDisplays)
        {
            display.SetHighlight(display == selectedDisplay);
        }
        UpdateConfirmButton();
    }

    private void UpdateConfirmButton()
    {
        confirmButton.interactable = (selectedMaterialCard != null);
    }

    private void OnConfirmClicked()
    {
        if (selectedMaterialCard != null)
        {
            onConfirm?.Invoke(selectedMaterialCard);
        }
        gameObject.SetActive(false);
    }

    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }
}
