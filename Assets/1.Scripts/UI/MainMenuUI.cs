// --- 파일명: MainMenuUI.cs (정리 후) ---
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// [수정] 더 이상 수동으로 버튼 리스트를 관리하거나 커서를 움직이지 않습니다.
public class MainMenuUI : MonoBehaviour
{
    [Header("UI 요소 참조")]
    public Button optionsButton;
    public Button startButton;
    public Button codexButton;
    public Button exitButton;
    public TextMeshProUGUI versionInfoText;

    public RectTransform cursorSprite; // [추가] Inspector에서 커서 스프라이트 할당

    void OnEnable()
    {
        // 씬이 활성화될 때 UICursorManager에 내 커서를 등록
        if (UICursorManager.Instance != null)
        {
            UICursorManager.Instance.RegisterCursor(cursorSprite);
        }
    }

    void Start()
    {
        // 버튼의 OnClick 이벤트는 여기서 직접 연결해주는 것이 좋습니다.
        startButton.onClick.AddListener(OnStartButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        codexButton.onClick.AddListener(OnCodexButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);

        if (versionInfoText == null)
        {
            versionInfoText.text = "Version: " + Application.version;
        }
    }

    // OnEnable, OnDisable, HandleMoveInput, HandleSubmitInput, SetCursorPosition 등
    // InputManager와 커서 위치를 제어하던 함수들은 모두 삭제합니다.

    // --- 버튼 클릭 시 호출될 함수들 ---
    public void OnOptionsButtonClicked()
    {
        Debug.Log("옵션 버튼 클릭!");
        // UIManager.Instance.ShowPanel("OptionsPanel"); // 예시: 옵션 패널 열기
    }

    public void OnStartButtonClicked()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.CharacterSelect);
    }

    public void OnCodexButtonClicked()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Codex);
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }
}