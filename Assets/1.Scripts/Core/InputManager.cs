using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("���� ����")]
    [Tooltip("Project â���� PlayerInputActions.inputactions ������ ���⿡ �����ؾ� �մϴ�.")]
    [SerializeField] private InputActionAsset playerActionsAsset; // .inputactions '���赵' ���� ����


    public PlayerInputActions InputActions { get; private set; }
    public InputActionAsset ActionsAsset => playerActionsAsset;

    private GameManager gameManager;


    void Awake()
    {
        Debug.Log("[INPUT TRACE ��] InputManager.Awake: ����");
        ServiceLocator.Register<InputManager>(this);
        InputActions = new PlayerInputActions();
        Debug.Log("[INPUT TRACE] InputManager.Awake: PlayerInputActions �ν��Ͻ� ���� �Ϸ�.");
    }
    /// <summary>
    /// GameManager�� �ڽ��� �غ� ������ �� �� �Լ��� ȣ���Ͽ� �̺�Ʈ ������ ���۽�ŵ�ϴ�.
    /// </summary>
    public void LinkToGameManager(GameManager gm)
    {
        if (gm != null)
        {
            this.gameManager = gm; //
            this.gameManager.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(this.gameManager.CurrentState);
            Debug.Log("[INPUT TRACE] InputManager: GameManager .");

            if (InputActions != null)
            {
                InputActions.Gameplay.Pause.performed += OnPausePerformed;
                InputActions.UI.Cancel.performed += OnPausePerformed;
            }
        }
    }

    /// <summary>
    /// GameManager�� �ı��� �� ȣ���Ͽ� �̺�Ʈ ������ �����ϰ� �����մϴ�.
    /// </summary>
    public void UnlinkFromGameManager(GameManager gm)
    {
        if (gm != null)
        {
            gm.OnGameStateChanged -= HandleGameStateChanged;
        }

        if (InputActions != null)
        {
            InputActions.Gameplay.Pause.performed -= OnPausePerformed;
            InputActions.UI.Cancel.performed -= OnPausePerformed;
        }

        InputActions?.Disable();
        this.gameManager = null;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (gameManager == null) return;

        if (gameManager.CurrentState == GameManager.GameState.Gameplay)
        {
            Debug.Log("[InputManager] Pause action performed in Gameplay. Pausing.");
            gameManager.PauseGame();
        }
        else if (gameManager.CurrentState == GameManager.GameState.Pause)
        {
            Debug.Log("[InputManager] Pause action performed in Pause state. Resuming.");
            gameManager.ResumeGame(); 
        }
    }

    private void OnDestroy()
    {
        InputActions?.Dispose();
    }

    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        Debug.Log($"[INPUT TRACE] InputManager.HandleGameStateChanged: GameManager�κ��� '{newState}' ���� ����. �Է� ��� ���� ����.");
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
        Debug.Log("[INPUT TRACE] InputManager: Gameplay �׼� �� Ȱ��ȭ.");
    }

    public void EnableUIControls()
    {
        InputActions.Gameplay.Disable();
        InputActions.UI.Enable();
        Debug.Log("[INPUT TRACE] InputManager: UI �׼� �� Ȱ��ȭ.");
    }
}