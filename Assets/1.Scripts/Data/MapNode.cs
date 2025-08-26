using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Monster, Elite, Shop, Rest, Boss, Event }

public class MapNode
{
    public NodeType NodeType;
    public Vector2Int Position; //  ڿ ġ (x, y)
    public List<MapNode> NextNodes = new List<MapNode>(); //  忡   ִ  
}