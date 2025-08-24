using UnityEngine;

/// <summary>
/// UIManager에 패널을 자동으로 등록하는 헬퍼 스크립트입니다.
/// 이 스크립트를 각 UI 패널의 루트 GameObject에 추가하세요.
/// </summary>
public class UIPanel : MonoBehaviour
{
    // UIManager에서 패널을 식별할 이름. Inspector에서 설정합니다.
    public string panelName;

    void Awake()
    {
        if (string.IsNullOrEmpty(panelName))
        {
            Debug.LogWarning($"UIPanel 스크립트가 부착된 '{gameObject.name}' 오브젝트에 'panelName'이 설정되지 않았습니다. GameObject 이름을 사용합니다.");
            panelName = gameObject.name;
        }

        // --- [수정] ServiceLocator를 통해 UIManager 인스턴스를 가져옵니다. ---
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
        {
            uiManager.RegisterPanel(panelName, gameObject);
        }
    }

    void OnDestroy()
    {
        // --- [수정] ServiceLocator를 통해 UIManager 인스턴스를 가져옵니다. ---
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null && !string.IsNullOrEmpty(panelName))
        {
            uiManager.UnregisterPanel(panelName);
        }
    }
}