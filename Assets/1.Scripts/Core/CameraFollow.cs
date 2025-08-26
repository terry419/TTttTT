using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // ī޶   (÷̾)
    public float smoothSpeed = 0.125f; // ī޶ 󰡴 ε巯 
    public Vector3 offset; // ī޶   Ÿ

    // ÷̾    Ŀ ī޶ ̵ LateUpdate մϴ.
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