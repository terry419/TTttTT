using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 메인 메뉴 UI의 입출력과 상호작용을 담당하는 스크립트입니다.
/// InputManager 및 GameManager와 연동하여 사용자 입력을 받아 씬 전환 등을 처리합니다.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI 요소 참조")]
    public RectTransform cursorSprite; // 커서 스프라이트 RectTransform
    public Button optionsButton; // 옵션 버튼
    public Button startButton; // 시작 버튼
    public Button codexButton; // 도감 버튼
    public Button exitButton; // 나가기 버튼
    public TextMeshProUGUI gameTitleText; // 게임 제목 텍스트
    public TextMeshProUGUI versionInfoText; // 버전 정보 텍스트

    private List<Button> menuButtons; // 메뉴 버튼 리스트
    private int currentButtonIndex = 0; // 현재 선택된 버튼 인덱스

    void Awake()
    {
        // 메뉴 버튼 리스트 초기화
        menuButtons = new List<Button>
        {
            optionsButton,
            startButton,
            codexButton,
            exitButton
        };

        // 게임 제목 설정 (Inspector에서 비어있을 경우)
        if (gameTitleText != null && string.IsNullOrEmpty(gameTitleText.text))
        {
            gameTitleText.text = "Game Title"; // 임시 제목
        }

        // 버전 정보 표시
        if (versionInfoText != null)
        {
            versionInfoText.text = "Version: " + Application.version; // Unity 프로젝트 버전에서 가져옵니다.
        }
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            // InputManager 이벤트 구독
            InputManager.Instance.OnMove.AddListener(HandleMoveInput);
            InputManager.Instance.OnSubmit.AddListener(HandleSubmitInput);
            InputManager.Instance.OnCancel.AddListener(HandleCancelInput);
        }
        else
        {
            Debug.LogError("InputManager 인스턴스를 찾을 수 없습니다. MainMenuUI는 InputManager 이벤트에 의존합니다.");
        }
    }

    void Start()
    {
        // 시작 시 커서를 '시작' 버튼에 위치시킵니다.
        currentButtonIndex = menuButtons.IndexOf(startButton);
        if (currentButtonIndex < 0) currentButtonIndex = 0; // startButton이 리스트에 없을 경우 대비

        SetCursorPosition(menuButtons[currentButtonIndex]);
    }

    // WASD 및 화살표 키 입력 처리
    private void HandleMoveInput(Vector2 input)
    {
        if (input.magnitude < 0.5f) return;

        int previousButtonIndex = currentButtonIndex;

        if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
        {
            if (input.y > 0) currentButtonIndex--; // 위로 이동
            else currentButtonIndex++; // 아래로 이동
        }
        else
        {
            if (input.x < 0) currentButtonIndex--; // 왼쪽으로 이동
            else currentButtonIndex++; // 오른쪽으로 이동
        }

        // 인덱스가 범위를 벗어나지 않도록 순환 처리
        if (currentButtonIndex < 0) currentButtonIndex = menuButtons.Count - 1;
        if (currentButtonIndex >= menuButtons.Count) currentButtonIndex = 0;

        if (previousButtonIndex != currentButtonIndex)
        {
            SetCursorPosition(menuButtons[currentButtonIndex]);
        }
    }

    // Enter 키 입력 처리 (버튼 클릭)
    private void HandleSubmitInput()
    {
        menuButtons[currentButtonIndex].onClick.Invoke();
    }

    // ESC 키 입력 처리 (나가기 또는 뒤로가기)
    private void HandleCancelInput()
    {
        exitButton.onClick.Invoke();
    }

    // 커서 스프라이트 위치를 설정합니다.
    private void SetCursorPosition(Button targetButton)
    {
        if (cursorSprite != null && targetButton != null)
        {
            cursorSprite.position = targetButton.transform.position;
        }
    }

    // --- 버튼 클릭 이벤트 핸들러들 --- //

    public void OnOptionsButtonClicked()
    {
        Debug.Log("옵션 버튼 클릭!");
        // 옵션 씬이나 패널을 여는 로직
        // GameManager.Instance.ChangeState(GameManager.GameState.Options); 
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("시작 버튼 클릭!");
        GameManager.Instance.ChangeState(GameManager.GameState.Allocation); // 캐릭터 선택/능력치 배분 씬으로 이동
    }

    public void OnCodexButtonClicked()
    {
        Debug.Log("도감 버튼 클릭!");
        // --- 수정된 부분 ---
        GameManager.Instance.ChangeState(GameManager.GameState.Codex); // 도감 씬으로 이동
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("나가기 버튼 클릭! 게임을 종료합니다.");
        Application.Quit(); // 어플리케이션 종료
    }

    void OnDisable()
    {
        // 스크립트가 비활성화될 때 InputManager 이벤트 구독 해제
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove.RemoveListener(HandleMoveInput);
            InputManager.Instance.OnSubmit.RemoveListener(HandleSubmitInput);
            InputManager.Instance.OnCancel.RemoveListener(HandleCancelInput);
        }
    }
}