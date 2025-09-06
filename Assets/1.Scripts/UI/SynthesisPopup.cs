// 파일 경로: Assets/1.Scripts/UI/SynthesisPopup.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using System.Linq;

public class SynthesisPopup : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private GameObject cardDisplayPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI titleText;

    private Action<CardInstance> onConfirm;
    private Action onCancel;
    private CardInstance selectedMaterialCard;
    private List<CardDisplay> spawnedCardDisplays = new List<CardDisplay>();

    void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        StartCoroutine(SetInitialPopupFocus());
    }

    public void Initialize(NewCardDataSO baseCard, List<CardInstance> materialChoices, Action<CardInstance> confirmCallback, Action cancelCallback)
    {
        gameObject.SetActive(true);
        onConfirm = confirmCallback;
        onCancel = cancelCallback;
        selectedMaterialCard = null;

        if (titleText != null) titleText.text = $"Synthesize: {baseCard.basicInfo.cardName}";

        foreach (var display in spawnedCardDisplays) Destroy(display.gameObject);
        spawnedCardDisplays.Clear();

        foreach (var cardInstance in materialChoices)
        {
            GameObject cardUI = Instantiate(cardDisplayPrefab, contentPanel.transform);
            CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Setup(cardInstance);
                cardDisplay.selectButton.onClick.AddListener(() => OnMaterialCardSelected(cardDisplay, cardInstance));
                spawnedCardDisplays.Add(cardDisplay);
            }
        }

        // ▼▼▼ [핵심 수정] 네비게이션 설정 함수 호출 ▼▼▼
        SetupNavigation();
        UpdateConfirmButton();
    }

    private void OnMaterialCardSelected(CardDisplay selectedDisplay, CardInstance selectedInstance)
    {
        selectedMaterialCard = selectedInstance;
        foreach (var display in spawnedCardDisplays)
        {
            display.SetHighlight(display == selectedDisplay);
        }
        UpdateConfirmButton();
        EventSystem.current.SetSelectedGameObject(confirmButton.gameObject);
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

    /// <summary>
    /// [새로 추가된 함수] 생성된 카드와 하단 버튼들의 네비게이션을 수동으로 연결합니다.
    /// </summary>
    private void SetupNavigation()
    {
        if (spawnedCardDisplays.Count == 0) return;

        int columnCount = 4; // Grid Layout Group의 Constraint Count와 동일한 값

        for (int i = 0; i < spawnedCardDisplays.Count; i++)
        {
            var currentButton = spawnedCardDisplays[i].selectButton;
            var nav = new Navigation { mode = Navigation.Mode.Explicit };

            // 위쪽 연결 (i - 4)
            nav.selectOnUp = (i < columnCount) ? null : spawnedCardDisplays[i - columnCount].selectButton;

            // 아래쪽 연결 (i + 4)
            if (i + columnCount >= spawnedCardDisplays.Count) // 마지막 줄 카드인 경우
            {
                nav.selectOnDown = cancelButton; // 일단 Cancel 버튼으로 연결
            }
            else
            {
                nav.selectOnDown = spawnedCardDisplays[i + columnCount].selectButton;
            }

            // 왼쪽 연결 (i - 1)
            nav.selectOnLeft = (i % columnCount == 0) ? null : spawnedCardDisplays[i - 1].selectButton;

            // 오른쪽 연결 (i + 1)
            nav.selectOnRight = ((i + 1) % columnCount == 0 || i + 1 >= spawnedCardDisplays.Count) ? null : spawnedCardDisplays[i + 1].selectButton;

            currentButton.navigation = nav;
        }

        // 하단 버튼(Cancel, Confirm)들의 위쪽 네비게이션 설정
        var cancelNav = cancelButton.navigation;
        cancelNav.mode = Navigation.Mode.Explicit;
        cancelNav.selectOnUp = spawnedCardDisplays.LastOrDefault(c => c.transform.GetSiblingIndex() % columnCount < 2)?.selectButton; // 왼쪽 카드들 중 마지막 줄
        cancelButton.navigation = cancelNav;

        var confirmNav = confirmButton.navigation;
        confirmNav.mode = Navigation.Mode.Explicit;
        confirmNav.selectOnUp = spawnedCardDisplays.LastOrDefault(c => c.transform.GetSiblingIndex() % columnCount >= 2)?.selectButton; // 오른쪽 카드들 중 마지막 줄
        confirmButton.navigation = confirmNav;
    }

    private IEnumerator SetInitialPopupFocus()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        if (spawnedCardDisplays.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(spawnedCardDisplays[0].gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
        }
    }
}