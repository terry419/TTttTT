using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 게임 내 UI 패널들을 관리하는 클래스입니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // UI 패널들을 저장할 딕셔너리 (패널 이름, GameObject)
    private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // TODO: 씬에 있는 모든 UI 패널들을 찾아서 딕셔너리에 등록하는 로직 추가
        // 예: FindObjectsOfType<UIPanelBase>()를 사용하여 각 패널의 이름을 키로 등록
    }

    /// <summary>
    /// 특정 UI 패널을 활성화/비활성화합니다.
    /// </summary>
    /// <param name="panelName">활성화/비활성화할 패널의 이름</param>
    /// <param name="isActive">활성화 여부 (true: 활성화, false: 비활성화)</param>
    public void ShowPanel(string panelName, bool isActive)
    {
        if (uiPanels.TryGetValue(panelName, out GameObject panel))
        {
            panel.SetActive(isActive);
            Debug.Log($"UI Panel '{panelName}' set to active: {isActive}");
        }
        else
        {
            Debug.LogWarning($"UI Panel '{panelName}' not found in UIManager.");
        }
    }

    /// <summary>
    /// 특정 UI 패널을 활성화합니다. (ShowPanel(panelName, true)와 동일)
    /// </summary>
    /// <param name="panelName">활성화할 패널의 이름</param>
    public void ShowPanel(string panelName)
    {
        ShowPanel(panelName, true);
    }

    /// <summary>
    /// 모든 UI 패널을 비활성화합니다.
    /// </summary>
    public void HideAllPanels()
    {
        foreach (var panel in uiPanels.Values)
        {
            panel.SetActive(false);
        }
        Debug.Log("All UI panels hidden.");
    }
    public void RegisterPanel(string panelName, GameObject panelGameObject)
    {
        if (!uiPanels.ContainsKey(panelName))
        {
            uiPanels.Add(panelName, panelGameObject);
            Debug.Log($"UI Panel '{panelName}' registered with UIManager.");
        }
        else
        {
            Debug.LogWarning($"UI Panel '{panelName}' is already registered.");
        }
     }
     /// <summary>
     /// UI 패널을 UIManager에서 해제합니다.
     /// </summary>
     /// <param name="panelName">해제할 패널의 이름</param>
     public void UnregisterPanel(string panelName)
     {
         if (uiPanels.Remove(panelName))
         {
             Debug.Log($"UI Panel '{panelName}' unregistered from UIManager.");
         }
         else
         {
             Debug.LogWarning($"UI Panel '{panelName}' not found for unregistration.");
         }
     }
    // TODO: UI 패널 등록/해제 메서드 추가 (예: 씬 로드 시 패널 등록, 씬 언로드 시 해제)
    // public void RegisterPanel(string panelName, GameObject panel) { ... }
    // public void UnregisterPanel(string panelName) { ... }
}
