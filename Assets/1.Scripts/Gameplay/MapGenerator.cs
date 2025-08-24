using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 절차적 맵 생성을 담당하는 클래스입니다.
/// MonoBehaviour를 상속하여 Inspector에서 맵 생성 규칙을 쉽게 조정할 수 있습니다.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [Header("맵 크기 설정")]
    [SerializeField] private int mapHeight = 15; // y: 0 ~ 14
    [SerializeField] private int mapWidth = 5;   // x: 0 ~ 4

    // [추가됨] 다른 스크립트에서 맵 크기를 읽을 수 있도록 public getter를 추가합니다.
    public int MapHeight => mapHeight;
    public int MapWidth => mapWidth;

    [Header("노드 타입 생성 확률 (0~100%)")]
    [Range(0, 100)] [SerializeField] private float monsterChance = 60f;
    [Range(0, 100)] [SerializeField] private float restChance = 15f;
    [Range(0, 100)] [SerializeField] private float eventChance = 15f;
    [Range(0, 100)] [SerializeField] private float merchantChance = 10f; // 상점

    [Header("맵 경로 설정")]
    [SerializeField] private int numberOfPathsToGenerate = 4; // 생성할 경로의 개수

    [Header("디버그 옵션")]
    [SerializeField] private bool forceAllMonsters = true; // 이 옵션이 켜져 있으면 모든 노드를 몬스터로 강제합니다.

    /// <summary>
    /// 설정된 규칙에 따라 새로운 맵 데이터를 생성하고 노드 리스트를 반환합니다.
    /// </summary>
    /// <returns>생성된 모든 MapNode 객체의 리스트</returns>
    public List<MapNode> Generate()
    {

        // 맵 그리드 초기화 (모든 노드를 일단 생성)
        MapNode[,] grid = new MapNode[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                grid[x, y] = new MapNode { Position = new Vector2Int(x, y) };
            }
        }

        // 경로에 포함될 노드를 추적하기 위한 HashSet
        HashSet<MapNode> nodesInPaths = new HashSet<MapNode>();

        // 1. 고정된 시작/끝 노드 처리
        MapNode startNode = grid[mapWidth / 2, 0]; // y=0, x=2
        MapNode endNode = grid[mapWidth / 2, mapHeight - 1]; // y=14, x=2
        nodesInPaths.Add(startNode);
        nodesInPaths.Add(endNode);

        // 2. 지정된 개수만큼 경로 생성
        for (int i = 0; i < numberOfPathsToGenerate; i++)
        {
            GenerateSinglePath(grid, nodesInPaths, startNode, endNode);
        }

        // 3. 경로에 포함되지 않은 노드 삭제 및 최종 노드 리스트 생성
        List<MapNode> finalNodes = nodesInPaths.ToList();

        // 4. 노드 타입 할당 및 연결
        AssignNodeTypesAndConnectNodes(finalNodes, grid);

        return finalNodes;
    }

    /// <summary>
    /// 단일 경로를 생성하고, 경로에 포함된 노드들을 nodesInPaths에 추가합니다.
    /// </summary>
    private void GenerateSinglePath(MapNode[,] grid, HashSet<MapNode> nodesInPaths, MapNode startNode, MapNode endNode)
    {
        MapNode currentNode = startNode;
        nodesInPaths.Add(currentNode);

        for (int y = 0; y < mapHeight - 1; y++)
        {
            List<MapNode> possibleNextNodes = new List<MapNode>();

            if (y == 0) // y=0 -> y=1로 갈 때: y=1의 모든 x 노드로 연결 가능
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    possibleNextNodes.Add(grid[x, y + 1]);
                }
            }
            else if (y == mapHeight - 2) // y=13 -> y=14로 갈 때: y=14의 x=2 노드로만 연결
            {
                possibleNextNodes.Add(endNode);
            }
            else // 일반적인 이동 규칙: x-1, x, x+1
            {
                int currentX = currentNode.Position.x;
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nextX = currentX + dx;
                    if (nextX >= 0 && nextX < mapWidth)
                    {
                        possibleNextNodes.Add(grid[nextX, y + 1]);
                    }
                }
            }

            if (possibleNextNodes.Count > 0)
            {
                MapNode nextNode = possibleNextNodes[Random.Range(0, possibleNextNodes.Count)];
                nodesInPaths.Add(nextNode);
                currentNode = nextNode;
            }
            else
            {
                Debug.LogWarning($"[MapGenerator] 경로 생성 중 다음 노드를 찾을 수 없습니다. Y:{y}, X:{currentNode.Position.x}");
                break; // 경로 생성 실패
            }
        }
    }

    /// <summary>
    /// 최종 노드 리스트에 노드 타입을 할당하고, 노드 간 연결을 설정합니다.
    /// </summary>
    private void AssignNodeTypesAndConnectNodes(List<MapNode> finalNodes, MapNode[,] grid)
    {
        // 노드 타입 할당
        foreach (var node in finalNodes)
        {
            node.NodeType = GetNodeTypeForPosition(node.Position.y);
        }

        // 노드 연결
        foreach (var currentNode in finalNodes)
        {
            // y=14 노드는 다음 노드가 없습니다.
            if (currentNode.Position.y == mapHeight - 1) continue;

            List<MapNode> possibleNextLayerNodes = new List<MapNode>();

            if (currentNode.Position.y == 0) // y=0 노드: y=1에 있는 모든 노드와 연결
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    MapNode nextNodeCandidate = grid[x, 1];
                    if (finalNodes.Contains(nextNodeCandidate))
                    {
                        possibleNextLayerNodes.Add(nextNodeCandidate);
                    }
                }
            }
            else if (currentNode.Position.y == mapHeight - 2) // y=13 노드: y=14의 x=2 노드와 연결
            {
                MapNode nextNodeCandidate = grid[mapWidth / 2, mapHeight - 1];
                if (finalNodes.Contains(nextNodeCandidate))
                {
                    possibleNextLayerNodes.Add(nextNodeCandidate);
                }
            }
            else // 일반적인 이동 규칙: x-1, x, x+1 범위 내의 다음 층 노드와 연결
            {
                int currentX = currentNode.Position.x;
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nextX = currentX + dx;
                    if (nextX >= 0 && nextX < mapWidth)
                    {
                        MapNode nextNodeCandidate = grid[nextX, currentNode.Position.y + 1];
                        if (finalNodes.Contains(nextNodeCandidate))
                        {
                            possibleNextLayerNodes.Add(nextNodeCandidate);
                        }
                    }
                }
            }

            // 실제 연결
            foreach (var nextNode in possibleNextLayerNodes)
            {
                currentNode.NextNodes.Add(nextNode);
            }
        }
    }

    /// <summary>
    /// y 좌표에 따라 노드 타입을 결정합니다.
    /// </summary>
    private NodeType GetNodeTypeForPosition(int y)
    {
        // 시작(y=0)과 끝(y=14)은 항상 몬스터 노드
        if (y == 0 || y == mapHeight - 1)
        {
            return NodeType.Monster;
        }

        // 디버그 옵션이 켜져 있으면 항상 몬스터 반환
        if (forceAllMonsters)
        {
            return NodeType.Monster;
        }

        // ---- 향후 구현을 위한 확률 기반 노드 타입 결정 로직 ----
        // 현재는 주석 처리되어 있으며, forceAllMonsters가 false일 때 동작합니다.

        float totalChance = monsterChance + restChance + eventChance + merchantChance;
        float randomValue = Random.Range(0, totalChance);

        // if (randomValue < monsterChance)
        // {
        //     return NodeType.Monster;
        // }
        // else if (randomValue < monsterChance + restChance)
        // {
        //     // return NodeType.Rest; // 기능 구현 시 주석 해제
        //     return NodeType.Monster; // 현재는 몬스터로 대체
        // }
        // else if (randomValue < monsterChance + restChance + eventChance)
        // {
        //     // return NodeType.Event; // 기능 구현 시 주석 해제
        //     return NodeType.Monster; // 현재는 몬스터로 대체
        // }
        // else
        // {
        //     // return NodeType.Shop; // 기능 구현 시 주석 해제
        //     return NodeType.Monster; // 현재는 몬스터로 대체
        // }

        // 위 로직을 모두 주석처리하고 몬스터로 고정합니다.
        return NodeType.Monster;
    }
}