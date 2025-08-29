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
                // 가져온 인스턴스의 InputActions 속성을 할당합니다.
                uiInputModule.actionsAsset = inputManager.ActionsAsset;
                Debug.Log("[EventSystemLinker] InputSystemUIInputModule에 PlayerInputActions를 성공적으로 연결했습니다.");
            }
            else
            {
                Debug.LogError("[EventSystemLinker] ServiceLocator에서 InputManager를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError("[EventSystemLinker] InputSystemUIInputModule을 찾을 수 없습니다!");
        }
    }
}