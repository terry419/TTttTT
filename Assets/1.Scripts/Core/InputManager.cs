using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        if (Instance != null && Instance != this)
        {
            // 이미 InputManager가 존재하면, 이 오브젝트는 파괴함
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad는 InitializeManagers에서 이미 처리하므로 여기서 할 필요 없음

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

        // 마우스 커서를 숨기고 화면 중앙에 고정합니다.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Update()
    {
        if (EventSystem.current == null) return;

        // [수정] 포커스가 사라졌을 때, 유저가 다시 키를 입력하는 경우에만 마지막 오브젝트를 선택하도록 변경
        // 이렇게 하면 다른 스크립트가 포커스를 설정하는 과정과 충돌하지 않습니다.
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 || Input.GetButtonDown("Submit"))
            {
                if (lastSelectedObject != null && lastSelectedObject.activeInHierarchy)
                {
                    EventSystem.current.SetSelectedGameObject(lastSelectedObject);
                }
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