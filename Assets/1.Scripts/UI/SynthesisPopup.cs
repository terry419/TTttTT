using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class SynthesisPopup : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI titleText;

    // [수정] 타입을 NewCardDataSO로 변경
    private Action<NewCardDataSO> onConfirm;
    private Action onCancel;
    private NewCardDataSO selectedMaterialCard;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();

    void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    // [수정] 파라미터 타입을 NewCardDataSO로 변경
    public void Initialize(string baseCardName, List<NewCardDataSO> materialChoices, Action<NewCardDataSO> confirmCallback, Action onCancelCallback)
    {
        gameObject.SetActive(true);
        onConfirm = confirmCallback;
        onCancel = onCancelCallback;
        selectedMaterialCard = null;
        UpdateConfirmButton();

        if (titleText != null) titleText.text = $"[기능 비활성화됨] Select '{baseCardName}' to synthesize.";

        foreach (var display in spawnedCardDisplays) Destroy(display.gameObject);
        spawnedCardDisplays.Clear();

        StartCoroutine(SetInitialPopupFocus());

        foreach (var card in materialChoices)
        {
            GameObject cardUI = Instantiate(cardDisplayPrefab, contentPanel.transform);
            CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Setup(card);
                cardDisplay.selectButton.onClick.RemoveAllListeners();
                cardDisplay.selectButton.onClick.AddListener(() => OnMaterialCardSelected(cardDisplay));
                spawnedCardDisplays.Add(cardDisplay);
            }
        }
    }

    private void OnMaterialCardSelected(CardDisplay selectedDisplay)
    {
        selectedMaterialCard = selectedDisplay.CurrentCard;
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
        onCancel?.Invoke();
        gameObject.SetActive(false);
    }

    private IEnumerator SetInitialPopupFocus()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        if (cancelButton != null && cancelButton.interactable)
        {
            EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
        }
    }
}