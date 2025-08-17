using UnityEngine;
using UnityEngine.UI;

// 이 스크립트는 UI 버튼 오브젝트가 어떤 맵 데이터(정보)를
// 가지고 있는지 저장하고 연결하는 역할을 합니다.
public class MapNodeUI : MonoBehaviour
{
    public MapNode nodeData; // 맵 데이터 정보 (3단계에서 사용)
    [HideInInspector] public Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }
}