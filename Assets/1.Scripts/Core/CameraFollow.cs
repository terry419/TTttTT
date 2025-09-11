// 파일 경로: Assets/1.Scripts/Core/CameraFollow.cs (수정된 최종본)
using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10f); // [수정] 기본 Z 오프셋 설정

    private MapBoundary[] mapBoundaries; // [수정] 모든 경계를 담을 배열
    private Camera cam;
    private float cameraHeight;
    private float cameraWidth;

    private void Start()
    {
        // [수정] 씬에 있는 모든 MapBoundary를 찾습니다.
        mapBoundaries = FindObjectsOfType<MapBoundary>();
        if (mapBoundaries.Length == 0)
        {
            Debug.LogWarning("MapBoundary 컴포넌트를 찾을 수 없습니다. 카메라 이동이 제한되지 않습니다.");
        }

        cam = GetComponent<Camera>();
        cameraHeight = cam.orthographicSize;
        cameraWidth = cameraHeight * cam.aspect;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // [추가] 가장 가까운 경계를 찾는 로직
            if (mapBoundaries != null && mapBoundaries.Length > 0)
            {
                // target의 위치를 기준으로 가장 가까운 MapBoundary를 찾습니다.
                MapBoundary closestBoundary = mapBoundaries
                    .OrderBy(boundary => Vector3.Distance(target.position, boundary.transform.position))
                    .FirstOrDefault();

                if (closestBoundary != null)
                {
                    Vector2 mapSize = closestBoundary.MapSize;
                    // 경계의 실제 월드 위치를 중심으로 최대/최소 좌표를 계산합니다.
                    float minX = closestBoundary.transform.position.x - mapSize.x / 2 + cameraWidth;
                    float maxX = closestBoundary.transform.position.x + mapSize.x / 2 - cameraWidth;
                    float minY = closestBoundary.transform.position.y - mapSize.y / 2 + cameraHeight;
                    float maxY = closestBoundary.transform.position.y + mapSize.y / 2 - cameraHeight;

                    smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
                    smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
                }
            }

            transform.position = smoothedPosition;
        }
    }
}