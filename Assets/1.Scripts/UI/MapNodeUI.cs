using UnityEngine;
using UnityEngine.UI;

// 이 스크립트는 UI 버튼 오브젝트가 어떤 맵 노드 데이터(위치)를
// 가지고 있는지 관리하는 스크립트입니다.
public class MapNodeUI : MonoBehaviour
{
    public MapNode nodeData; // 이 노드 UI가 나타내는 맵 노드 데이터
    
    // [수정됨] Inspector에서 Button 컴포넌트를 연결할 수 있도록 [HideInInspector]를 제거합니다.
    [SerializeField] public Button button; 

    void Awake()
    {
        // [수정됨] 자식 오브젝트에서도 Button 컴포넌트를 찾을 수 있도록 GetComponentInChildren를 사용합니다.
        if (button == null) // Inspector에서 연결되지 않았다면 코드로 찾습니다.
        {
            button = GetComponentInChildren<Button>();
            if (button == null)
            {
                Debug.LogError($"[MapNodeUI] {gameObject.name}에서 Button 컴포넌트를 찾을 수 없습니다! Inspector에서 연결하거나 자식 오브젝트에 Button이 있는지 확인하세요.");
            }
        }
    }
}