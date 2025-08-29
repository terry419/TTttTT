using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(InputSystemUIInputModule))]
public class EventSystemInputActionsLinker : MonoBehaviour
{
    void Start()
    {
        var uiInputModule = GetComponent<InputSystemUIInputModule>();
        if (uiInputModule != null)
        {
            var inputManager = ServiceLocator.Get<InputManager>();
            if (inputManager != null)
            {
                // ������ �ν��Ͻ��� InputActions �Ӽ��� �Ҵ��մϴ�.
                uiInputModule.actionsAsset = inputManager.ActionsAsset;
                Debug.Log("[EventSystemLinker] InputSystemUIInputModule�� PlayerInputActions�� ���������� �����߽��ϴ�.");
            }
            else
            {
                Debug.LogError("[EventSystemLinker] ServiceLocator���� InputManager�� ã�� �� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogError("[EventSystemLinker] InputSystemUIInputModule�� ã�� �� �����ϴ�!");
        }
    }
}