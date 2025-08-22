using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    private const string MANAGERS_PREFAB_PATH = "_Managers";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeManagers()
    {
        if (FindObjectOfType<GameManager>() == null)
        {
            var managersPrefab = Resources.Load<GameObject>(MANAGERS_PREFAB_PATH);
            if (managersPrefab != null)
            {
                var managerInstance = Instantiate(managersPrefab);
                DontDestroyOnLoad(managerInstance);
            }
            else
            {
                Debug.LogError($"[InputManager] '{MANAGERS_PREFAB_PATH}' 경로에 _Managers 프리팹이 없습니다! Assets/Resources/Managers 폴더를 확인하세요.");
            }
        }
    }

    public UnityEvent<Vector2> OnMove = new UnityEvent<Vector2>();
    private GameObject lastSelectedObject;

    void Awake()
    {
        ServiceLocator.Register<InputManager>(this);
        // DontDestroyOnLoad는 InitializeManagers에서 처리되므로 여기선 생략

        // 장면의 모든 GraphicRaycaster를 찾아 비활성화합니다.
        GraphicRaycaster[] allGraphicRaycasters = FindObjectsOfType<GraphicRaycaster>();
        foreach (GraphicRaycaster raycaster in allGraphicRaycasters)
        {
            raycaster.enabled = false;
        }

        // 장면의 모든 Physics2DRaycaster를 찾아 비활성화합니다.
        Physics2DRaycaster[] allPhysics2DRaycasters = FindObjectsOfType<Physics2DRaycaster>();
        foreach (Physics2DRaycaster raycaster in allPhysics2DRaycasters)
        {
            raycaster.enabled = false;
        }
    }

    void Update()
    {
        if (EventSystem.current == null) return;

        // 만약 현재 선택된 UI 오브젝트가 있다면, 그 오브젝트를 '마지막으로 선택된 오브젝트'로 기억합니다.
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        }
        // 만약 현재 선택된 UI 오브젝트가 없다면 (예: 배경 클릭),
        // 그리고 우리가 이전에 선택했던 오브젝트를 기억하고 있다면, 그 오브젝트를 다시 선택 상태로 만듭니다.
        else
        {
            if (lastSelectedObject != null && lastSelectedObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            }
        }

        // 위 로직을 거친 후에도 여전히 선택된 UI가 없다면, 게임플레이 입력을 처리합니다.
        // (메뉴 씬이 아닌 실제 게임 씬에서만 이 부분이 의미를 가집니다)
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            HandleGameplayInput();
        }
    }

    private void HandleGameplayInput()
    {

        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        OnMove.Invoke(move.normalized);
    }
}