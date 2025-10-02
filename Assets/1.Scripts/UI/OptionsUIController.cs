using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsUIController : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("씬이 시작될 때 가장 먼저 선택될 버튼")]
    public GameObject firstSelectedButton;

    [Tooltip("메인 메뉴로 돌아갈 버튼")]
    public Button backButton;

    void Start()
    {
        // 뒤로가기 버튼에 기능 연결
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // 첫 번째 버튼에 포커스 설정
        StartCoroutine(SetInitialFocus());
    }

    private IEnumerator SetInitialFocus()
    {
        // EventSystem이 준비될 때까지 한 프레임 대기
        yield return null;

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // 혹시 모를 이전 포커스 해제
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            Debug.Log($"[OptionsUI] 초기 포커스를 '{firstSelectedButton.name}'으로 설정했습니다.");
        }
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("뒤로가기 버튼 클릭! 메인 메뉴로 돌아갑니다.");
        // GameManager를 통해 메인 메뉴 상태로 변경
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
    }
}