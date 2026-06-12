using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Entity
{
    [Header("Player Mana")]
    [SerializeField] private int maxMana = 50;
    [SerializeField] private int currentMana;

    [Header("Player UI")]
    [SerializeField] private PlayerUI playerUI;

    [Header("Attack Combo")]
    [SerializeField] private int maxCombo = 4;
    [SerializeField] private float attack1Duration = 0.6f;
    [SerializeField] private float attack2Duration = 0.55f;
    [SerializeField] private float attack3Duration = 0.7f;
    [SerializeField] private float attack4Duration = 0.8f;

    private int comboStep = 0;
    private bool isAttacking = false;
    private int queuedComboClicks = 0;

    [Header("Jump")]
    [SerializeField] private int maxAirJumps = 1;
    [SerializeField] private float fallMultiplier = 2.2f;
    [SerializeField] private float lowJumpMultiplier = 1.6f;
    [SerializeField] private float maxFallSpeed = 18f;

    private int airJumpCount = 0;

    [Header("Wall Grab / Wall Jump")]
    [SerializeField] private KeyCode wallGrabKey = KeyCode.LeftControl;
    [SerializeField] private float wallCheckDistance = 0.18f;
    [SerializeField] private float wallJumpXForce = 8f;
    [SerializeField] private float wallJumpYForce = 10f;
    [SerializeField] private float wallJumpControlLockTime = 0.18f;
    [SerializeField] private float wallJumpCooldown = 0.15f;

    private bool isTouchingWall;
    private bool isWallGrabbing;
    private bool isWallJumping;

    private int wallSide; // -1 = tường bên trái, 1 = tường bên phải
    private float wallJumpTimer;
    private float lastWallJumpTime = -999f;

    [Header("Dash / Dodge")]
    [SerializeField] private KeyCode dashKey = KeyCode.Mouse1;
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.8f;
    [SerializeField] private bool invincibleWhileDashing = true;

    private bool isDashing;
    private float lastDashTime = -999f;

    private float normalGravityScale = 1f;

    protected override void Awake()
    {
        base.Awake();

        currentMana = maxMana;

        if (rb != null)
        {
            normalGravityScale = rb.gravityScale;
        }
    }

    private void Start()
    {
        if (playerUI != null)
        {
            playerUI.UpdateHealthBar(currentHealth, maxHealth);
            playerUI.UpdateManaBar(currentMana, maxMana);
        }

        string savedScene = PlayerPrefs.GetString("LastSavedScene", "");
        string currentScene = SceneManager.GetActiveScene().name;

        if (savedScene == currentScene &&
            PlayerPrefs.HasKey("PlayerX") &&
            PlayerPrefs.HasKey("PlayerY"))
        {
            float x = PlayerPrefs.GetFloat("PlayerX");
            float y = PlayerPrefs.GetFloat("PlayerY");
            transform.position = new Vector3(x, y, 0f);

            currentHealth = PlayerPrefs.GetInt("PlayerHealth", maxHealth);
            currentMana = PlayerPrefs.GetInt("PlayerMana", maxMana);

            if (playerUI != null)
            {
                playerUI.UpdateHealthBar(currentHealth, maxHealth);
                playerUI.UpdateManaBar(currentMana, maxMana);
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(5);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            UseMana(5);
        }
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();
        CheckWall();

        if (isGrounded)
        {
            airJumpCount = 0;
            isWallJumping = false;
            isWallGrabbing = false;

            if (!isDashing && rb != null)
            {
                rb.gravityScale = normalGravityScale;
            }
        }
    }

    private void CheckWall()
    {
        if (col == null)
        {
            isTouchingWall = false;
            wallSide = 0;
            return;
        }

        Bounds bounds = col.bounds;

        Vector2 boxSize = new Vector2(
            0.08f,
            bounds.size.y * 0.75f
        );

        Vector2 rightBoxCenter = new Vector2(
            bounds.max.x,
            bounds.center.y
        );

        Vector2 leftBoxCenter = new Vector2(
            bounds.min.x,
            bounds.center.y
        );

        RaycastHit2D rightHit = Physics2D.BoxCast(
            rightBoxCenter,
            boxSize,
            0f,
            Vector2.right,
            wallCheckDistance,
            whatIsGround
        );

        RaycastHit2D leftHit = Physics2D.BoxCast(
            leftBoxCenter,
            boxSize,
            0f,
            Vector2.left,
            wallCheckDistance,
            whatIsGround
        );

        if (rightHit.collider != null)
        {
            isTouchingWall = true;
            wallSide = 1;
        }
        else if (leftHit.collider != null)
        {
            isTouchingWall = true;
            wallSide = -1;
        }
        else
        {
            isTouchingWall = false;
            wallSide = 0;
        }
    }

    protected override void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryToJump();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryToAttack();
        }

        if (Input.GetKeyDown(dashKey))
        {
            TryDash();
        }
    }

    protected override void HandleMovement()
    {
        if (!canMove)
        {
            StopMove();
            return;
        }

        if (isDashing)
        {
            return;
        }

        HandleWallGrab();

        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;

            if (wallJumpTimer <= 0)
            {
                isWallJumping = false;
            }

            ApplyBetterJump();
            return;
        }

        if (!isWallGrabbing)
        {
            MoveX(xInput);
            ApplyBetterJump();
        }
    }

    private void HandleWallGrab()
    {
        if (rb == null)
            return;

        bool holdingGrabKey = Input.GetKey(wallGrabKey);

        isWallGrabbing =
            holdingGrabKey &&
            isTouchingWall &&
            !isGrounded &&
            !isWallJumping &&
            !isDashing;

        if (isWallGrabbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.gravityScale = normalGravityScale;
        }
    }

    private void ApplyBetterJump()
    {
        if (rb == null)
            return;

        if (isWallGrabbing || isDashing)
            return;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.deltaTime;
        }

        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    protected override void TryToJump()
    {
        if (!canJump || rb == null)
            return;

        if (isDashing)
            return;

        if (CanWallJump())
        {
            WallJump();
            return;
        }

        if (isGrounded)
        {
            NormalJump();
            return;
        }

        if (airJumpCount < maxAirJumps)
        {
            AirJump();
            return;
        }

        Debug.Log("Hết số lần nhảy trên không.");
    }

    private bool CanWallJump()
    {
        if (!isTouchingWall)
            return false;

        if (isGrounded)
            return false;

        if (!Input.GetKey(wallGrabKey) && !isWallGrabbing)
            return false;

        if (Time.time < lastWallJumpTime + wallJumpCooldown)
            return false;

        return true;
    }

    private void NormalJump()
    {
        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        airJumpCount = 0;
        isWallGrabbing = false;
    }

    private void AirJump()
    {
        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        airJumpCount++;
        isWallGrabbing = false;

        Debug.Log("Air Jump: " + airJumpCount + "/" + maxAirJumps);
    }

    private void WallJump()
    {
        int jumpDirection = -wallSide;

        if (jumpDirection == 0)
        {
            jumpDirection = facingRight ? -1 : 1;
        }

        rb.gravityScale = normalGravityScale;

        rb.linearVelocity = new Vector2(
            jumpDirection * wallJumpXForce,
            wallJumpYForce
        );

        isWallGrabbing = false;
        isWallJumping = true;

        wallJumpTimer = wallJumpControlLockTime;
        lastWallJumpTime = Time.time;

        airJumpCount = 0;

        if (jumpDirection > 0 && !facingRight)
        {
            Flip();
        }
        else if (jumpDirection < 0 && facingRight)
        {
            Flip();
        }
    }

    private void TryDash()
    {
        if (rb == null)
            return;

        if (!canMove)
            return;

        if (isDashing)
            return;

        if (Time.time < lastDashTime + dashCooldown)
        {
            Debug.Log("Dash đang hồi chiêu.");
            return;
        }

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        lastDashTime = Time.time;

        isWallGrabbing = false;
        isWallJumping = false;

        rb.gravityScale = 0f;

        int dashDirection;

        if (xInput != 0)
        {
            dashDirection = xInput > 0 ? 1 : -1;
        }
        else
        {
            dashDirection = facingRight ? 1 : -1;
        }

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        SetAnimatorTriggerIfExists("dash");

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        isDashing = false;
    }

    protected override void HandleAnimations()
    {
        base.HandleAnimations();

        SetAnimatorBoolIfExists("isWallGrabbing", isWallGrabbing);
        SetAnimatorBoolIfExists("isWallJumping", isWallJumping);
        SetAnimatorBoolIfExists("isDashing", isDashing);
    }

    private void SetAnimatorBoolIfExists(string parameterName, bool value)
    {
        if (anim == null)
            return;

        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(parameterName, value);
                return;
            }
        }
    }

    private void SetAnimatorTriggerIfExists(string parameterName)
    {
        if (anim == null)
            return;

        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(parameterName);
                return;
            }
        }
    }

    private void TryToAttack()
    {
        if (!isGrounded)
            return;

        if (isDashing)
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

    private void ResetAttackTriggers()
    {
        if (anim == null)
            return;

        anim.ResetTrigger("attack1");
        anim.ResetTrigger("attack2");
        anim.ResetTrigger("attack3");
        anim.ResetTrigger("attack4");
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

        EnableMovement(true);
    }

    public override void TakeDamage(int damage)
    {
        if (isDashing && invincibleWhileDashing)
        {
            Debug.Log("Dash né sát thương.");
            return;
        }

        base.TakeDamage(damage);

        if (playerUI != null)
        {
            playerUI.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;

        if (currentMana < 0)
            currentMana = 0;

        if (playerUI != null)
        {
            playerUI.UpdateManaBar(currentMana, maxMana);
        }
    }

    public override void Animation_DisableMovementAndJump()
    {
        EnableMovement(false);
        StopMove();
    }

    public override void Animation_EnableMovementAndJump()
    {
        if (!isAttacking)
        {
            EnableMovement(true);
        }
    }

    public override void Animation_OpenComboWindow()
    {
        Debug.Log("Open Combo Window");
    }

    public override void Animation_CloseComboWindow()
    {
        Debug.Log("Close Combo Window");
    }

    public override void Animation_FinishAttack()
    {
        Debug.Log("Animation Finish Attack");
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("PlayerX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerY", transform.position.y);
        PlayerPrefs.SetInt("PlayerHealth", currentHealth);
        PlayerPrefs.SetInt("PlayerMana", currentMana);
        PlayerPrefs.SetString("LastSavedScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.SetInt("HasSavedGame", 1);
        PlayerPrefs.Save();
    }

    protected override void Die()
    {
        if (isDead) return;

        isDead = true;
        isDying = false;
        canMove = false;
        canJump = false;

        Debug.Log("Player đã tử trận!");

        StopAllCoroutines();
        isAttacking = false;
        isDashing = false;
        isWallGrabbing = false;
        isWallJumping = false;

        if (rb != null)
        {
            rb.gravityScale = normalGravityScale;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (anim != null)
        {
            anim.SetTrigger("die");
        }

        if (col != null)
        {
            col.enabled = false;
        }

        StartCoroutine(ShowGameOverScreenWithDelay(1.5f));
    }

    private IEnumerator ShowGameOverScreenWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOverScreen();
        }
        else
        {
            Debug.LogError("Chưa có GameManager Singleton trong Scene để bật UI Game Over!");
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Collider2D gizmoCol = GetComponent<Collider2D>();

        if (gizmoCol == null)
        {
            gizmoCol = GetComponentInChildren<Collider2D>();
        }

        if (gizmoCol == null)
            return;

        Bounds bounds = gizmoCol.bounds;

        Vector2 boxSize = new Vector2(
            0.08f,
            bounds.size.y * 0.75f
        );

        Vector2 rightBoxCenter = new Vector2(bounds.max.x, bounds.center.y);
        Vector2 leftBoxCenter = new Vector2(bounds.min.x, bounds.center.y);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(rightBoxCenter, boxSize);
        Gizmos.DrawWireCube(leftBoxCenter, boxSize);
    }
}