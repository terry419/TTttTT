// --- 파일명: MapGenerator.cs (경로: Assets/1.Scripts/Core/MapGenerator.cs) ---
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator
{
    private List<List<MapNode>> nodesByRow = new List<List<MapNode>>();

    public List<MapNode> Generate(int mapHeight, int[] nodeCountsByRow)
    {
        nodesByRow.Clear();
        var allNodes = new List<MapNode>();

        // 1. 각 행에 노드 배치하기
        for (int y = 0; y < mapHeight; y++)
        {
            var rowNodes = new List<MapNode>();
            int nodeCount = nodeCountsByRow[y];

            // x좌표는 - (nodeCount/2) ... + (nodeCount/2) 방식으로 중앙 정렬
            for (int i = 0; i < nodeCount; i++)
            {
                int x = i - (nodeCount - 1) / 2;
                var newNode = new MapNode
                {
                    Position = new Vector2Int(x, y),
                    NodeType = (y == mapHeight - 1) ? NodeType.Boss : NodeType.Monster,
                };
                rowNodes.Add(newNode);
                allNodes.Add(newNode);
            }
            nodesByRow.Add(rowNodes);
        }

        // 2. 노드들을 아래에서 위로 연결하기
        for (int y = 0; y < mapHeight - 1; y++)
        {
            foreach (var parent in nodesByRow[y])
            {
                // 연결 가능한 자식 후보: 바로 위 행의 x좌표가 비슷한 노드들
                var potentialChildren = nodesByRow[y + 1]
                    .Where(child => Mathf.Abs(child.Position.x - parent.Position.x) <= 1)
                    .ToList();

                if (potentialChildren.Any())
                {
                    // 후보 중 하나를 랜덤하게 선택하여 연결
                    var child = potentialChildren[Random.Range(0, potentialChildren.Count)];
                    parent.NextNodes.Add(child);
                }
            }
        }

        // 3. (안전장치) 연결이 하나도 없는 자식 노드가 있다면, 가장 가까운 부모와 강제 연결
        for (int y = 1; y < mapHeight; y++)
        {
            foreach (var child in nodesByRow[y])
            {
                bool isConnected = nodesByRow[y - 1].Any(p => p.NextNodes.Contains(child));
                if (!isConnected)
                {
                    var closestParent = nodesByRow[y - 1]
                        .OrderBy(p => Mathf.Abs(p.Position.x - child.Position.x))
                        .First();
                    closestParent.NextNodes.Add(child);
                }
            }
        }
        return allNodes;
    }
}