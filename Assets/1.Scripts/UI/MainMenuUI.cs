using UnityEngine;
using UnityEngine.UI; // UI ������Ʈ ����� ���� �ʿ�
using TMPro; // TextMeshProUGUI ����� ���� �ʿ�
using System.Collections.Generic; // List ����� ���� �ʿ�

// ���� �޴� UI�� �����ϴ� ��ũ��Ʈ�Դϴ�.
// InputManager�� GameManager�� ����Ͽ� ����� �Է� �� �� ��ȯ�� ó���մϴ�.
public class MainMenuUI : MonoBehaviour
{
    [Header("UI ��� ����")]
    public RectTransform cursorSprite; // Ŀ�� ��������Ʈ�� RectTransform
    public Button optionsButton; // �ɼ� ��ư
    public Button startButton; // ���� ��ư
    public Button codexButton; // ���� ��ư
    public Button exitButton; // ������ ��ư
    public TextMeshProUGUI gameTitleText; // ���� ���� �ؽ�Ʈ
    public TextMeshProUGUI versionInfoText; // ���� ���� �ؽ�Ʈ

    private List<Button> menuButtons; // �޴� ��ư ���
    private int currentButtonIndex = 0; // ���� ���õ� ��ư�� �ε���

    void Awake()
    {
        // �޴� ��ư ��� �ʱ�ȭ
        menuButtons = new List<Button>
        {
            optionsButton,
            startButton,
            codexButton,
            exitButton
        };

        // ���� ���� ���� (����)
        if (gameTitleText == null)
        {
            gameTitleText.text = "Game Title"; // ���� ���� �������� ����
        }

        // ���� ���� ǥ��
        if (versionInfoText == null)
        {
            versionInfoText.text = "Version: " + Application.version; // Unity ������Ʈ ������ ���� ������ �����ɴϴ�.
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
            Debug.LogError("InputManager �ν��Ͻ��� ã�� �� �����ϴ�. MainMenuUI�� Inputy �̺�Ʈ�� ������ �� �����ϴ�.");
        }

    }

    void Start()
    {
        // �� �ε� �� Ŀ���� '����' ��ư�� ��ġ��ŵ�ϴ�.
        SetCursorPosition(startButton);
    }

    // WASD �� ����Ű �Է� ó��
    private void HandleMoveInput(Vector2 input)
    {
        // �Է� ������ ũ�Ⱑ 0�� ������ �Է��� �����մϴ�. (Deadzone)
        // �̴� ���̽�ƽ�� �̼��� ���� ������ ���� �ǵ�ġ �ʰ� Ŀ���� �����̴� ���� �����մϴ�.
        if (input.magnitude < 0.5f)
        {
            return;
        }

        int previousButtonIndex = currentButtonIndex;

        // ���� �Է�(W, S, ��/�Ʒ� ȭ��ǥ)�� ���� �Է�(A, D, ��/�� ȭ��ǥ) �� ��� ���� �Է��� �� ������ Ȯ���մϴ�.
        // �̸� ���� �밢�� �Է��� ������ �� ������� �ǵ��� �� ������ ������ �� �ֽ��ϴ�.
        if (Mathf.Abs(input.y) > Mathf.Abs(input.x))
        {
            if (input.y > 0) // ���� �̵�
            {
                currentButtonIndex--;
            }
            else // �Ʒ��� �̵�
            {
                currentButtonIndex++;
            }
        }
        else
        {
            if (input.x < 0) // �������� �̵� (���� �����ϰ� ó��)
            {
                currentButtonIndex--;
            }
            else // ���������� �̵� (�Ʒ��� �����ϰ� ó��)
            {
                currentButtonIndex++;
            }
        }

        // ��ư �ε����� ������ ����Ǿ��� ��쿡�� Ŀ�� ��ġ�� ������Ʈ�ϰ�, �ε��� ������ ����� �ʵ��� ó���մϴ�.
        // �̷��� �ϸ� ���ʿ��� �Լ� ȣ���� �ٿ� ������ �ణ�̳��� ������ �� �ֽ��ϴ�.
        if (previousButtonIndex != currentButtonIndex)
        {
            if (currentButtonIndex < 0)
            {
                currentButtonIndex = menuButtons.Count - 1; // ����� �� �Ʒ��� ��ȯ
            }
            else if (currentButtonIndex >= menuButtons.Count)
            {
                currentButtonIndex = 0; // ����� �� ���� ��ȯ
            }

            SetCursorPosition(menuButtons[currentButtonIndex]);
        }
    }

    // Enter �Է� ó�� (��ư ����)
    private void HandleSubmitInput()
    {
        // ���� ���õ� ��ư�� OnClick �̺�Ʈ�� ȣ���մϴ�.
        menuButtons[currentButtonIndex].onClick.Invoke();
    }

    // ESC �Է� ó�� (������ �Ǵ� �ڷ� ����)
    private void HandleCancelInput()
    {
        // ���� �޴������� ESC�� ������ ���� ���� ��ư�� �����ϰ� �۵��մϴ�.
        exitButton.onClick.Invoke();
    }

    // Ŀ�� ��������Ʈ�� ��ġ�� �����մϴ�.
    private void SetCursorPosition(Button targetButton)
    {
        if (cursorSprite != null && targetButton != null)
        {
            // ��ư�� ��ġ�� ���� Ŀ�� ��������Ʈ�� ��ġ�� �����մϴ�.
            // ��Ȯ�� ��ġ ������ UI �����ο� ���� �޶��� �� �ֽ��ϴ�.
            cursorSprite.position = targetButton.transform.position;
            // Ŀ�� ��������Ʈ�� ��ư ������ ��¦ �̵���Ű�� �������� �߰��� �� �ֽ��ϴ�.
            // cursorSprite.anchoredPosition = targetButton.GetComponent<RectTransform>().anchoredPosition + new Vector2(-targetButton.GetComponent<RectTransform>().sizeDelta.x / 2 - 10, 0);
        }
    }

    // --- ��ư Ŭ�� �̺�Ʈ �ڵ鷯 --- //

    public void OnOptionsButtonClicked()
    {
        Debug.Log("�ɼ� ��ư Ŭ��!");
        GameManager.Instance.ChangeState(GameManager.GameState.Pause); // �ɼ� ������ �̵� (Pause ���� �ɼ��� ���Ե� �� ����)
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("���� ��ư Ŭ��!");
        GameManager.Instance.ChangeState(GameManager.GameState.Allocation); // ĳ���� ���� �� �ɷ�ġ ��� ������ �̵�
    }

    public void OnCodexButtonClicked()
    {
        Debug.Log("���� ��ư Ŭ��!"); // Fixed: Added Debug.
        GameManager.Instance.ChangeState(GameManager.GameState.Reward); // ���� ������ �̵� (�ӽ÷� Reward ������ ����)
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("������ ��ư Ŭ��! ���� ����.");
        Application.Quit(); // ���ø����̼� ����
    }

    // --- ��Ÿ --- //

    void OnDestroy()
    {
        // ��ũ��Ʈ�� �ı��� �� InputManager �̺�Ʈ ���� ����
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove.RemoveListener(HandleMoveInput);
            InputManager.Instance.OnSubmit.RemoveListener(HandleSubmitInput);
            InputManager.Instance.OnCancel.RemoveListener(HandleCancelInput);
        }
    }
}