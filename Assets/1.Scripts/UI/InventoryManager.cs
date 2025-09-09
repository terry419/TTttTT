using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 인벤토리 UI의 표시, 숨김 및 콜백을 관리합니다.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("포커스 관리")]
    [Tooltip("인벤토리가 열릴 때 처음으로 포커스될 UI 요소")]
    [SerializeField] private GameObject firstSelectedItem;
    private Action onInventoryClosed;

    void Awake()
    {
        // 시작 시 인벤토리는 비활성화 상태여야 함
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 인벤토리를 표시하고, 닫힐 때 실행될 콜백을 등록합니다.
    /// </summary>
    /// <param name="onClosedCallback">인벤토리가 닫힐 때 실행될 동작</param>
    public void Show(Action onClosedCallback)
    {
        this.onInventoryClosed = onClosedCallback;

        // ▼▼▼ [추가] 콜백 등록 디버그 로그 ▼▼▼
        if (this.onInventoryClosed != null)
        {
            Debug.Log($"[InventoryManager] Show 호출됨. 닫기 콜백이 성공적으로 등록되었습니다.");
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] Show 호출됨. 하지만 닫기 콜백이 null입니다.");
        }

        gameObject.SetActive(true);

        if (firstSelectedItem != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedItem);
        }
    }

    /// <summary>
    /// 인벤토리를 닫고 등록된 콜백을 실행합니다.
    /// </summary>
    public void Close()
    {
        // ▼▼▼ [수정] 콜백 호출 디버그 로그 추가 ▼▼▼
        Debug.Log($"[InventoryManager] Close 호출됨. 등록된 콜백을 실행합니다.");
        gameObject.SetActive(false);

        if (onInventoryClosed != null)
        {
            Debug.Log($"[InventoryManager] onInventoryClosed.Invoke() 실행 직전.");
            onInventoryClosed?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] onInventoryClosed 콜백이 null이라 실행할 수 없습니다.");
        }
    }
}