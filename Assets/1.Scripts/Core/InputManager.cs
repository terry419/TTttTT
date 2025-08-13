// --- 파일명: InputManager.cs (2025-08-14 최종 통합 완성본) ---
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// 게임의 모든 입력을 처리하고, 마우스 클릭으로 인한 UI 선택 해제를 방지하며,
/// 핵심 매니저(_Managers 프리팹)가 존재하지 않을 경우 자동으로 생성하는 핵심 클래스입니다.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // --- 매니저 자동 생성 ---
    private const string MANAGERS_PREFAB_PATH = "Managers/_Managers";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeManagers()
    {
        if (Instance == null)
        {
            var managersPrefab = Resources.Load<GameObject>(MANAGERS_PREFAB_PATH);
            if (managersPrefab != null)
            {
                var managerInstance = Instantiate(managersPrefab);
                // 중요: 프리팹의 루트 오브젝트(_Managers)에 DontDestroyOnLoad가 설정되어 있어야 합니다.
                DontDestroyOnLoad(managerInstance);
            }
            else
            {
                Debug.LogError($"[InputManager] '{MANAGERS_PREFAB_PATH}' 경로에 _Managers 프리팹이 없습니다! Assets/Resources/Managers 폴더를 확인하세요.");
            }
        }
    }
    // --- 매니저 자동 생성 끝 ---

    // 게임 플레이용 이벤트 (PlayerController가 이 이벤트를 구독합니다)
    public UnityEvent<Vector2> OnMove = new UnityEvent<Vector2>();

    private GameObject lastSelectedObject; // 마우스 클릭 방지를 위해 마지막으로 선택된 UI 오브젝트

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 이미 인스턴스가 있고, 이 오브젝트가 그 인스턴스가 아니라면
            // 중복 생성을 방지하기 위해 이 오브젝트를 파괴합니다.
            Destroy(gameObject);
            return;
        }
        // 이 스크립트가 붙은 오브젝트가 최초의 인스턴스일 경우 Instance로 등록
        Instance = this;
    }

    void Update()
    {
        // --- 마우스 클릭으로 인한 선택 해제 방지 로직 (항상 실행) ---
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

        // --- UI 모드 / 게임 플레이 모드 입력 분기 ---
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            // UI가 선택되지 않았을 때 -> 게임 플레이 입력 처리
            HandleGameplayInput();
        }
        // UI가 선택된 상태일 때는 EventSystem이 입력을 모두 처리하므로,
        // InputManager에서는 추가적인 키보드 입력을 처리하지 않습니다.
    }

    /// <summary>
    /// 순수 게임 플레이 상태일 때의 입력을 처리합니다.
    /// </summary>
    private void HandleGameplayInput()
    {
        // 이동 키(WASD, 방향키) 입력을 받아 Vector2로 변환
        Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // OnMove 이벤트를 발생시켜, 이 이벤트를 구독하는 모든 대상(PlayerController)에게 이동 값을 전달
        OnMove.Invoke(move.normalized);
    }
}