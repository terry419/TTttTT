using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// ���� �޴� UI�� ����°� ��ȣ�ۿ��� ����ϴ� ��ũ��Ʈ�Դϴ�.
/// InputManager �� GameManager�� �����Ͽ� ����� �Է��� �޾� �� ��ȯ ���� ó���մϴ�.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI ��� ����")]
    public RectTransform cursorSprite; // Ŀ�� ��������Ʈ RectTransform
    public Button optionsButton; // �ɼ� ��ư
    public Button startButton; // ���� ��ư
    public Button codexButton; // ���� ��ư
    public Button exitButton; // ������ ��ư
    public TextMeshProUGUI gameTitleText; // ���� ���� �ؽ�Ʈ
    public TextMeshProUGUI versionInfoText; // ���� ���� �ؽ�Ʈ

    private List<Button> menuButtons; // �޴� ��ư ����Ʈ
    private int currentButtonIndex = 0; // ���� ���õ� ��ư �ε���

    void Awake()
    {
        // �޴� ��ư ����Ʈ �ʱ�ȭ
        menuButtons = new List<Button>
        {
            optionsButton,
            startButton,
            codexButton,
            exitButton
        };

        // ���� ���� ���� (Inspector���� ������� ���)
        if (gameTitleText != null && string.IsNullOrEmpty(gameTitleText.text))
        {
            gameTitleText.text = "Game Title"; // �ӽ� ����
        }

        // ���� ���� ǥ��
        if (versionInfoText != null)
        {
            versionInfoText.text = "Version: " + Application.version; // Unity ������Ʈ �������� �����ɴϴ�.
        }
    }

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            // InputManager �̺�Ʈ ����
            InputManager.Instance.OnMove.AddListener(HandleMoveInput);
            InputManager.Instance.OnSubmit.AddListener(HandleSubmitInput);
            InputManager.Instance.OnCancel.AddListener(HandleCancelInput);
        }
        else
        {
            Debug.LogError("InputManager �ν��Ͻ��� ã�� �� �����ϴ�. MainMenuUI�� InputManager �̺�Ʈ�� �����մϴ�.");
        }
    }

    void Start()
    {
        // ���� �� Ŀ���� '����' ��ư�� ��ġ��ŵ�ϴ�.
        currentButtonIndex = menuButtons.IndexOf(startButton);
        if (currentButtonIndex < 0) currentButtonIndex = 0; // startButton�� ����Ʈ�� ���� ��� ���

        SetCursorPosition(menuButtons[currentButtonIndex]);
    }

    // WASD �� ȭ��ǥ Ű �Է� ó��
    private void HandleMoveInput(Vector2 input)
    {
        if (input.magnitude < 0.5f) return;

        int previousButtonIndex = currentButtonIndex;

        if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
        {
            if (input.y > 0) currentButtonIndex--; // ���� �̵�
            else currentButtonIndex++; // �Ʒ��� �̵�
        }
        else
        {
            if (input.x < 0) currentButtonIndex--; // �������� �̵�
            else currentButtonIndex++; // ���������� �̵�
        }

        // �ε����� ������ ����� �ʵ��� ��ȯ ó��
        if (currentButtonIndex < 0) currentButtonIndex = menuButtons.Count - 1;
        if (currentButtonIndex >= menuButtons.Count) currentButtonIndex = 0;

        if (previousButtonIndex != currentButtonIndex)
        {
            SetCursorPosition(menuButtons[currentButtonIndex]);
        }
    }

    // Enter Ű �Է� ó�� (��ư Ŭ��)
    private void HandleSubmitInput()
    {
        menuButtons[currentButtonIndex].onClick.Invoke();
    }

    // ESC Ű �Է� ó�� (������ �Ǵ� �ڷΰ���)
    private void HandleCancelInput()
    {
        exitButton.onClick.Invoke();
    }

    // Ŀ�� ��������Ʈ ��ġ�� �����մϴ�.
    private void SetCursorPosition(Button targetButton)
    {
        if (cursorSprite != null && targetButton != null)
        {
            cursorSprite.position = targetButton.transform.position;
        }
    }

    // --- ��ư Ŭ�� �̺�Ʈ �ڵ鷯�� --- //

    public void OnOptionsButtonClicked()
    {
        Debug.Log("�ɼ� ��ư Ŭ��!");
        // �ɼ� ���̳� �г��� ���� ����
        // GameManager.Instance.ChangeState(GameManager.GameState.Options); 
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("���� ��ư Ŭ��!");
        GameManager.Instance.ChangeState(GameManager.GameState.Allocation); // ĳ���� ����/�ɷ�ġ ��� ������ �̵�
    }

    public void OnCodexButtonClicked()
    {
        Debug.Log("���� ��ư Ŭ��!");
        // --- ������ �κ� ---
        GameManager.Instance.ChangeState(GameManager.GameState.Codex); // ���� ������ �̵�
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("������ ��ư Ŭ��! ������ �����մϴ�.");
        Application.Quit(); // ���ø����̼� ����
    }

    void OnDisable()
    {
        // ��ũ��Ʈ�� ��Ȱ��ȭ�� �� InputManager �̺�Ʈ ���� ����
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove.RemoveListener(HandleMoveInput);
            InputManager.Instance.OnSubmit.RemoveListener(HandleSubmitInput);
            InputManager.Instance.OnCancel.RemoveListener(HandleCancelInput);
        }
    }
}