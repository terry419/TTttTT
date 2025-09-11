using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("참조 설정")]
    [Tooltip("Project 창에서 PlayerInputActions.inputactions 에셋을 여기에 연결해야 합니다.")]
    [SerializeField] private InputActionAsset playerActionsAsset; // .inputactions '설계도' 파일 참조


    public PlayerInputActions InputActions { get; private set; }
    public InputActionAsset ActionsAsset => playerActionsAsset;


    void Awake()
    {
        Debug.Log("[INPUT TRACE ①] InputManager.Awake: 시작");
        ServiceLocator.Register<InputManager>(this);
        InputActions = new PlayerInputActions();
        Debug.Log("[INPUT TRACE] InputManager.Awake: PlayerInputActions 인스턴스 생성 완료.");
    }
    /// <summary>
    /// GameManager가 자신의 준비가 끝났을 때 이 함수를 호출하여 이벤트 구독을 시작시킵니다.
    /// </summary>
    public void LinkToGameManager(GameManager gameManager)
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(gameManager.CurrentState); // 초기 상태 적용
            Debug.Log("[INPUT TRACE] InputManager: GameManager의 상태 변경 이벤트 구독 완료.");
        }
    }

    /// <summary>
    /// GameManager가 파괴될 때 호출하여 이벤트 구독을 안전하게 해제합니다.
    /// </summary>
    public void UnlinkFromGameManager(GameManager gameManager)
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
        InputActions?.Disable();
    }

    private void OnDestroy()
    {
        InputActions?.Dispose();
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        Debug.Log($"[INPUT TRACE] InputManager.HandleGameStateChanged: GameManager로부터 '{newState}' 상태 수신. 입력 모드 변경 실행.");
        if (newState == GameManager.GameState.Gameplay || newState == GameManager.GameState.BossStage)
        {
            EnableGameplayControls();
        }
        else
        {
            EnableUIControls();
        }
    }

    public void EnableGameplayControls()
    {
        InputActions.UI.Disable();
        InputActions.Gameplay.Enable();
        Debug.Log("[INPUT TRACE] InputManager: Gameplay 액션 맵 활성화.");
    }

    public void EnableUIControls()
    {
        InputActions.Gameplay.Disable();
        InputActions.UI.Enable();
        Debug.Log("[INPUT TRACE] InputManager: UI 액션 맵 활성화.");
    }
}