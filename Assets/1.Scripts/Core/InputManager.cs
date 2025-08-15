using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private const string MANAGERS_PREFAB_PATH = "_Managers";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeManagers()
    {
        if (GameManager.Instance == null)
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
        Instance = this;
    }

    void Update()
    {
        if (EventSystem.current == null) return;

        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (lastSelectedObject != null)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            }
        }
        else
        {
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        }

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