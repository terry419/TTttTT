using UnityEngine;

/// <summary>
/// 맵의 경계를 정의하고, 해당 경계에 물리적 벽을 생성하는 컴포넌트입니다.
/// </summary>
public class MapBoundary : MonoBehaviour
{
    [SerializeField] private Vector2 mapSize = new Vector2(100f, 100f);
    [SerializeField] private float wallThickness = 1f;

    public Vector2 MapSize => mapSize;

    private void Awake()
    {
        GenerateBoundaries();
    }

    /// <summary>
    /// 맵 경계에 보이지 않는 물리적 벽을 생성합니다.
    /// </summary>
    private void GenerateBoundaries()
    {
        GameObject wallParent = new GameObject("Boundaries");
        wallParent.transform.parent = transform;

        // Top wall
        CreateWall("TopWall", new Vector2(0, mapSize.y / 2 + wallThickness / 2), new Vector2(mapSize.x + wallThickness * 2, wallThickness), wallParent.transform);

        // Bottom wall
        CreateWall("BottomWall", new Vector2(0, -mapSize.y / 2 - wallThickness / 2), new Vector2(mapSize.x + wallThickness * 2, wallThickness), wallParent.transform);

        // Left wall
        CreateWall("LeftWall", new Vector2(-mapSize.x / 2 - wallThickness / 2, 0), new Vector2(wallThickness, mapSize.y), wallParent.transform);

        // Right wall
        CreateWall("RightWall", new Vector2(mapSize.x / 2 + wallThickness / 2, 0), new Vector2(wallThickness, mapSize.y), wallParent.transform);
    }

    private void CreateWall(string name, Vector2 position, Vector2 size, Transform parent)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = parent;
        wall.transform.position = position;

        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
        
        // 벽 레이어를 설정하여 다른 오브젝트와의 충돌을 보다 세밀하게 제어할 수 있습니다.
        // 예: wall.layer = LayerMask.NameToLayer("Wall");
    }

    /// <summary>
    /// Scene 뷰에서 맵 경계를 시각적으로 표시합니다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(mapSize.x, mapSize.y, 0));
    }
}
