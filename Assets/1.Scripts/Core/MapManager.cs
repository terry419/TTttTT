// 파일명: MapManager.cs (리팩토링 완료)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapManager : MonoBehaviour
{
    public bool IsMapInitialized { get; private set; }
    public List<MapNode> AllNodes { get; private set; }
    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }
    public MapNode CurrentNode { get; private set; }

    void Awake()
    {
        // 중복이 아님이 확인되었으므로, 이제 ServiceLocator에 자기 자신을 등록합니다.
        ServiceLocator.Register<MapManager>(this);
        DontDestroyOnLoad(this.gameObject);
    }
    
    /// <summary>
    /// 맵 데이터와 함께 맵의 크기 정보도 받아 초기화합니다.
    /// </summary>
    public void InitializeMap(List<MapNode> mapData, int width, int height)
    {
        AllNodes = mapData;
        MapWidth = width;
        MapHeight = height;
        CurrentNode = AllNodes.FirstOrDefault(n => n.Position.y == 0); // 시작 노드 설정
        IsMapInitialized = true;
        Debug.Log($"[MapManager] 맵 데이터 초기화 완료. (노드 수: {AllNodes.Count}, 너비: {width}, 높이: {height})");
    }

    /// <summary>
    /// 플레이어를 새 노드로 이동시키고, Gameplay 씬으로 전환을 요청합니다.
    /// </summary>
    public void MoveToNode(MapNode node)
    {
        if (AllNodes.Contains(node))
        {
            CurrentNode = node;
            Debug.Log($"[MapManager] 현재 위치를 노드 {node.Position}로 이동했습니다.");
        }
    }

    /// <summary>
    /// [추가된 함수] 씬을 전환하지 않고, 현재 노드 데이터만 업데이트합니다.
    /// </summary>
    public void MoveToNode_OnlyUpdateData(MapNode newNode)
    {
        if (CurrentNode != null && !CurrentNode.NextNodes.Contains(newNode))
        {
            Debug.LogError($"{newNode.Position}은(는) 현재 위치({CurrentNode.Position})에서 이동할 수 없는 노드입니다!");
            return;
        }
        CurrentNode = newNode;
        Debug.Log($"[MapManager] 현재 노드가 {newNode.Position}(으)로 업데이트되었습니다.");
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