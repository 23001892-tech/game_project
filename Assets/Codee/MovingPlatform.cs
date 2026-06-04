using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public Transform topPoint; // Điểm giới hạn trên
    public Transform bottomPoint; // Điểm giới hạn dưới
    public float speed = 3f;  // Tốc độ di chuyển

    private int currentDirection = 0; // 0: đứng yên, 1: đi lên, -1: đi xuống
    private bool isPlayerOnPlatform = false; // Biến kiểm tra xem người chơi có đang đứng trên thang máy hay không


    void Update()
    {
        if (isPlayerOnPlatform)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                if (currentDirection == 1)
                {
                    currentDirection = 0; // Dừng lại nếu đang đi lên
                }
                else
                {
                    currentDirection = 1; // Đi lên
                }
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (currentDirection == -1)
                {
                    currentDirection = 0; // Dừng lại nếu đang đi xuống
                }
                else
                {
                    currentDirection = -1; // Đi xuống
                }
            }
        }
        if (currentDirection == 1)
        {
            if (transform.position.y < topPoint.position.y)
            {
                transform.Translate(Vector3.up * speed * Time.deltaTime);
            }
            else
            {
                currentDirection = 0; // Dừng lại khi đạt đến điểm trên
            }
        }
        else if (currentDirection == -1)
        {
            if (transform.position.y > bottomPoint.position.y)
            {
                transform.Translate(Vector3.down * speed * Time.deltaTime);
            }
            else
            {
                currentDirection = 0; // Dừng lại khi đạt đến điểm dưới
            }
        }
    }

    // Xử lý giữ nhân vật đứng yên trên thang máy khi di chuyển
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra nếu vật chạm vào có Tag là "Player" (hoặc tùy bạn đặt)
        if (collision.gameObject.CompareTag("Player"))
        {
            // Biến thang máy thành cha của Player để Player di chuyển theo
            Debug.Log("Đã nhận diện được Player lên thang máy!"); // Thêm dòng này
            collision.transform.SetParent(transform);
            isPlayerOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Trả Player về tự do khi rời thang máy
            isPlayerOnPlatform = false;
            collision.transform.SetParent(null);
        }
    }
}