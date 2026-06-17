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
    [SerializeField] private int maxCombo = 3;
    [SerializeField] private float attack1Duration = 0.6f;
    [SerializeField] private float attack2Duration = 0.55f;
    [SerializeField] private float attack3Duration = 0.7f;

    [Header("Attack Cancel")]
    [SerializeField] private bool canCancelAttackByMove = true;
    [SerializeField] private bool canCancelAttackByJump = true;
    [SerializeField] private bool canCancelAttackByDash = true;
    [SerializeField] private float attackCancelInputDelay = 0.03f;
    [SerializeField] private float attackCancelTransitionTime = 0.05f;
    [SerializeField] private string locomotionStateName = "idle/move";
    [SerializeField] private string airStateName = "jump/fall";

    private int comboStep = 0;
    private bool isAttacking = false;
    private int queuedComboClicks = 0;
    private Coroutine comboCoroutine;
    private float attackStartTime;

    [Header("Jump")]
    [SerializeField] private int maxAirJumps = 1;
    [SerializeField] private float fallMultiplier = 2.2f;
    [SerializeField] private float lowJumpMultiplier = 1.6f;
    [SerializeField] private float maxFallSpeed = 18f;
    [SerializeField] private float jumpGroundCheckDisableTime = 0.1f;

    private int airJumpCount = 0;
    private float groundCheckDisableTimer;

    [Header("Wall Grab / Wall Jump")]
    [SerializeField] private KeyCode wallGrabKey = KeyCode.LeftControl;
    [SerializeField] private float wallCheckDistance = 0.25f;
    [SerializeField] private float wallGrabJumpXForce = 10f;
    [SerializeField] private float wallJumpControlLockTime = 0.2f;
    [SerializeField] private float wallJumpCooldown = 0.15f;
    [SerializeField] private float wallGrabReEnableDelay = 0.3f;

    private bool isTouchingWall;
    private bool isWallGrabbing;
    private bool isWallJumping;

    private int wallSide;
    private float wallJumpTimer;
    private float wallGrabDisableTimer;
    private float lastWallJumpTime = -999f;

    [Header("Dash / Dodge")]
    [SerializeField] private KeyCode dashKey = KeyCode.Mouse1;
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.8f;
    [SerializeField] private bool invincibleWhileDashing = true;

    private bool isDashing;
    private float lastDashTime = -999f;

    [Header("Physics Fix")]
    [SerializeField] private bool autoSetNoFrictionMaterial = true;

    private float normalGravityScale = 1f;
    private PhysicsMaterial2D noFrictionMaterial;

    protected override void Awake()
    {
        base.Awake();

        currentMana = maxMana;

        if (rb != null)
        {
            normalGravityScale = rb.gravityScale;
        }

        if (autoSetNoFrictionMaterial && col != null)
        {
            noFrictionMaterial = new PhysicsMaterial2D("Player_No_Friction");
            noFrictionMaterial.friction = 0f;
            noFrictionMaterial.bounciness = 0f;
            col.sharedMaterial = noFrictionMaterial;
        }
    }

    private void Start()
{
    SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
    if (sprite != null)
    {
        sprite.enabled = true;
    }

    // 1. Đọc dữ liệu từ file JSON lên RAM
    if (!GameSession.SessionStarted)
    {
        if (GameSession.CurrentGameState == GameState.NewGame || !SaveSystem.LoadGame())
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
        }
        else
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (SaveSystem.currentData.lastSavedScene == currentSceneName)
            {
                transform.position = new Vector3(
                    SaveSystem.currentData.playerX,
                    SaveSystem.currentData.playerY,
                    0f
                );
            }

            currentHealth = SaveSystem.currentData.currentHealth;
            currentMana = SaveSystem.currentData.currentMana;
        }

        GameSession.SessionStarted = true;
    }
    else
    {
        SaveSystem.LoadGame();
        currentHealth = SaveSystem.currentData.currentHealth;
        currentMana = SaveSystem.currentData.currentMana;
    }

    if (currentHealth <= 0)
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    UpdateUI();
    
    SaveCurrentState();
}

    protected override void Update()
    {
        base.Update();

        if (isDead)
            return;

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

        if (groundCheckDisableTimer > 0f)
        {
            groundCheckDisableTimer -= Time.deltaTime;
            isGrounded = false;
        }

        CheckWall();

        if (isGrounded)
        {
            airJumpCount = 0;
            isWallJumping = false;
            isWallGrabbing = false;
            wallGrabDisableTimer = 0f;

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

    private bool IsPressingIntoWall()
    {
        if (!isTouchingWall || wallSide == 0)
            return false;

        if (wallSide == 1 && xInput > 0.1f)
            return true;

        if (wallSide == -1 && xInput < -0.1f)
            return true;

        return false;
    }

    private bool IsPressingAwayFromWall()
    {
        if (!isTouchingWall || wallSide == 0)
            return false;

        if (wallSide == 1 && xInput < -0.1f)
            return true;

        if (wallSide == -1 && xInput > 0.1f)
            return true;

        return false;
    }

    protected override void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (HandleAttackCancelInput())
            return;

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

    private bool HandleAttackCancelInput()
    {
        if (!isAttacking)
            return false;

        if (Time.time < attackStartTime + attackCancelInputDelay)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                TryToAttack();
                return true;
            }

            return false;
        }

        if (canCancelAttackByDash && Input.GetKeyDown(dashKey))
        {
            CancelAttack(false);
            TryDash();
            return true;
        }

        if (canCancelAttackByJump && Input.GetKeyDown(KeyCode.Space))
        {
            CancelAttack(false);
            TryToJump();

            if (anim != null && !string.IsNullOrEmpty(airStateName))
            {
                anim.CrossFadeInFixedTime(airStateName, attackCancelTransitionTime);
            }

            return true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryToAttack();
            return true;
        }

        if (canCancelAttackByMove)
        {
            bool pressedMoveKey =
                Input.GetKeyDown(KeyCode.A) ||
                Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow);

            if (pressedMoveKey)
            {
                CancelAttack(true);
                MoveX(xInput);
                return true;
            }
        }

        return false;
    }

    private void CancelAttack(bool crossFadeToLocomotion)
    {
        if (!isAttacking)
            return;

        Debug.Log("Cancel Attack");

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }

        ResetAttackTriggers();

        isAttacking = false;
        queuedComboClicks = 0;
        comboStep = 0;

        SetAnimatorBoolIfExists("isAttacking", false);

        EnableMovement(true);

        if (crossFadeToLocomotion && anim != null && !string.IsNullOrEmpty(locomotionStateName))
        {
            anim.CrossFadeInFixedTime(locomotionStateName, attackCancelTransitionTime);
        }
    }

    protected override void HandleMovement()
    {
        if (!canMove)
        {
            StopMove();
            return;
        }

        if (wallGrabDisableTimer > 0f)
        {
            wallGrabDisableTimer -= Time.deltaTime;
        }

        if (isDashing)
        {
            return;
        }

        HandleWallGrab();

        if (isWallJumping)
        {
            wallJumpTimer -= Time.deltaTime;

            if (wallJumpTimer <= 0f)
            {
                isWallJumping = false;
            }

            ApplyBetterJump();
            return;
        }

        if (!isWallGrabbing)
        {
            if (!isGrounded &&
                isTouchingWall &&
                !Input.GetKey(wallGrabKey) &&
                IsPressingIntoWall())
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                ApplyBetterJump();
                return;
            }

            MoveX(xInput);
            ApplyBetterJump();
        }
    }

    private void HandleWallGrab()
    {
        if (rb == null)
            return;

        if (wallGrabDisableTimer > 0f)
        {
            isWallGrabbing = false;
            rb.gravityScale = normalGravityScale;
            return;
        }

        bool holdingGrabKey = Input.GetKey(wallGrabKey);
        bool pressingAwayFromWall = IsPressingAwayFromWall();

        isWallGrabbing =
            holdingGrabKey &&
            isTouchingWall &&
            !isGrounded &&
            !isWallJumping &&
            !isDashing &&
            !pressingAwayFromWall;

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
            WallGrabJump();
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
        if (isGrounded)
            return false;

        if (Time.time < lastWallJumpTime + wallJumpCooldown)
            return false;

        return isWallGrabbing;
    }

    private void NormalJump()
    {
        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        isGrounded = false;
        groundCheckDisableTimer = jumpGroundCheckDisableTime;

        airJumpCount = 0;
        isWallGrabbing = false;
    }

    private void AirJump()
    {
        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        isGrounded = false;
        groundCheckDisableTimer = jumpGroundCheckDisableTime;

        airJumpCount++;
        isWallGrabbing = false;

        Debug.Log("Air Jump: " + airJumpCount + "/" + maxAirJumps);
    }

    private void WallGrabJump()
    {
        int jumpDirection = -wallSide;

        if (jumpDirection == 0)
        {
            jumpDirection = facingRight ? -1 : 1;
        }

        rb.gravityScale = normalGravityScale;

        rb.linearVelocity = new Vector2(
            jumpDirection * wallGrabJumpXForce,
            jumpForce
        );

        isGrounded = false;
        isWallGrabbing = false;
        isWallJumping = true;

        groundCheckDisableTimer = jumpGroundCheckDisableTime;
        wallJumpTimer = wallJumpControlLockTime;
        wallGrabDisableTimer = wallGrabReEnableDelay;
        lastWallJumpTime = Time.time;

        airJumpCount = 0;

        if ((jumpDirection > 0 && !facingRight) || (jumpDirection < 0 && facingRight))
        {
            Flip();
        }

        Debug.Log("Wall Jump: " + jumpDirection);
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
        wallGrabDisableTimer = wallGrabReEnableDelay;

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
        SetAnimatorBoolIfExists("isDashing", isDashing);
        SetAnimatorBoolIfExists("isAttacking", isAttacking);
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

        if (isWallGrabbing)
            return;

        if (!isAttacking)
        {
            queuedComboClicks = 0;
            comboStep = 1;

            StopMove();
            EnableMovement(false);

            comboCoroutine = StartCoroutine(ComboRoutine());
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
        SetAnimatorBoolIfExists("isAttacking", true);

        EnableMovement(false);
        StopMove();

        while (true)
        {
            attackStartTime = Time.time;

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
    }

    private float GetAttackDuration(int step)
    {
        if (step == 1) return attack1Duration;
        if (step == 2) return attack2Duration;
        if (step == 3) return attack3Duration;

        return 0.6f;
    }

    private void EndCombo()
    {
        Debug.Log("End Combo");

        comboCoroutine = null;

        isAttacking = false;
        queuedComboClicks = 0;
        comboStep = 0;

        SetAnimatorBoolIfExists("isAttacking", false);

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
        UpdateUI();
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;

        if (currentMana < 0)
        {
            currentMana = 0;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (playerUI != null)
        {
            playerUI.UpdateHealthBar(currentHealth, maxHealth);
            playerUI.UpdateManaBar(currentMana, maxMana);
        }

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.UpdateHealthBar(currentHealth, maxHealth);
            PlayerUI.Instance.UpdateManaBar(currentMana, maxMana);
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

    public void SaveCurrentState()
    {
        SaveSystem.currentData.playerX = transform.position.x;
        SaveSystem.currentData.playerY = transform.position.y;
        SaveSystem.currentData.currentHealth = currentHealth;
        SaveSystem.currentData.currentMana = currentMana;
        SaveSystem.currentData.lastSavedScene = SceneManager.GetActiveScene().name;

        SaveSystem.SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentState();
    }

    protected override void Die()
    {
        if (isDead)
            return;

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

        SetAnimatorTriggerIfExists("die");

        if (col != null)
        {
            col.enabled = false;
        }

        StartCoroutine(PlayerDeathRoutine(1.5f));
    }

    private IEnumerator PlayerDeathRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowGameOverScreen();
        }

        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer s in allSprites)
        {
            if (s != null)
            {
                s.enabled = false;
            }
        }
    }

    public void syncBeforeLoad()
    {
        if (SaveSystem.LoadGame())
        {
            SaveSystem.currentData.currentHealth = currentHealth;
            SaveSystem.currentData.currentMana = currentMana;
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

        Vector2 rightBoxCenter = new Vector2(
            bounds.max.x,
            bounds.center.y
        );

        Vector2 leftBoxCenter = new Vector2(
            bounds.min.x,
            bounds.center.y
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(rightBoxCenter, boxSize);
        Gizmos.DrawWireCube(leftBoxCenter, boxSize);
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetCurrentMana() => currentMana;
    public int GetMaxHealth() => maxHealth;
    public int GetMaxMana() => maxMana;
    public int GetAttackDamage() => attackDamage;

    public void AddHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateUI();
    }

    public void AddMana(int amount)
    {
        maxMana += amount;
        currentMana += amount;

        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        UpdateUI();
    }

    public void AddAttackDamage(int amount)
    {
        attackDamage += amount;
    }

    public override void DamageTargets()
{
    // 1. Gọi lại logic gây sát thương mặc định của Entity
    base.DamageTargets();

    if (attackPoint != null)
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);
        
        if (targets.Length > 0)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.PlayerHit);
        }
    }
}
}