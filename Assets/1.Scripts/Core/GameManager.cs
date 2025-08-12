using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // 코루틴 사용을 위해 추가

/// <summary>
/// 게임의 전체 상태 머신을 관리하며, 씬 전환과 주요 시스템 간 조율을 담당합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    // GameManager는 하나만 존재해야 하므로, 싱글톤으로 구현합니다.
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// 게임의 현재 상태를 나타내는 열거형입니다.
    /// </summary>
    public enum GameState
    {
        MainMenu,       // 메인 메뉴 화면
        Allocation,     // 캐릭터 선택 및 능력치 배분 화면
        Gameplay,       // 게임 플레이 화면
        Reward,         // 라운드 클리어 보상 화면
        Pause,          // 일시정지 화면
        Codex           // --- 수정된 부분: 도감 상태 추가 ---
    }

    // 게임의 현재 상태를 저장하는 변수입니다.
    public GameState CurrentState { get; private set; }

    // 씬 간에 전달될 플레이어 선택 데이터
    public CharacterDataSO SelectedCharacter { get; set; }

    // 씬 간에 전달될 플레이어 할당 포인트
    public int AllocatedPoints { get; set; }

    // 씬 전환을 담당하는 SceneTransitionManager 참조입니다.
    private SceneTransitionManager sceneTransitionManager;
    public UIManager uiManager; // UIManager 참조
    public GameObject mainCanvas; // 메인 UI Canvas를 참조할 필드

    // 스크립트 인스턴스가 로드될 때 호출되는 Unity 생명주기 함수입니다.
    private void Awake()
    {
        // 싱글톤 패턴 구현: 이미 인스턴스가 있다면 현재 GameObject를 파괴합니다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this; // 현재 인스턴스를 할당합니다.
        DontDestroyOnLoad(gameObject); // 씬이 변경되어도 이 GameObject가 파괴되지 않도록 합니다.
    }

    // 스크립트가 처음 활성화될 때 호출되는 Unity 생명주기 함수입니다.
    private void Start()
    {
        // SceneTransitionManager를 찾아 할당합니다.
        sceneTransitionManager = FindObjectOfType<SceneTransitionManager>();
        uiManager = FindObjectOfType<UIManager>(); // UIManager 참조 초기화

        // UIManager에 메인 캔버스 등록
        if (uiManager != null && mainCanvas != null)
        {
            uiManager.RegisterPanel("MainMenuPanel", mainCanvas);
        }

        // 게임이 시작되면 메인 메뉴 상태로 전환합니다.
        ChangeState(GameState.MainMenu);
    }

    /// <summary>
    /// 게임 상태를 변경하는 메서드입니다.
    /// </summary>
    /// <param name="newState">새로운 게임 상태</param>
    public void ChangeState(GameState newState)
    {
        // 현재 상태와 동일하면 아무것도 하지 않습니다.
        if (newState == CurrentState) return;

        CurrentState = newState; // 새로운 상태로 업데이트합니다.

        // 모든 UI 패널을 숨깁니다. (새로운 패널을 보여주기 전에)
        if (uiManager != null) uiManager.HideAllPanels();

        // 새로운 상태에 따라 적절한 씬을 로드하고 UI를 활성화합니다.
        switch (newState)
        {
            case GameState.MainMenu:
                sceneTransitionManager.LoadScene("MainMenu");
                if (uiManager != null) uiManager.ShowPanel("MainMenuPanel");
                Time.timeScale = 1; // 게임 시간 정상화
                break;
            case GameState.Allocation:
                // Allocation 상태는 이제 CharacterSelect 씬과 PointAllocation 씬을 모두 포함합니다.
                // CharacterSelectController에서 이 상태를 호출할 때, PointAllocation 씬으로 전환됩니다.
                sceneTransitionManager.LoadScene("PointAllocation"); // PointAllocation 씬 로드
                Time.timeScale = 1; // 게임 시간 정상화
                break;
            case GameState.Gameplay:
                sceneTransitionManager.LoadScene("Gameplay"); // Gameplay 씬 로드
                StartCoroutine(StartRoundAfterSceneLoad());
                break;
            case GameState.Reward:
                sceneTransitionManager.LoadScene("CardReward");
                if (uiManager != null) uiManager.ShowPanel("CardRewardPanel");
                Time.timeScale = 0; // 보상 화면에서는 게임 시간 정지
                break;
            case GameState.Pause:
                Time.timeScale = 0; // 게임 시간 정지
                if (uiManager != null) uiManager.ShowPanel("PausePanel");
                break;

            // --- 수정된 부분: Codex 케이스 추가 ---
            case GameState.Codex:
                sceneTransitionManager.LoadScene("Codex");
                if (uiManager != null) uiManager.ShowPanel("CodexPanel"); // Codex 씬의 UI 패널 이름
                Time.timeScale = 1; // 도감에서는 시간 정상화
                break;
        }
    }

    private IEnumerator StartRoundAfterSceneLoad()
    {
        // 씬이 완전히 로드될 때까지 한 프레임 대기합니다.
        yield return null;

        RoundManager roundManager = FindObjectOfType<RoundManager>();
        if (roundManager != null)
        {
            roundManager.StartRound();
        }
        else
        {
            Debug.LogError("Gameplay 씬에서 RoundManager를 찾을 수 없습니다!");
        }

        // HUD는 라운드 매니저가 활성화된 후 보여주는 것이 더 안정적일 수 있습니다.
        if (uiManager != null) uiManager.ShowPanel("GameplayHUD");
        Time.timeScale = 1f;
    }
}