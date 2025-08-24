using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System; // Action 사용을 위해 필요

/// <summary>
/// 게임 전체에서 사용될 수 있는 범용 팝업 UI를 관리하는 싱글톤 클래스입니다.
/// 간단한 오류 메시지를 일정 시간 동안 보여주거나, 
/// 사용자에게 확인/취소 선택지를 제공하는 확인(Confirm) 팝업을 생성합니다.
/// </summary>
public class PopupController : MonoBehaviour
{
    [Header("오류 팝업 참조")]
    [SerializeField] private GameObject errorPopupPanel; // 오류 메시지 패널
    [SerializeField] private TextMeshProUGUI errorText; // 오류 메시지 텍스트

    [Header("확인 팝업 참조")]
    [SerializeField] private GameObject confirmPopupPanel; // 확인 팝업 패널
    [SerializeField] private TextMeshProUGUI confirmText; // 확인 메시지 텍스트
    [SerializeField] private Button confirmYesButton; // '예' 버튼
    [SerializeField] private Button confirmNoButton; // '아니오' 버튼

    private Action onConfirmYes; // '예' 버튼 클릭 시 실행될 콜백
    private Action onConfirmNo; // '아니오' 버튼 클릭 시 실행될 콜백

    void Awake()
    {
        ServiceLocator.Register<PopupController>(this);
        DontDestroyOnLoad(gameObject);

        // 버튼 리스너 초기화
        confirmYesButton.onClick.AddListener(OnConfirmYesClicked);
        confirmNoButton.onClick.AddListener(OnConfirmNoClicked);

        // 초기에는 모든 팝업을 비활성화
        errorPopupPanel.SetActive(false);
        confirmPopupPanel.SetActive(false);
    }

    /// <summary>
    /// 지정된 시간 동안 오류 메시지 팝업을 표시합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="duration">표시 시간(초)</param>
    public void ShowError(string message, float duration = 2f)
    {
        errorText.text = message;
        StartCoroutine(ShowErrorRoutine(duration));
    }

    private IEnumerator ShowErrorRoutine(float duration)
    {
        errorPopupPanel.SetActive(true);
        yield return new WaitForSeconds(duration);
        errorPopupPanel.SetActive(false);
    }

    /// <summary>
    /// 확인/취소 버튼이 있는 확인 팝업을 표시합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="onYes">'예'를 눌렀을 때 실행될 동작</param>
    /// <param name="onNo">'아니오'를 눌렀을 때 실행될 동작 (선택 사항)</param>
    public void ShowConfirm(string message, Action onYes, Action onNo = null)
    {
        confirmText.text = message;
        onConfirmYes = onYes;
        onConfirmNo = onNo;
        confirmPopupPanel.SetActive(true);
    }

    private void OnConfirmYesClicked()
    {
        // '예' 콜백이 있다면 실행
        onConfirmYes?.Invoke();
        confirmPopupPanel.SetActive(false);
    }

    private void OnConfirmNoClicked()
    {
        // '아니오' 콜백이 있다면 실행
        onConfirmNo?.Invoke();
        confirmPopupPanel.SetActive(false);
    }
}
