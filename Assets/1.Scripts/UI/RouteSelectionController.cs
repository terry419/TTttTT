using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RouteSelectionController : MonoBehaviour
{
    public static RouteSelectionController Instance { get; private set; }

    [Header("UI 프리팹")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject pathPrefab;

    [Header("UI 부모 오브젝트")]
    [SerializeField] private Transform pathParent;
    [SerializeField] private Transform nodeParent;

    [Header("맵 시각화 설정")]
    [SerializeField] private float nodeSpacingX = 200f;
    [SerializeField] private float nodeSpacingY = 120f;

    private MapGenerator mapGenerator = new MapGenerator();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 평소에는 비활성화 상태로 시작
        gameObject.SetActive(false);
    }

    // 외부(예: RewardManager)에서 맵을 표시하라고 호출할 함수
    public void Show()
    {
        gameObject.SetActive(true);

        // 기존에 그려진 UI가 있다면 모두 삭제
        foreach (Transform child in nodeParent) Destroy(child.gameObject);
        foreach (Transform child in pathParent) Destroy(child.gameObject);

        // 1. 맵 설계도 생성 요청
        var nodeCounts = new int[] { 1, 3, 2, 3, 2, 3, 1, 2, 3, 1 };
        List<MapNode> mapData = mapGenerator.Generate(10, nodeCounts);

        var nodeUiMap = new Dictionary<MapNode, GameObject>();

        // 2. 설계도를 기반으로 노드(버튼) UI 생성
        foreach (var nodeData in mapData)
        {
            GameObject nodeObj = Instantiate(nodePrefab, nodeParent);
            float xPos = nodeData.Position.x * nodeSpacingX;
            float yPos = nodeData.Position.y * nodeSpacingY;
            nodeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);

            MapNodeUI nodeUI = nodeObj.GetComponent<MapNodeUI>();
            nodeUI.nodeData = nodeData;
            nodeUI.button.onClick.AddListener(() => OnNodeSelected(nodeData));

            nodeUiMap.Add(nodeData, nodeObj);
        }

        // 3. 노드들을 기반으로 경로(선) UI 생성
        foreach (var nodeData in mapData)
        {
            GameObject fromObj = nodeUiMap[nodeData];
            foreach (var childNode in nodeData.NextNodes)
            {
                GameObject toObj = nodeUiMap[childNode];
                DrawPath(fromObj, toObj);
            }
        }
    }

    private void DrawPath(GameObject from, GameObject to)
    {
        GameObject pathObj = Instantiate(pathPrefab, pathParent);
        RectTransform pathRect = pathObj.GetComponent<RectTransform>();

        Vector2 dir = (to.transform.position - from.transform.position).normalized;
        float distance = Vector2.Distance(to.transform.position, from.transform.position);

        pathRect.sizeDelta = new Vector2(distance, pathRect.sizeDelta.y);
        pathRect.position = from.transform.position;
        pathRect.pivot = new Vector2(0, 0.5f);
        pathRect.rotation = Quaternion.FromToRotation(Vector3.right, dir);
    }

    private void OnNodeSelected(MapNode node)
    {
        Debug.Log($"선택된 노드 타입: {node.NodeType}, 위치: {node.Position}");
    }
}