using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Monster, Elite, Shop, Rest, Boss, Event }

public class MapNode
{
    public NodeType NodeType;
    public Vector2Int Position; // 맵 격자에서의 위치 (x, y)
    public List<MapNode> NextNodes = new List<MapNode>(); // 이 노드에서 갈 수 있는 다음 노드들
}