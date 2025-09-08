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
        gameObject.SetActive(true);

        // TODO: 여기에 PlayerDataManager의 데이터를 이용해 UI를 표시하는 로직을 추가해야 합니다.

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
        gameObject.SetActive(false);
        onInventoryClosed?.Invoke();
    }
}
