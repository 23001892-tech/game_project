using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator anim;

    [Header("Collision Details")]
    [SerializeField] protected float groundCheckDistance = 1.4f;
    [SerializeField] protected LayerMask whatIsGround;
    protected bool isGrounded;

    [Header("Attack Details")]
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius = 1f;
    [SerializeField] protected LayerMask whatIsTarget; // Target cho Player là Enemy, cho Enemy là Player

    [Header("Movement Details")]
    [SerializeField] protected float moveSpeed = 8f;
    [SerializeField] protected float jumpForce = 12f;

    protected float xInput;
    protected int facingDirection = 1; 
    protected bool facingRight = true;
    protected bool canMove = true;
    protected bool canJump = true; 

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    protected virtual void Update()
    {
        HandleCollision();
        HandleInput(); 
        HandleAnimations();
        HandleMovement();
        HandleFlip();
    }

    protected virtual void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryToJump();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            HandleAttack(); // Đổi tên từ TryToAttack thành HandleAttack
        }
    }

    // Thêm virtual để Enemy có thể tự định nghĩa lại việc phát hiện mục tiêu
    protected virtual void HandleCollision() 
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }

    protected virtual void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
    }

    protected virtual void HandleMovement()
    {
        if (canMove)
        {
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    protected virtual void TryToJump()
    {
        if (isGrounded && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // Thêm virtual để Enemy tự chém khi tới gần người chơi
    protected virtual void HandleAttack()
    {
        if (isGrounded)
        {
            anim.SetTrigger("attack");
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    protected virtual void HandleFlip()
    {
        if (rb.linearVelocity.x > 0 && !facingRight)
            Flip();
        else if (rb.linearVelocity.x < 0 && facingRight)
            Flip();
    }

    public virtual void Flip()
    {
        facingDirection = facingDirection * -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180f, 0);
    }

    public virtual void DamageTargets()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);
        foreach (Collider2D target in colliders)
        {
            Entity targetEntity = target.GetComponent<Entity>();
            if (targetEntity != null)
            {
                targetEntity.TakeDamage();
            }
        }
    }

    public virtual void TakeDamage()
    {
        // Ở bước này tạm thời chưa làm phần máu và chết (Damage and Death) nên tác giả chỉ in ra Console
        Debug.Log(gameObject.name + " took some damage");
    }

    public virtual void EnableMovement(bool enable)
    {
        canMove = enable;
        canJump = enable;
    }

    public virtual void Animation_DisableMovementAndJump()
    {
        EnableMovement(false);
    }

    public virtual void Animation_EnableMovementAndJump()
    {
        EnableMovement(true);
    }

    public virtual void Animation_OpenComboWindow()
    {
        // Override in subclasses if combo state is needed.
    }

    public virtual void Animation_CloseComboWindow()
    {
        // Override in subclasses if combo state is needed.
    }

    public virtual void Animation_FinishAttack()
    {
        EnableMovement(true);
    }

    public virtual void Animation_DamageTargets()
    {
        DamageTargets();
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance, 0));
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}