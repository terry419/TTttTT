using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using System.Collections; // <-- [1] Coroutine 사용을 위해 추가
using UnityEngine.EventSystems; // <-- [2] EventSystem 사용을 위해 추가

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
    private Action onCancel; // <-- [1] 취소 콜백을 저장할 변수 추가
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
    public void Initialize(string baseCardName, List<CardDataSO> materialChoices, Action<CardDataSO> confirmCallback, Action onCancelCallback)
    {
        gameObject.SetActive(true);
        onConfirm = confirmCallback;
        onCancel = onCancelCallback; // <-- [3] 전달받은 취소 콜백 저장
        selectedMaterialCard = null;
        UpdateConfirmButton();

        if (titleText != null) titleText.text = $"Select '{baseCardName}' to use as your composite card.";

        // 기존 UI 삭제
        foreach (var display in spawnedCardDisplays) Destroy(display.gameObject);
        spawnedCardDisplays.Clear();
        
        // ▼▼▼▼▼ [3] 포커스 설정을 위한 코루틴을 시작하는 코드 추가 ▼▼▼▼▼
        StartCoroutine(SetInitialPopupFocus());

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
        onCancel?.Invoke(); // <-- [4] 저장해둔 취소 콜백 실행
        gameObject.SetActive(false);
    }

    // ▼▼▼▼▼ [4] 아래 코루틴 함수 전체를 새로 추가 ▼▼▼▼▼
    /// <summary>
    /// 팝업이 나타난 후, UI 포커스를 팝업 내부의 버튼으로 설정합니다.
    /// </summary>
    private IEnumerator SetInitialPopupFocus()
    {
        // UI 요소들이 완전히 활성화될 시간을 벌기 위해 한 프레임 대기합니다.
        yield return null;

        // EventSystem의 현재 선택된 오브젝트를 null로 초기화하여 이전 포커스를 지웁니다.
        EventSystem.current.SetSelectedGameObject(null);

        // '취소' 버튼이 존재하고 활성화 상태라면, 그곳에 포커스를 맞춥니다.
        if (cancelButton != null && cancelButton.interactable)
        {
            EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
            Debug.Log("[SynthesisPopup] UI 포커스를 '취소' 버튼으로 설정했습니다.");
        }
    }
}
