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
    /// <summary>
    /// GameManager�� �ڽ��� �غ� ������ �� �� �Լ��� ȣ���Ͽ� �̺�Ʈ ������ ���۽�ŵ�ϴ�.
    /// </summary>
    public void LinkToGameManager(GameManager gameManager)
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(gameManager.CurrentState); // �ʱ� ���� ����
            Debug.Log("[INPUT TRACE] InputManager: GameManager�� ���� ���� �̺�Ʈ ���� �Ϸ�.");
        }
    }

    /// <summary>
    /// GameManager�� �ı��� �� ȣ���Ͽ� �̺�Ʈ ������ �����ϰ� �����մϴ�.
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