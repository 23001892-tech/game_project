using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target; // Kéo Player vào đây
    public float smoothing = 5f; // Độ mượt của camera

    void LateUpdate()
    {
        if (target != null)
        {
            // Chỉ thay đổi x và y, giữ nguyên z của camera
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            
            // Di chuyển mượt mà tới vị trí player
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
        }
    }
}