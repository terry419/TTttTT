using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MapView : MonoBehaviour
{
    [Header("UI 프리팹")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject pathPrefab;

    [Header("UI 부모 오브젝트")]
    [SerializeField] private Transform pathParent;
    [SerializeField] private Transform nodeParent;

    [Header("맵 시각화 설정")]
    [SerializeField] private float nodeSpacingX = 200f;
    [SerializeField] private float nodeSpacingY = 120f;
    [SerializeField] private ScrollRect mapScrollRect;

    private Dictionary<MapNode, GameObject> nodeUiMap = new Dictionary<MapNode, GameObject>();

    public event System.Action<MapNode> OnNodeSelected;

    public void GenerateMapView(List<MapNode> mapData, int mapWidth, int intMapHeight)
    {
        Debug.Log("[MapView] GenerateMapView 시작."); // [추가됨] 디버그 로그
        foreach (Transform child in nodeParent) Destroy(child.gameObject); // [수정됨] intMapHeight로 변수명 변경
        foreach (Transform child in pathParent) Destroy(child.gameObject);
        nodeUiMap.Clear();

        float horizontalOffset = (mapWidth - 1) * nodeSpacingX * 0.5f;

        foreach (var nodeData in mapData)
        {
            GameObject nodeObj = Instantiate(nodePrefab, nodeParent);
            RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();
            if (nodeRect != null)
            {
                float posX = nodeData.Position.x * nodeSpacingX - horizontalOffset;
                float posY = nodeData.Position.y * nodeSpacingY;
                nodeRect.anchoredPosition = new Vector2(posX, posY);
            }

            MapNodeUI mapNodeUI = nodeObj.GetComponent<MapNodeUI>();
            if (mapNodeUI != null)
            {
                // [추가됨] MapNodeUI의 button 참조 확인
                if (mapNodeUI.button == null)
                {
                    Debug.LogError($"[MapView] MapNodeUI ({nodeObj.name})에 Button 컴포넌트가 연결되지 않았습니다!");
                }
                else
                {
                    mapNodeUI.nodeData = nodeData;
                    MapNode currentNodeData = nodeData;
                    mapNodeUI.button.onClick.AddListener(() => OnNodeClicked(currentNodeData));
                }
            }
            else
            {
                Debug.LogError($"[MapView] 노드 프리팹 ({nodePrefab.name})에 MapNodeUI 컴포넌트가 없습니다!"); // [추가됨] 디버그 로그
            }

            nodeUiMap.Add(nodeData, nodeObj);
        }

        foreach (var nodeData in mapData)
        {
            if (nodeData.NextNodes.Count > 0)
            {
                GameObject fromObj = nodeUiMap[nodeData];
                foreach (var childNode in nodeData.NextNodes)
                {
                    if (nodeUiMap.ContainsKey(childNode))
                    {
                        DrawPath(fromObj, nodeUiMap[childNode]);
                    }
                }
            }
        }

        RectTransform contentRect = mapScrollRect.content;
        float totalMapHeight = (intMapHeight - 1) * nodeSpacingY; // [수정됨] intMapHeight로 변수명 변경
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalMapHeight + 8000f);
        Debug.Log("[MapView] GenerateMapView 완료."); // [추가됨] 디버그 로그
    }

    private void OnNodeClicked(MapNode node)
    {
        OnNodeSelected?.Invoke(node);
        Debug.Log($"[MapView] OnNodeSelected 이벤트 발생 시도.");
    }

    private void DrawPath(GameObject from, GameObject to)
    {
        GameObject pathObj = Instantiate(pathPrefab, pathParent);
        RectTransform pathRect = pathObj.GetComponent<RectTransform>();
        Image pathImage = pathObj.GetComponent<Image>();

        Vector2 dir = (to.transform.position - from.transform.position).normalized;
        float distance = Vector2.Distance(to.transform.position, from.transform.position);

        pathRect.sizeDelta = new Vector2(distance, pathRect.sizeDelta.y);
        pathRect.position = from.transform.position;
        pathRect.pivot = new Vector2(0, 0.5f);
        pathRect.rotation = Quaternion.FromToRotation(Vector3.right, dir);
        if (pathImage != null) pathImage.type = Image.Type.Tiled;
    }

    /// <summary>
    /// [변경됨] 모든 노드의 네비게이션을 요구사항에 맞게 설정합니다.
    /// </summary>
    public void SetupAllNodeNavigations(Button backButton, Button inventoryButton)
    {
        foreach (var pair in nodeUiMap)
        {
            MapNode currentNodeData = pair.Key;
            GameObject currentNodeObj = pair.Value;
            Button currentBtn = currentNodeObj.GetComponent<Button>();

            if (currentBtn == null)
            {
                Debug.LogError($"[MapView] 노드 오브젝트 {currentNodeObj.name}에 Button 컴포넌트가 없습니다!");
                continue;
            }

            Navigation nav = new Navigation
            {
                mode = Navigation.Mode.Explicit
            };

            // Up Navigation
            var upCandidates = nodeUiMap.Where(p => p.Key.Position.y == currentNodeData.Position.y + 1);
            if (upCandidates.Any())
            {
                var targetUp = upCandidates.OrderBy(p => Mathf.Abs(p.Value.transform.position.x - currentNodeObj.transform.position.x)).First();
                nav.selectOnUp = targetUp.Value.GetComponent<Button>();
            }

            // Down Navigation
            var downCandidates = nodeUiMap.Where(p => p.Key.Position.y == currentNodeData.Position.y - 1);
            if (downCandidates.Any())
            {
                var targetDown = downCandidates.OrderBy(p => Mathf.Abs(p.Value.transform.position.x - currentNodeObj.transform.position.x)).First();
                nav.selectOnDown = targetDown.Value.GetComponent<Button>();
            }

            // Left Navigation
            var leftCandidates = nodeUiMap.Where(p => p.Key.Position.y == currentNodeData.Position.y && p.Key.Position.x < currentNodeData.Position.x);
            if (leftCandidates.Any())
            {
                var targetLeft = leftCandidates.OrderByDescending(p => p.Key.Position.x).First();
                nav.selectOnLeft = targetLeft.Value.GetComponent<Button>();
            }
            else
            {
                if (backButton != null && backButton.gameObject.activeInHierarchy)
                {
                    nav.selectOnLeft = backButton;
                }
            }

            // Right Navigation (수정된 로직)
            var rightCandidates = nodeUiMap.Where(p => p.Key.Position.y == currentNodeData.Position.y && p.Key.Position.x > currentNodeData.Position.x);

            if (rightCandidates.Any())
            {
                // 오른쪽에 다른 노드가 있는 경우
                var targetRight = rightCandidates.OrderBy(p => p.Key.Position.x).First();
                nav.selectOnRight = targetRight.Value.GetComponent<Button>();
                Debug.Log($"[네비게이션 설정] 노드 {currentNodeData.Position}의 오른쪽 타겟: 노드 {targetRight.Key.Position}");
            }
            else
            {
                // 오른쪽에 더 이상 노드가 없는 경우 (가장 오른쪽 노드)
                Debug.Log($"[네비게이션 설정] 노드 {currentNodeData.Position}는 가장 오른쪽 노드입니다. InventoryButton으로 연결을 시도합니다.");
                if (inventoryButton != null && inventoryButton.gameObject.activeInHierarchy)
                {
                    nav.selectOnRight = inventoryButton;
                    Debug.Log($"<color=lime>[네비게이션 설정] 성공: 노드 {currentNodeData.Position}의 오른쪽 타겟을 InventoryButton으로 설정했습니다.</color>");
                }
                else
                {
                    nav.selectOnRight = null; // 연결할 버튼이 없으면 null로 설정
                    Debug.LogWarning($"[네비게이션 설정] InventoryButton이 비활성화 상태이거나 null이어서 노드 {currentNodeData.Position}의 오른쪽 네비게이션을 설정하지 못했습니다.");
                }
            }

            currentBtn.navigation = nav;
        }
    }

    public void UpdateNodeInteractability(List<MapNode> reachableNodes)
    {
        Debug.Log($"[MapView] UpdateNodeInteractability 시작. 도달 가능한 노드 수: {reachableNodes.Count}"); // [추가됨] 디버그 로그
        foreach (var entry in nodeUiMap)
        {
            Button button = entry.Value.GetComponentInChildren<Button>();
            if (button != null)
            {
                bool isInteractable = reachableNodes.Contains(entry.Key);
                button.interactable = isInteractable;
            }
            else
            {
                Debug.LogWarning($"[MapView] 노드 {entry.Key.Position}의 자식에서 Button 컴포넌트를 찾을 수 없습니다."); // [추가됨] 디버그 로그
            }
        }
        Debug.Log("[MapView] UpdateNodeInteractability 완료."); // [추가됨] 디버그 로그
    }

    public GameObject FindClosestNodeTo(Vector3 position)
    {
        if (nodeUiMap.Count == 0) return null;
        return nodeUiMap.OrderBy(p => Vector3.Distance(p.Value.transform.position, position)).First().Value;
    }

    public GameObject FindLeftmostAvailableNode(List<MapNode> reachableNodes)
    {
        if (reachableNodes == null || reachableNodes.Count == 0) return null;

        MapNode leftmostNode = reachableNodes
            .OrderBy(n => n.Position.y)
            .ThenBy(n => n.Position.x)
            .FirstOrDefault();

        if (leftmostNode != null && nodeUiMap.ContainsKey(leftmostNode))
        {
            return nodeUiMap[leftmostNode];
        }
        return null;
    }
    public GameObject GetNodeUIObject(Vector2Int position)
    {
        Debug.Log($"[네비게이션 디버그] MapView: GetNodeUIObject 호출됨. 찾는 좌표: {position}");
        // 딕셔너리에서 해당 좌표를 가진 MapNode 데이터를 찾습니다.
        MapNode targetNodeData = nodeUiMap.Keys.FirstOrDefault(node => node.Position == position);
        if (targetNodeData != null && nodeUiMap.TryGetValue(targetNodeData, out GameObject nodeObject))
        {
            Debug.Log($"<color=lime>[네비게이션 디버그] MapView: {position} 좌표의 노드 UI '{nodeObject.name}'를 찾았습니다.</color>");
            return nodeObject;
        }
        // 해당 노드를 찾지 못하면 null을 반환합니다.
        Debug.LogWarning($"[네비게이션 디버그] MapView: {position} 좌표에 해당하는 노드를 찾지 못했습니다!");
        return null;
    }
}