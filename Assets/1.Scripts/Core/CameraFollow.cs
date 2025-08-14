using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 카메라가 따라갈 대상 (플레이어)
    public float smoothSpeed = 0.125f; // 카메라가 따라가는 부드러움의 정도
    public Vector3 offset; // 카메라와 대상 사이의 거리

    // 플레이어의 움직임이 모두 끝난 후에 카메라가 움직이도록 LateUpdate를 사용합니다.
    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}