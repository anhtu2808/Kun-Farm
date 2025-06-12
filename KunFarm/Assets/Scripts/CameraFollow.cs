using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("Transform của Player mà camera sẽ theo dõi")]
    public Transform target;
    
    [Tooltip("Khoảng cách cố định giữa camera và player")]
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Tooltip("Tốc độ làm mượt chuyển động camera")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;

        // Vị trí mong muốn của camera
        Vector3 desiredPosition = target.position + offset;
        // Lerp để tạo chuyển động mượt
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
