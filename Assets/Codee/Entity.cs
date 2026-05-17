using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Components")]
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D entityCollider;
    protected Collider2D col;
    protected SpriteRenderer sr;

    protected SpriteRenderer[] spriteRenderers;
    protected Material[] originalMaterials;

    [Header("Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;

    [Header("Movement Details")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float jumpForce = 8f;

    protected float xInput;
    protected bool canMove = true;
    protected bool canJump = true;

    protected int facDir = 1;
    protected int facingDir => facDir;
    protected int facingDirection => facDir;
    protected bool facingRight = true;

    [Header("Attack Combo")]
    [SerializeField] protected int maxCombo = 4;
    [SerializeField] protected float attack1Duration = 0.6f;
    [SerializeField] protected float attack2Duration = 0.55f;
    [SerializeField] protected float attack3Duration = 0.7f;
    [SerializeField] protected float attack4Duration = 0.8f;

    protected int comboStep = 0;
    protected bool isAttacking = false;
    protected int queuedComboClicks = 0;

    [Header("Health & Damage Details")]
    [SerializeField] protected Material damageMaterial;
    [SerializeField] protected float damageFeedbackDuration = 0.15f;
    protected Coroutine damageFeedbackCoroutine;

    [Header("Death Details")]
    [SerializeField] protected float deathDestroyDelay = 3f;
    [SerializeField] protected float deathJumpForce = 15f;
    [SerializeField] protected float deathGravityScale = 3.5f;

    protected bool isDead;
    protected bool isDying;

    [Header("Collision Details")]
    [SerializeField] protected float groundCheckDistance = 0.1f;
    [SerializeField] protected LayerMask whatIsGround;
    protected bool isGrounded;

    [Header("Attack Details")]
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float attackRadius = 0.6f;
    [SerializeField] protected int attackDamage = 10;
    [SerializeField] protected LayerMask whatIsTarget;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        entityCollider = GetComponent<Collider2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

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

        HandleInput();
        HandleCollision();
        HandleMovement();
        HandleAnimations();
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
            HandleAttack();
        }
    }

    protected virtual void HandleCollision()
    {
        if (entityCollider == null)
            return;

        Bounds bounds = entityCollider.bounds;

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
        if (rb == null)
            return;

        if (canMove)
        {
            MoveX(xInput);
        }
        else
        {
            StopMove();
        }
    }

    protected virtual void HandleAnimations()
    {
        if (anim == null || rb == null)
            return;

        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
    }

    protected virtual void TryToJump()
    {
        if (isGrounded && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    protected virtual void HandleAttack()
    {
        TryToAttack();
    }

    protected virtual void TryToAttack()
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

    protected virtual IEnumerator ComboRoutine()
    {
        isAttacking = true;
        EnableMovement(false);

        while (true)
        {
            Debug.Log("Play Attack " + comboStep);

            ResetAttackTriggers();

            if (anim != null)
            {
                anim.SetTrigger("attack" + comboStep);
            }

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

    protected virtual void ResetAttackTriggers()
    {
        if (anim == null)
            return;

        anim.ResetTrigger("attack1");
        anim.ResetTrigger("attack2");
        anim.ResetTrigger("attack3");
        anim.ResetTrigger("attack4");
    }

    protected virtual float GetAttackDuration(int step)
    {
        if (step == 1) return attack1Duration;
        if (step == 2) return attack2Duration;
        if (step == 3) return attack3Duration;
        if (step == 4) return attack4Duration;

        return 0.6f;
    }

    protected virtual void EndCombo()
    {
        Debug.Log("End Combo");

        isAttacking = false;
        queuedComboClicks = 0;
        comboStep = 0;

        EnableMovement(true);
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

        Debug.Log(gameObject.name + " HP: " + currentHealth);

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
        if (isDead)
            return;

        isDead = true;
        isDying = false;

        Debug.Log(gameObject.name + " died.");

        canMove = false;
        canJump = false;

        if (anim != null)
        {
            anim.enabled = false;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, deathJumpForce);
            rb.gravityScale = deathGravityScale;
        }

        Destroy(gameObject, deathDestroyDelay);
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
            Entity entityTarget = target.GetComponentInParent<Entity>();

            if (entityTarget != null && entityTarget != this)
            {
                entityTarget.TakeDamage(attackDamage);
            }
        }
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
        if (!isAttacking)
        {
            EnableMovement(true);
        }
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
        Debug.Log("Open Combo Window");
    }

    public virtual void Animation_CloseComboWindow()
    {
        Debug.Log("Close Combo Window");
    }

    public virtual void Animation_FinishAttack()
    {
        Debug.Log("Animation Finish Attack");
    }

    protected virtual void OnDrawGizmos()
    {
        Collider2D gizmoCol = GetComponent<Collider2D>();

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