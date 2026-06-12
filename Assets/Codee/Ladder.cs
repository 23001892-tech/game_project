using UnityEngine;

public class Ladder : MonoBehaviour
{
    [Header("Cấu hình thang leo")]
    public float climbSpeed = 5f; // Tốc độ leo thang

    private bool isPlayerInZone = false; 
    private bool isClimbing = false;     
    private Rigidbody2D playerRb;        
    private float originalGravity;       

    // Hai biến dùng để tự động tính giới hạn dựa trên Collider của cây thang
    private float topY;
    private float bottomY;
    private BoxCollider2D ladderCollider;

    void Start()
    {
        // Lấy BoxCollider2D của chính cây thang để tính toán kích thước
        ladderCollider = GetComponent<BoxCollider2D>();
        if (ladderCollider != null)
        {
            // Tính toán tọa độ Y cao nhất và thấp nhất của vùng Trigger
            topY = ladderCollider.bounds.max.y;
            bottomY = ladderCollider.bounds.min.y;
        }
    }

    void Update()
    {
        if (isPlayerInZone)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                if (!isClimbing)
                {
                    StartClimbing();
                }
            }
        }

        if (isClimbing)
        {
            float moveInput = Input.GetAxisRaw("Vertical");
            float nextYVelocity = moveInput * climbSpeed;

            // ---- KHU VỰC KIỂM TRA GIỚI HẠN (TOP / BOTTOM) ----
            float playerCurrentY = playerRb.transform.position.y;

            // Nếu đang ở đỉnh thang mà vẫn cố bấm W đi lên -> Khóa tốc độ bằng 0
            if (playerCurrentY >= topY && moveInput > 0)
            {
                //nextYVelocity = 0;
                // Hoặc bạn có thể gọi StopClimbing() ở đây nếu muốn nhân vật tự động đứng hẳn lên sàn
            }
            // Nếu đang ở đáy thang mà vẫn cố bấm S đi xuống -> Tự động dừng leo để rớt xuống đất
            else if (playerCurrentY <= bottomY && moveInput < 0)
            {
                StopClimbing();
                return;
            }
            // --------------------------------------------------

            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, nextYVelocity);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopClimbing();
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 6f); 
            }
        }
    }
private void FixedUpdate()
    {
        if (isClimbing)
        {
            // Hút Player vào chính giữa tâm cây thang theo trục X trong FixedUpdate để tránh bị lệch khi di chuyển
            playerRb.transform.position = new Vector3(transform.position.x, playerRb.transform.position.y, playerRb.transform.position.z);
        }
    }
    private void StartClimbing()
    {
        isClimbing = true;
        originalGravity = playerRb.gravityScale;
        playerRb.gravityScale = 0f;

        // Hút Player vào chính giữa tâm cây thang theo trục X
        playerRb.transform.position = new Vector3(transform.position.x, playerRb.transform.position.y, playerRb.transform.position.z);
    }

    private void StopClimbing()
    {
        if (isClimbing)
        {
            isClimbing = false;
            playerRb.gravityScale = originalGravity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = true;
            playerRb = collision.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInZone = false;
            StopClimbing();
            playerRb = null;
        }
    }
}