// --- ���ϸ�: MapGenerator.cs (���: Assets/1.Scripts/Core/MapGenerator.cs) ---
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

        // 1. �� �࿡ ��� ��ġ�ϱ�
        for (int y = 0; y < mapHeight; y++)
        {
            var rowNodes = new List<MapNode>();
            int nodeCount = nodeCountsByRow[y];

            // x��ǥ�� - (nodeCount/2) ... + (nodeCount/2) ������� �߾� ����
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

        // 2. ������ �Ʒ����� ���� �����ϱ�
        for (int y = 0; y < mapHeight - 1; y++)
        {
            foreach (var parent in nodesByRow[y])
            {
                // ���� ������ �ڽ� �ĺ�: �ٷ� �� ���� x��ǥ�� ����� ����
                var potentialChildren = nodesByRow[y + 1]
                    .Where(child => Mathf.Abs(child.Position.x - parent.Position.x) <= 1)
                    .ToList();

                if (potentialChildren.Any())
                {
                    // �ĺ� �� �ϳ��� �����ϰ� �����Ͽ� ����
                    var child = potentialChildren[Random.Range(0, potentialChildren.Count)];
                    parent.NextNodes.Add(child);
                }
            }
        }

        // 3. (������ġ) ������ �ϳ��� ���� �ڽ� ��尡 �ִٸ�, ���� ����� �θ�� ���� ����
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