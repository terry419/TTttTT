using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 카메라가 따라갈 대상 (플레이어)
    public float smoothSpeed = 0.125f; // 카메라 부드러운 이동 속도
    public Vector3 offset; // 카메라와 대상 간의 오프셋

    private MapBoundary mapBoundary;
    private Camera cam;
    private float cameraHeight;
    private float cameraWidth;

    private void Start()
    {
        mapBoundary = FindObjectOfType<MapBoundary>();
        if (mapBoundary == null)
        {
            Debug.LogWarning("MapBoundary 컴포넌트를 찾을 수 없습니다. 카메라 이동이 제한되지 않습니다.");
        }

        cam = GetComponent<Camera>();
        cameraHeight = cam.orthographicSize;
        cameraWidth = cameraHeight * cam.aspect;
    }

    // 플레이어를 따라 카메라 이동 - 다른 업데이트 후에 실행되도록 LateUpdate 사용
    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            if (mapBoundary != null)
            {
                Vector2 mapSize = mapBoundary.MapSize;
                float minX = -mapSize.x / 2 + cameraWidth;
                float maxX = mapSize.x / 2 - cameraWidth;
                float minY = -mapSize.y / 2 + cameraHeight;
                float maxY = mapSize.y / 2 - cameraHeight;

                smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
                smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
            }

            transform.position = smoothedPosition;
        }
    }
}
