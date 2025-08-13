// --- 파일명: UICursorManager.cs (새로 생성) ---
using UnityEngine;
using UnityEngine.EventSystems; // EventSystem 사용을 위해 필수

/// <summary>
/// EventSystem이 현재 선택한 UI 요소를 추적하여
/// 커서 스프라이트의 위치를 실시간으로 업데이트하는 중앙 관리자입니다.
/// 이 스크립트는 DontDestroyOnLoad로 설정되어 모든 씬에서 동작합니다.
/// </summary>
public class UICursorManager : MonoBehaviour
{
    public static UICursorManager Instance { get; private set; }

    // 현재 씬에서 제어할 커서 스프라이트의 RectTransform
    private RectTransform activeCursor;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // 제어할 커서가 등록되어 있지 않으면 아무것도 하지 않음
        if (activeCursor == null)
        {
            return;
        }

        // EventSystem이 현재 선택하고 있는 게임오브젝트를 가져옵니다.
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        // 선택된 오브젝트가 있다면
        if (selectedObject != null)
        {
            // 커서 스프라이트를 활성화하고, 선택된 오브젝트의 위치로 이동시킵니다.
            activeCursor.gameObject.SetActive(true);
            activeCursor.position = selectedObject.transform.position;
        }
        else
        {
            // 선택된 오브젝트가 아무것도 없다면 커서를 숨깁니다.
            activeCursor.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 새로운 씬이 로드될 때, 해당 씬의 UI 매니저가 이 함수를 호출하여
    /// 제어할 커서 스프라이트를 등록합니다.
    /// </summary>
    /// <param name="cursorRectTransform">새로운 씬의 커서 RectTransform</param>
    public void RegisterCursor(RectTransform cursorRectTransform)
    {
        this.activeCursor = cursorRectTransform;
        if (this.activeCursor != null)
        {
            Debug.Log($"[UICursorManager] 새로운 커서 등록: {activeCursor.name}");
        }
    }

    /// <summary>
    /// 씬을 떠날 때 커서 등록을 해제합니다.
    /// </summary>
    public void UnregisterCursor()
    {
        this.activeCursor = null;
    }
}