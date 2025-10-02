using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsUIController : MonoBehaviour
{
    [Header("UI ���")]
    [Tooltip("���� ���۵� �� ���� ���� ���õ� ��ư")]
    public GameObject firstSelectedButton;

    [Tooltip("���� �޴��� ���ư� ��ư")]
    public Button backButton;

    void Start()
    {
        // �ڷΰ��� ��ư�� ��� ����
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // ù ��° ��ư�� ��Ŀ�� ����
        StartCoroutine(SetInitialFocus());
    }

    private IEnumerator SetInitialFocus()
    {
        // EventSystem�� �غ�� ������ �� ������ ���
        yield return null;

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Ȥ�� �� ���� ��Ŀ�� ����
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            Debug.Log($"[OptionsUI] �ʱ� ��Ŀ���� '{firstSelectedButton.name}'���� �����߽��ϴ�.");
        }
    }

    public void OnBackButtonClicked()
    {
        Debug.Log("�ڷΰ��� ��ư Ŭ��! ���� �޴��� ���ư��ϴ�.");
        // GameManager�� ���� ���� �޴� ���·� ����
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
    }
}