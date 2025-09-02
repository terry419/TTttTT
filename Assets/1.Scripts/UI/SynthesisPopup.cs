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
        // UI가 활성화된 후 포커스를 설정하도록 코루틴을 여기서 시작합니다.
        StartCoroutine(SetInitialPopupFocus());
    }

    public void Initialize(NewCardDataSO baseCard, List<CardInstance> materialChoices, Action<CardInstance> confirmCallback, Action cancelCallback)
    {
        gameObject.SetActive(true);
        onConfirm = confirmCallback;
        onCancel = cancelCallback;
        selectedMaterialCard = null;

        if (titleText != null) titleText.text = $"Synthesize: {baseCard.basicInfo.cardName}";

        // 이전 재료 카드 UI 삭제
        foreach (var display in spawnedCardDisplays) Destroy(display.gameObject);
        spawnedCardDisplays.Clear();

        // 새 재료 카드로 UI 생성
        foreach (var cardInstance in materialChoices)
        {
            GameObject cardUI = Instantiate(cardDisplayPrefab, contentPanel.transform);
            CardDisplay cardDisplay = cardUI.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.Setup(cardInstance.CardData); // CardInstance의 CardData 사용
                cardDisplay.selectButton.onClick.AddListener(() => OnMaterialCardSelected(cardDisplay, cardInstance));
                spawnedCardDisplays.Add(cardDisplay);
            }
        }

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
        // 재료 선택 시 확인 버튼으로 포커스 이동
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

    private void SetupNavigation()
    {
        // 포커스가 팝업 밖으로 나가지 않도록 네비게이션 설정
        for (int i = 0; i < spawnedCardDisplays.Count; i++)
        {
            var currentSelectable = spawnedCardDisplays[i].selectButton;
            Navigation nav = currentSelectable.navigation;
            nav.mode = Navigation.Mode.Explicit;
            
            // 좌우 네비게이션 설정
            nav.selectOnLeft = (i == 0) ? cancelButton : spawnedCardDisplays[i - 1].selectButton;
            nav.selectOnRight = (i == spawnedCardDisplays.Count - 1) ? confirmButton : spawnedCardDisplays[i + 1].selectButton;
            
            currentSelectable.navigation = nav;
        }

        // 확인/취소 버튼 네비게이션 설정
        Navigation confirmNav = confirmButton.navigation;
        confirmNav.mode = Navigation.Mode.Explicit;
        confirmNav.selectOnLeft = spawnedCardDisplays.LastOrDefault()?.selectButton;
        confirmButton.navigation = confirmNav;

        Navigation cancelNav = cancelButton.navigation;
        cancelNav.mode = Navigation.Mode.Explicit;
        cancelNav.selectOnRight = spawnedCardDisplays.FirstOrDefault()?.selectButton;
        cancelButton.navigation = cancelNav;
    }

    private IEnumerator SetInitialPopupFocus()
    {
        yield return null; // UI가 완전히 활성화될 때까지 한 프레임 대기
        EventSystem.current.SetSelectedGameObject(null);
        
        // 가장 왼쪽 카드에 포커스
        if (spawnedCardDisplays.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(spawnedCardDisplays[0].gameObject);
        }
        else
        {
            // 카드가 없으면 취소 버튼에 포커스
            EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
        }
    }
}
