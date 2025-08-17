using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RouteSelectionController : MonoBehaviour
{
    public static RouteSelectionController Instance { get; private set; }

    [Header("UI ������")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject pathPrefab;

    [Header("UI �θ� ������Ʈ")]
    [SerializeField] private Transform pathParent;
    [SerializeField] private Transform nodeParent;

    [Header("�� �ð�ȭ ����")]
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
        // ��ҿ��� ��Ȱ��ȭ ���·� ����
        gameObject.SetActive(false);
    }

    // �ܺ�(��: RewardManager)���� ���� ǥ���϶�� ȣ���� �Լ�
    public void Show()
    {
        gameObject.SetActive(true);

        // ������ �׷��� UI�� �ִٸ� ��� ����
        foreach (Transform child in nodeParent) Destroy(child.gameObject);
        foreach (Transform child in pathParent) Destroy(child.gameObject);

        // 1. �� ���赵 ���� ��û
        var nodeCounts = new int[] { 1, 3, 2, 3, 2, 3, 1, 2, 3, 1 };
        List<MapNode> mapData = mapGenerator.Generate(10, nodeCounts);

        var nodeUiMap = new Dictionary<MapNode, GameObject>();

        // 2. ���赵�� ������� ���(��ư) UI ����
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

        // 3. ������ ������� ���(��) UI ����
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
        Debug.Log($"���õ� ��� Ÿ��: {node.NodeType}, ��ġ: {node.Position}");
    }
}