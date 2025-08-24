// 파일명: UIManager.cs (리팩토링 완료)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 게임 내 UI 패널들을 관리하는 클래스입니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();

    void Awake()
    {
        ServiceLocator.Register<UIManager>(this);
        DontDestroyOnLoad(gameObject);
    }

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

    public void ShowPanel(string panelName)
    {
        ShowPanel(panelName, true);
    }

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
}