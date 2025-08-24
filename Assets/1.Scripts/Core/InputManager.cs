using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Added for scene management events
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations; // 코루틴 사용을 위해 추가

public class InputManager : MonoBehaviour
{
    // [삭제] 이 const 변수들은 더 이상 사용하지 않습니다.
    // private const string MANAGERS_PREFAB_PATH = PrefabKeys.Managers;
    // private const string SESSION_PREFAB_PATH = PrefabKeys.GameplaySession;

    

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 기존 로직을 직접 실행하는 대신 코루틴을 시작시킵니다.
        StartCoroutine(OnSceneLoadedRoutine(scene, mode));
    }

    private IEnumerator OnSceneLoadedRoutine(Scene scene, LoadSceneMode mode)
    {
        // 한 프레임 대기하여 씬의 모든 오브젝트가 Awake() 및 OnEnable()을 마칠 시간을 줍니다.
        yield return null;

        Debug.Log($"[InputManager] 씬 로드됨: {scene.name}, 모드: {mode}");
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[InputManager] 씬 로드 후 EventSystem.current가 null입니다.");
        }
        else
        {
            Debug.Log($"[InputManager] EventSystem.current: {EventSystem.current.gameObject.name}");
            if (EventSystem.current.currentInputModule != null)
            {
                Debug.Log($"[InputManager] 현재 입력 모듈: {EventSystem.current.currentInputModule.GetType().Name}, 활성화됨: {EventSystem.current.currentInputModule.enabled}");
            }
            else
            {
                // 이 코루틴 수정으로 인해 이 경고는 더 이상 나타나지 않을 것입니다.
                Debug.LogWarning("[InputManager] EventSystem.current.currentInputModule이 null입니다.");
            }
        }

        // 기존의 Raycaster 비활성화 로직은 그대로 유지합니다.
        GraphicRaycaster[] allGraphicRaycasters = FindObjectsOfType<GraphicRaycaster>();
        Debug.Log($"[InputManager] 씬 로드 후 {allGraphicRaycasters.Length}개의 GraphicRaycaster 발견.");
        foreach (GraphicRaycaster raycaster in allGraphicRaycasters)
        {
            if (raycaster.enabled)
            {
                Debug.LogWarning($"[InputManager] GraphicRaycaster '{raycaster.gameObject.name}'가 씬 로드 후 활성화되어 있습니다.");
                raycaster.enabled = false;
            }
        }

        Physics2DRaycaster[] allPhysics2DRaycasters = FindObjectsOfType<Physics2DRaycaster>();
        Debug.Log($"[InputManager] 씬 로드 후 {allPhysics2DRaycasters.Length}개의 Physics2DRaycaster 발견.");
        foreach (Physics2DRaycaster raycaster in allPhysics2DRaycasters)
        {
            if (raycaster.enabled)
            {
                Debug.LogWarning($"[InputManager] Physics2DRaycaster '{raycaster.gameObject.name}'가 씬 로드 후 활성화되어 있습니다.");
                raycaster.enabled = false;
            }
        }
    }

    void Update()
    {
        if (EventSystem.current == null) return;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

        // 1. 현재 무언가 선택되어 있고, 그게 Selectable(버튼, 토글 등)이라면 마지막 선택으로 기억합니다.
        if (currentSelected != null && currentSelected.GetComponent<Selectable>() != null)
        {
            lastSelectedObject = currentSelected;
        }
        // 2. 현재 선택된 것이 없거나, Selectable이 아닌 것(예: 배경 패널)이 선택되었다면
        else
        {
            // 3. 마지막으로 기억해 둔 Selectable이 있다면 강제로 포커스를 되돌립니다.
            if (lastSelectedObject != null && lastSelectedObject.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedObject);
            }
        }

        // 게임플레이 중에는 키보드/게임패드 입력을 처리합니다.
        HandleGameplayInput();
    }

    private void HandleGameplayInput()
    {
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        OnMove.Invoke(move.normalized);
    }
}