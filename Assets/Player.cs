using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    private bool facingRight = true;
    private float xInput;
    private bool isGrounded;
    
    [Header("Movement Details")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float jumpForce = 8;
    

    [Header("Collision Details")]
    [SerializeField] private float groundCheckDistance;

   
    [SerializeField] private LayerMask whatIsGround; // [8]
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleCollision();

        HandleInput();

        HandldeMovement();

        HandldeAnimations();
        
        HandleFlip();
        
        
    }

     private void Flip()
    {
        // Xoay nhân vật 180 độ theo trục Y thay vì dùng hàm flipX trên SpriteRenderer
        transform.Rotate(0, 180f, 0); // [18]
        facingRight = !facingRight;   // [19]
    }
    private void HandleFlip()
    {
        // Kiểm tra nếu đang di chuyển sang phải nhưng mặt quay sang trái, hoặc ngược lại thì lật nhân vật
        if (rb.linearVelocity.x > 0 && !facingRight) // [13]
        {
            Flip();
        }
        else if (rb.linearVelocity.x < 0 && facingRight) // [13]
        {
            Flip();
        }
    }
    private void HandleCollision()
    {
        // Bắn một tia Raycast xuống dưới để kiểm tra chạm đất
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround); // [8]
    }
    private void HandldeAnimations()
    {
    
        anim.SetFloat("xVelocity", rb.linearVelocity.x); 
        anim.SetFloat("yVelocity", rb.linearVelocity.y); // [21]
        anim.SetBool("isGrounded", isGrounded);
    }  
    private void HandleInput()
    {
        // Sử dụng GetAxisRaw để nhân vật đạt tốc độ tối đa ngay lập tức (snappy controls)
        xInput = Input.GetAxisRaw("Horizontal"); // [15]

        // Kiểm tra đầu vào cho chức năng nhảy
        if (Input.GetKeyDown(KeyCode.Space)) // [16]
        {
            TryToJump();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
             TryToAttack();
        }
    }
    private void TryToAttack()
    {
        // Chỉ cho phép chém khi đang đứng trên mặt đất (tránh lỗi animation khi nhảy)
        if (isGrounded)
        {
            anim.SetTrigger("attack");
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Dừng lại ngay lập tức
        }
    }

    private void HandldeMovement()
    {
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y); // [4]
    }
         

    private void TryToJump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);  
        }
    }

    private void OnDrawGizmos()
    {
        // Vẽ tia line trực quan trong Unity Editor giúp dễ dàng điều chỉnh độ dài kiểm tra chạm đất
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance)); // [20]
    }
}