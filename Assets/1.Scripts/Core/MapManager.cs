using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public bool IsMapInitialized { get; private set; }

    public List<MapNode> AllNodes { get; private set; }
    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }

    public MapNode CurrentNode { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 맵 데이터와 함께 맵의 크기 정보도 받아 초기화합니다.
    /// </summary>
    public void InitializeMap(List<MapNode> generatedNodes, int width, int height)
    {
        AllNodes = generatedNodes;
        MapWidth = width;
        MapHeight = height;

        CurrentNode = AllNodes.FirstOrDefault(node => node.Position.y == 0);
        IsMapInitialized = true;

        Debug.Log($"[MapManager] 맵 초기화 완료. IsMapInitialized = {IsMapInitialized}. 총 노드: {AllNodes.Count}");
    }

    /// <summary>
    /// 플레이어를 새 노드로 이동시키고, Gameplay 씬으로 전환을 요청합니다.
    /// </summary>
    public void MoveToNode(MapNode newNode)
    {
        Debug.Log($"[MapManager] MoveToNode 호출됨. 이동할 노드: {newNode.Position}, 타입: {newNode.NodeType}"); // [추가됨] 디버그 로그
        if (CurrentNode != null && !CurrentNode.NextNodes.Contains(newNode))
        {
            Debug.LogError($"{newNode.Position}은(는) 현재 위치({CurrentNode.Position})에서 이동할 수 없는 노드입니다!");
            return;
        }

        CurrentNode = newNode;
        Debug.Log($"[MapManager] 플레이어 위치 업데이트: {CurrentNode.Position}, 노드 타입: {CurrentNode.NodeType}");

        GameManager.Instance.ChangeState(GameManager.GameState.Gameplay);
    }

    /// <summary>
    /// 현재 플레이어가 이동할 수 있는 다음 노드들의 리스트를 반환합니다.
    /// </summary>
    public List<MapNode> GetReachableNodes()
    {
        if (CurrentNode == null)
        {
            return AllNodes.Where(node => node.Position.y == 0).ToList();
        }
        return CurrentNode.NextNodes;
    }
}
