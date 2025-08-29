using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("���� ����")]
    [Tooltip("Project â���� PlayerInputActions.inputactions ������ ���⿡ �����ؾ� �մϴ�.")]
    [SerializeField] private InputActionAsset playerActionsAsset; // .inputactions '���赵' ���� ����


    public PlayerInputActions InputActions { get; private set; }
    public InputActionAsset ActionsAsset => playerActionsAsset;


    void Awake()
    {
        Debug.Log("[INPUT TRACE ��] InputManager.Awake: ����");
        ServiceLocator.Register<InputManager>(this);
        InputActions = new PlayerInputActions();
        Debug.Log("[INPUT TRACE] InputManager.Awake: PlayerInputActions �ν��Ͻ� ���� �Ϸ�.");
    }

    void OnEnable()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(gameManager.CurrentState);
        }
        Debug.Log("[INPUT TRACE] InputManager.OnEnable: GameManager�� ���� ���� �̺�Ʈ ���� �Ϸ�.");
    }

    void OnDisable()
    {
        var gameManager = ServiceLocator.Get<GameManager>();
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
        Debug.Log($"[INPUT TRACE] InputManager.HandleGameStateChanged: GameManager�κ��� '{newState}' ���� ����. �Է� ��� ���� ����.");
        if (newState == GameManager.GameState.Gameplay)
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