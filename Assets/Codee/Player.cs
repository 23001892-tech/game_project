using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    private bool facingRight = true;
    private float xInput;
    private bool canMove = true;
    private bool canJump = true;
    private bool isGrounded;

    private int comboStep = 0;
    private bool isAttacking = false;
    private int queuedComboClicks = 0;
    private Collider2D playerCollider;
    
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [SerializeField] private int maxMana = 50;
    [SerializeField] private int currentMana;
    
    [Header("UI")]
    [SerializeField] private PlayerUI playerUI;

    [Header("Movement Details")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float jumpForce = 8;
    

    [Header("Collision Details")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask whatIsGround;


    [Header("Attack Combo")]
    [SerializeField] private int maxCombo = 4;
    [SerializeField] private float attack1Duration = 0.6f;
    [SerializeField] private float attack2Duration = 0.55f;
    [SerializeField] private float attack3Duration = 0.7f;
    [SerializeField] private float attack4Duration = 0.8f;

    [Header("Attack Details")]
    [SerializeField] private float attackRadius;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask whatIsEnemy; // Lớp lọc mục tiêu kẻ địch

    private void Awake()
    {
        
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }
    private void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        playerUI.UpdateHealthBar(currentHealth, maxHealth);
        playerUI.UpdateManaBar(currentMana, maxMana);
    }

    private void Update()
    {
        HandleCollision();

        HandleInput();

        HandldeMovement();

        HandldeAnimations();
        
        HandleFlip();
        
        // TEST
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            UseMana(5);
        }
    }

    public void DamageEnemies()
    {
        // 1. Quét tất cả quái nằm trong vòng tròn bán kính attackRadius
         Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsEnemy);

         foreach (Collider2D enemy in enemyColliders)
        {
            enemy.GetComponent<Enemy>().TakeDamage();
        }
        
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        playerUI.UpdateHealthBar(currentHealth, maxHealth);

        Debug.Log("Player HP: " + currentHealth);
    }
    
    public void UseMana(int amount)
    {
        currentMana -= amount;

        if (currentMana < 0)
            currentMana = 0;

        playerUI.UpdateManaBar(currentMana, maxMana);
    }
    public void EnableMovementAndJump(bool enable)
    {
        canMove = enable;
        canJump = enable;
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
    Bounds bounds = playerCollider.bounds;

    Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y);
    Vector2 boxSize = new Vector2(bounds.size.x * 0.8f, 0.08f);

    RaycastHit2D hit = Physics2D.BoxCast(
        boxCenter,
        boxSize,
        0f,
        Vector2.down,
        groundCheckDistance,
        whatIsGround
    );

    isGrounded = hit.collider != null;
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
    if (!isGrounded)
        return;

    if (!isAttacking)
    {
        queuedComboClicks = 0;
        comboStep = 1;
        StartCoroutine(ComboRoutine());
    }
    else
    {
        if (comboStep + queuedComboClicks < maxCombo)
        {
            queuedComboClicks++;
            Debug.Log("Queue next attack: " + queuedComboClicks);
        }
    }
}

private IEnumerator ComboRoutine()
{
    isAttacking = true;
    EnableMovementAndJump(false);

    while (true)
    {
        Debug.Log("Play Attack " + comboStep);

        anim.SetTrigger("attack" + comboStep);

        yield return new WaitForSeconds(GetAttackDuration(comboStep));

        if (queuedComboClicks > 0 && comboStep < maxCombo)
        {
            queuedComboClicks--;
            comboStep++;
        }
        else
        {
            break;
        }
    }

    EndCombo();
}

private float GetAttackDuration(int step)
{
    if (step == 1) return attack1Duration;
    if (step == 2) return attack2Duration;
    if (step == 3) return attack3Duration;
    if (step == 4) return attack4Duration;

    return 0.6f;
}

private void EndCombo()
{
    Debug.Log("End Combo");

    isAttacking = false;
    queuedComboClicks = 0;
    comboStep = 0;

    EnableMovementAndJump(true);
}
    

    private void HandldeMovement()
    {
        if (canMove) 
        {
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y); // [4]
        }
        else
        {
             rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
        }

    }
         

    private void TryToJump()
    {
        if (isGrounded && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);  
        }
    }

   private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col == null)
            return;

        Bounds bounds = col.bounds;

        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - groundCheckDistance);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.8f, 0.08f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter, boxSize);

        if (attackPoint != null) 
        {
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
    public void Animation_DisableMovementAndJump()
{
    EnableMovementAndJump(false);
}

public void Animation_EnableMovementAndJump()
{
    EnableMovementAndJump(true);
}

public void Animation_OpenComboWindow()
{
    // Sau này nếu muốn dùng combo window bằng animation event thì xử lý ở đây.
    Debug.Log("Open Combo Window");
}

public void Animation_CloseComboWindow()
{
    // Sau này nếu muốn đóng combo window bằng animation event thì xử lý ở đây.
    Debug.Log("Close Combo Window");
}

public void Animation_FinishAttack()
{
    // Hiện tại combo đang chạy bằng thời gian trong Player.cs.
    // Hàm này để sau này dùng animation event kết thúc đòn đánh.
    Debug.Log("Animation Finish Attack");
}
}