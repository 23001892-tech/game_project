using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Collider2D col;

    protected SpriteRenderer[] spriteRenderers;
    protected Material[] originalMaterials;

    [Header("Health & Damage")]
    [SerializeField] protected int maxHealth = 10;
    [SerializeField] protected int currentHealth;

    [SerializeField] protected Material damageMaterial;
    [SerializeField] protected float damageFeedbackDuration = 0.15f;

    protected Coroutine damageFeedbackCoroutine;
    protected bool isDead;
    protected bool isDying;

    [Header("Death Details")]
    [SerializeField] protected float deathDestroyDelay = 3f;
    [SerializeField] protected float deathJumpForce = 15f;
    [SerializeField] protected float deathGravityScale = 3.5f;

    [Header("Movement Details")]
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float jumpForce = 8f;

    protected float xInput;
    protected bool canMove = true;
    protected bool canJump = true;

    protected int facDir = 1;
    protected bool facingRight = true;

    [Header("Collision Details")]
    [SerializeField] protected float groundCheckDistance = 0.1f;
    [SerializeField] protected LayerMask whatIsGround;
    protected bool isGrounded;

    [Header("Attack Details")]
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius = 1f;
    [SerializeField] protected int attackDamage = 1;
    [SerializeField] protected LayerMask whatIsTarget;

    protected virtual void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    anim = GetComponentInChildren<Animator>();

    col = GetComponent<Collider2D>();

    if (col == null)
    {
        col = GetComponentInChildren<Collider2D>();
    }

    spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    originalMaterials = new Material[spriteRenderers.Length];

    for (int i = 0; i < spriteRenderers.Length; i++)
    {
        originalMaterials[i] = spriteRenderers[i].material;
    }

    currentHealth = maxHealth;
}

    protected virtual void Update()
    {
        if (isDead)
            return;

        HandleCollision();
        HandleInput();
        HandleMovement();
        HandleAnimations();
        HandleFlip();
    }

    protected virtual void HandleInput()
    {
        // Entity cha không dùng input.
        // Player sẽ override.
    }

    protected virtual void HandleCollision()
{
    if (col == null)
    {
        Debug.LogWarning(gameObject.name + " không có Collider2D để check ground.");
        isGrounded = false;
        return;
    }

    Bounds bounds = col.bounds;

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

    protected virtual void HandleMovement()
    {
        // Entity cha không tự di chuyển.
        // Player và Enemy sẽ override.
    }

    protected virtual void HandleAnimations()
{
    if (anim == null) return;

    if (HasParameter("isGrounded", anim))
    {
        anim.SetBool("isGrounded", isGrounded);
    }
    
    if (HasParameter("yVelocity", anim))
    {
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }
}

private bool HasParameter(string paramName, Animator animator)
{
    foreach (AnimatorControllerParameter param in animator.parameters)
    {
        if (param.name == paramName) return true;
    }
    return false;
}

    protected virtual void TryToJump()
    {
        // Player sẽ override.
    }

    protected virtual void HandleAttack()
    {
        // Player hoặc Enemy sẽ override nếu cần.
    }

    protected virtual void MoveX(float direction)
    {
        if (rb == null || !canMove)
            return;

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    protected virtual void StopMove()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    protected virtual void HandleFlip()
    {
        if (rb == null)
            return;

        if (rb.linearVelocity.x > 0.1f && !facingRight)
        {
            Flip();
        }
        else if (rb.linearVelocity.x < -0.1f && facingRight)
        {
            Flip();
        }
    }

    protected virtual void Flip()
    {
        facDir *= -1;
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected virtual void FaceTarget(Transform target)
    {
        if (target == null)
            return;

        float direction = target.position.x - transform.position.x;

        if (direction > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction < 0 && facingRight)
        {
            Flip();
        }
    }

    public virtual void DamageTargets()
    {
        if (attackPoint == null)
        {
            Debug.LogWarning(gameObject.name + " chưa có AttackPoint.");
            return;
        }

        Collider2D[] targets = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            whatIsTarget
        );

        foreach (Collider2D target in targets)
        {
            Entity targetEntity = target.GetComponentInParent<Entity>();

            if (targetEntity != null && targetEntity != this)
            {
                targetEntity.TakeDamage(attackDamage);
            }
        }
    }

    public virtual void TakeDamage()
    {
        TakeDamage(1);
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead || isDying)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        Debug.Log(gameObject.name + " Current Health: " + currentHealth);

        PlayDamageFeedback();

        if (currentHealth <= 0)
        {
            isDying = true;
            StartCoroutine(DieAfterDamageFeedback());
        }
    }

    protected virtual void PlayDamageFeedback()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            Debug.LogWarning(gameObject.name + " không tìm thấy SpriteRenderer.");
            return;
        }

        if (damageMaterial == null)
        {
            Debug.LogWarning(gameObject.name + " chưa gán Damage Material.");
            return;
        }

        if (damageFeedbackCoroutine != null)
        {
            StopCoroutine(damageFeedbackCoroutine);
        }

        damageFeedbackCoroutine = StartCoroutine(DamageFeedbackCoroutine());
    }

    protected virtual IEnumerator DamageFeedbackCoroutine()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].material = damageMaterial;
            }
        }

        yield return new WaitForSeconds(damageFeedbackDuration);

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].material = originalMaterials[i];
            }
        }

        damageFeedbackCoroutine = null;
    }

    protected virtual IEnumerator DieAfterDamageFeedback()
    {
        yield return new WaitForSeconds(damageFeedbackDuration);

        Die();
    }

    protected virtual void Die()
{
    if (isDead) return;

    isDead = true;
    isDying = false;
    canMove = false;
    canJump = false;

    // Lưu trạng thái chết nếu là enemy
    var enemySave = GetComponent<EnemyDeadSave>();
    if (enemySave != null) enemySave.MarkAsDead();

    if (anim != null) anim.enabled = false;
    if (col != null) col.enabled = false;
    if (rb != null)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, deathJumpForce);
        rb.gravityScale = deathGravityScale;
    }

    Destroy(gameObject, deathDestroyDelay);
}

    public virtual void EnableMovement(bool enable)
    {
        canMove = enable;
        canJump = enable;
    }

    public virtual void Animation_DisableMovement()
    {
        EnableMovement(false);
        StopMove();
    }

    public virtual void Animation_EnableMovement()
    {
        EnableMovement(true);
    }

    public virtual void Animation_DamageTargets()
    {
        DamageTargets();
    }

    public virtual void Animation_DisableMovementAndJump()
    {
        Animation_DisableMovement();
    }

    public virtual void Animation_EnableMovementAndJump()
    {
        Animation_EnableMovement();
    }

    public virtual void Animation_OpenComboWindow()
    {
        // Player override nếu cần.
    }

    public virtual void Animation_CloseComboWindow()
    {
        // Player override nếu cần.
    }

    public virtual void Animation_FinishAttack()
    {
        // Player override nếu cần.
    }

    protected virtual void OnDrawGizmos()
{
    Collider2D gizmoCol = GetComponent<Collider2D>();

    if (gizmoCol == null)
    {
        gizmoCol = GetComponentInChildren<Collider2D>();
    }

    if (gizmoCol != null)
    {
        Bounds bounds = gizmoCol.bounds;

        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y - groundCheckDistance);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.8f, 0.08f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }

    if (attackPoint != null)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
}