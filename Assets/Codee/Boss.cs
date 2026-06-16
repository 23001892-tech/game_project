using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Entity
{
    private enum BossState
    {
        Idle,
        Chase,
        Attack,
        Skill,
        Dead
    }

    [Header("Boss AI")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float bossMoveSpeed = 2.5f;

    [Header("Facing")]
    [Tooltip("Bật nếu sprite boss gốc nhìn sang phải. Tắt nếu sprite boss gốc nhìn sang trái.")]
    [SerializeField] private bool spriteFacesRightByDefault = true;

    [Header("Normal Attack")]
    [SerializeField] private float normalAttackCooldown = 1.5f;
    [SerializeField] private float normalAttackLockTime = 0.9f;
    [SerializeField] private float normalAttackDamageDelay = 0.45f;
    [SerializeField] private bool useAnimationEventForNormalDamage = false;

    [Header("Jump Slam Skill")]
    [SerializeField] private bool useJumpSlamSkill = true;
    [SerializeField] private float skillMinRange = 3f;
    [SerializeField] private float skillMaxRange = 7f;
    [SerializeField] private float skillCooldown = 5f;

    [SerializeField] private float skillChargeTime = 0.45f;
    [SerializeField] private float skillJumpForce = 8.85f;
    [SerializeField] private float skillJumpXSpeed = 2f;
    [SerializeField] private float skillSlamFallSpeed = 24f;
    [SerializeField] private float skillGravityScale = 5f;
    [SerializeField] private float skillMinAirTime = 0.25f;
    [SerializeField] private float skillMaxAirTime = 2.5f;

    [Tooltip("Thời gian cho animation Impact chạy xong. Hết thời gian này mới tạo 1 sóng.")]
    [SerializeField] private float skillImpactLockTime = 0.7f;

    [Header("Skill Impact Damage")]
    [SerializeField] private Transform impactPoint;
    [SerializeField] private float impactRadius = 1.8f;
    [SerializeField] private int impactDamage = 20;

    [Header("Shockwave")]
    [SerializeField] private GameObject shockwavePrefab;
    [SerializeField] private Transform shockwaveSpawnPoint;
    [SerializeField] private LayerMask shockwaveTargetLayer;
    [SerializeField] private float shockwaveSpeed = 8f;
    [SerializeField] private float shockwaveLifeTime = 2f;
    [SerializeField] private int shockwaveDamage = 10;
    [SerializeField] private bool spawnShockwaveBothSides = true;
    [SerializeField] private bool spawnShockwaveAfterImpactEnd = true;

    [Header("Animator Params")]
    [SerializeField] private string xVelocityParam = "xVelocity";
    [SerializeField] private string yVelocityParam = "yVelocity";
    [SerializeField] private string isGroundedParam = "isGrounded";
    [SerializeField] private string isMovingParam = "isMoving";
    [SerializeField] private string isAttackingParam = "isAttacking";
    [SerializeField] private string isUsingSkillParam = "isUsingSkill";

    [SerializeField] private string attackTriggerParam = "attack";
    [SerializeField] private string skillJumpStartTriggerParam = "skillJumpStart";
    [SerializeField] private string skillFallTriggerParam = "skillFall";
    [SerializeField] private string skillImpactTriggerParam = "skillImpact";

    [Header("Skill Animation State Names")]
    [SerializeField] private string skillStartStateName = "bossSkillStart1";
    [SerializeField] private string skillFallStateName = "bossSkillFall1";
    [SerializeField] private string skillImpactStateName = "bossSkillImpact1";
    [SerializeField] private float skillAnimTransitionTime = 0.03f;

    [Header("Debug Gizmos")]
    [SerializeField] private bool showDebugRanges = true;

    private BossState currentState = BossState.Idle;

    private bool isAttacking;
    private bool isUsingSkill;

    private float lastNormalAttackTime = -999f;
    private float lastSkillTime = -999f;

    private Coroutine normalAttackResetCoroutine;
    private Coroutine normalAttackDamageCoroutine;
    private Coroutine skillCoroutine;

    private HashSet<string> animatorParams = new HashSet<string>();

    private Vector3 originalScale;
    private float originalGravityScale;
    private float attackPointStartAbsX;
    private int facingDirection = 1;

    protected override void Awake()
    {
        base.Awake();

        originalScale = transform.localScale;

        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
            rb.freezeRotation = true;
        }

        if (attackPoint != null)
        {
            attackPointStartAbsX = Mathf.Abs(attackPoint.localPosition.x);
        }

        CacheAnimatorParams();

        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        FindPlayer();
    }

    protected override void Update()
    {
        if (isDead)
            return;

        FindPlayer();
        HandleCollision();

        if (player == null)
        {
            ChangeState(BossState.Idle);
            StopBossMove();
            UpdateBossAnimator();
            return;
        }

        if (isUsingSkill)
        {
            UpdateBossAnimator();
            return;
        }

        if (isAttacking)
        {
            StopBossMove();
            FacePlayer();
            UpdateBossAnimator();
            return;
        }

        float distanceFull = Vector2.Distance(transform.position, player.position);
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);

        if (distanceFull > detectRange)
        {
            ChangeState(BossState.Idle);
            StopBossMove();
            UpdateBossAnimator();
            return;
        }

        FacePlayer();

        if (CanUseJumpSlamSkill(distanceX))
        {
            StartJumpSlamSkill();
            UpdateBossAnimator();
            return;
        }

        if (distanceX <= attackRange)
        {
            ChangeState(BossState.Attack);
            StopBossMove();
            TryNormalAttack();
            UpdateBossAnimator();
            return;
        }

        ChangeState(BossState.Chase);
        ChasePlayer();
        UpdateBossAnimator();
    }

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
            return;
        }

        Player playerScript = FindFirstObjectByType<Player>();

        if (playerScript != null)
        {
            player = playerScript.transform;
        }
    }

    private void CacheAnimatorParams()
    {
        animatorParams.Clear();

        if (anim == null)
            return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            animatorParams.Add(param.name);
        }
    }

    private bool HasAnimParam(string paramName)
    {
        return anim != null && animatorParams.Contains(paramName);
    }

    private void SetBoolSafe(string paramName, bool value)
    {
        if (HasAnimParam(paramName))
        {
            anim.SetBool(paramName, value);
        }
    }

    private void SetFloatSafe(string paramName, float value)
    {
        if (HasAnimParam(paramName))
        {
            anim.SetFloat(paramName, value);
        }
    }

    private void SetTriggerSafe(string paramName)
    {
        if (HasAnimParam(paramName))
        {
            anim.SetTrigger(paramName);
        }
    }

    private void ResetTriggerSafe(string paramName)
    {
        if (HasAnimParam(paramName))
        {
            anim.ResetTrigger(paramName);
        }
    }

    private void PlaySkillState(string stateName)
    {
        if (anim == null)
            return;

        if (string.IsNullOrEmpty(stateName))
            return;

        ResetTriggerSafe(attackTriggerParam);
        ResetTriggerSafe(skillJumpStartTriggerParam);
        ResetTriggerSafe(skillFallTriggerParam);
        ResetTriggerSafe(skillImpactTriggerParam);

        anim.CrossFadeInFixedTime(stateName, skillAnimTransitionTime, 0, 0f);
    }

    private void UpdateBossAnimator()
    {
        if (anim == null || rb == null)
            return;

        float xVel = rb.linearVelocity.x;
        float yVel = rb.linearVelocity.y;

        SetFloatSafe(xVelocityParam, xVel);
        SetFloatSafe(yVelocityParam, yVel);

        SetBoolSafe(isGroundedParam, isGrounded);
        SetBoolSafe(isMovingParam, Mathf.Abs(xVel) > 0.05f && !isAttacking && !isUsingSkill);
        SetBoolSafe(isAttackingParam, isAttacking);
        SetBoolSafe(isUsingSkillParam, isUsingSkill);
    }

    private int GetDirectionToPlayer()
    {
        if (player == null)
            return facingDirection;

        float xDelta = player.position.x - transform.position.x;

        if (Mathf.Abs(xDelta) < 0.05f)
            return facingDirection;

        return xDelta > 0f ? 1 : -1;
    }

    private void FacePlayer()
    {
        FaceDirection(GetDirectionToPlayer());
    }

    private void FaceDirection(int direction)
    {
        if (direction == 0)
            return;

        facingDirection = direction;

        Vector3 newScale = originalScale;

        if (spriteFacesRightByDefault)
        {
            newScale.x = Mathf.Abs(originalScale.x) * direction;
        }
        else
        {
            newScale.x = Mathf.Abs(originalScale.x) * -direction;
        }

        transform.localScale = newScale;

        if (attackPoint != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = attackPointStartAbsX * direction;
            attackPoint.localPosition = localPos;
        }

        facDir = direction;
        facingRight = direction > 0;
    }

    private void ChasePlayer()
    {
        if (rb == null || player == null)
            return;

        int direction = GetDirectionToPlayer();
        FaceDirection(direction);

        rb.linearVelocity = new Vector2(direction * bossMoveSpeed, rb.linearVelocity.y);
    }

    private void StopBossMove()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void TryNormalAttack()
    {
        if (isAttacking)
            return;

        if (Time.time < lastNormalAttackTime + normalAttackCooldown)
            return;

        isAttacking = true;
        isUsingSkill = false;

        lastNormalAttackTime = Time.time;

        StopBossMove();
        FacePlayer();

        ResetTriggerSafe(skillJumpStartTriggerParam);
        ResetTriggerSafe(skillFallTriggerParam);
        ResetTriggerSafe(skillImpactTriggerParam);

        ResetTriggerSafe(attackTriggerParam);
        SetTriggerSafe(attackTriggerParam);

        if (!useAnimationEventForNormalDamage)
        {
            StartNormalAttackDamageCoroutine();
        }

        StartNormalAttackResetCoroutine(normalAttackLockTime);
    }

    private void StartNormalAttackDamageCoroutine()
    {
        if (normalAttackDamageCoroutine != null)
        {
            StopCoroutine(normalAttackDamageCoroutine);
        }

        normalAttackDamageCoroutine = StartCoroutine(NormalAttackDamageDelayRoutine());
    }

    private IEnumerator NormalAttackDamageDelayRoutine()
    {
        yield return new WaitForSeconds(normalAttackDamageDelay);

        if (isAttacking && !isUsingSkill)
        {
            DamageTargets();
        }

        normalAttackDamageCoroutine = null;
    }

    public void BossNormalAttackDamage()
    {
        DamageTargets();
    }

    public void Animation_BossDamageTarget()
    {
        DamageTargets();
    }

    public void Animation_BossFinishAttack()
    {
        EndBossAttack();
    }

    public override void Animation_FinishAttack()
    {
        EndBossAttack();
    }

    private void EndBossAttack()
    {
        isAttacking = false;
        isUsingSkill = false;

        StopNormalAttackResetCoroutine();

        if (normalAttackDamageCoroutine != null)
        {
            StopCoroutine(normalAttackDamageCoroutine);
            normalAttackDamageCoroutine = null;
        }

        StopBossMove();
        UpdateBossAnimator();
    }

    private void StartNormalAttackResetCoroutine(float duration)
    {
        StopNormalAttackResetCoroutine();
        normalAttackResetCoroutine = StartCoroutine(ResetNormalAttackState(duration));
    }

    private void StopNormalAttackResetCoroutine()
    {
        if (normalAttackResetCoroutine != null)
        {
            StopCoroutine(normalAttackResetCoroutine);
            normalAttackResetCoroutine = null;
        }
    }

    private IEnumerator ResetNormalAttackState(float duration)
    {
        yield return new WaitForSeconds(duration);

        isAttacking = false;
        isUsingSkill = false;
        normalAttackResetCoroutine = null;

        UpdateBossAnimator();
    }

    private bool CanUseJumpSlamSkill(float distanceX)
    {
        if (!useJumpSlamSkill)
            return false;

        if (Time.time < lastSkillTime + skillCooldown)
            return false;

        return distanceX >= skillMinRange && distanceX <= skillMaxRange;
    }

    private void StartJumpSlamSkill()
    {
        if (skillCoroutine != null)
            return;

        ChangeState(BossState.Skill);

        isUsingSkill = true;
        isAttacking = true;

        lastSkillTime = Time.time;

        StopBossMove();
        FacePlayer();

        PlaySkillState(skillStartStateName);

        skillCoroutine = StartCoroutine(JumpSlamRoutine());
    }

    private IEnumerator JumpSlamRoutine()
    {
        if (rb == null)
        {
            EndJumpSlamSkill();
            yield break;
        }

        yield return new WaitForSeconds(skillChargeTime);

        int direction = GetDirectionToPlayer();
        FaceDirection(direction);

        rb.gravityScale = originalGravityScale;

        rb.linearVelocity = new Vector2(
            direction * skillJumpXSpeed,
            skillJumpForce
        );

        float airTimer = 0f;

        while (rb.linearVelocity.y > 0f && airTimer < skillMaxAirTime)
        {
            airTimer += Time.deltaTime;
            UpdateBossAnimator();
            yield return null;
        }

        PlaySkillState(skillFallStateName);

        rb.gravityScale = skillGravityScale;

        airTimer = 0f;

        while (!isGrounded && airTimer < skillMaxAirTime)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -skillSlamFallSpeed
            );

            airTimer += Time.deltaTime;
            UpdateBossAnimator();
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravityScale;

        PlaySkillState(skillImpactStateName);

        DamageImpactArea();

        yield return new WaitForSeconds(skillImpactLockTime);

        if (spawnShockwaveAfterImpactEnd)
        {
            SpawnShockwave();
        }

        EndJumpSlamSkill();
    }

    private void DamageImpactArea()
    {
        Vector3 center = transform.position;

        if (impactPoint != null)
        {
            center = impactPoint.position;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            center,
            impactRadius,
            whatIsTarget
        );

        foreach (Collider2D hit in hits)
        {
            Entity target = hit.GetComponentInParent<Entity>();

            if (target != null && target != this)
            {
                target.TakeDamage(impactDamage);
            }
        }
    }

    public void Animation_BossImpactShockwave()
    {
        // Không dùng Animation Event để tạo sóng nữa.
        // Giữ hàm này để nếu clip còn event cũ thì không bị lỗi.
    }

    private void SpawnShockwave()
    {
        if (shockwavePrefab == null)
        {
            Debug.LogWarning(gameObject.name + " chưa gắn Shockwave Prefab");
            return;
        }

        Vector3 spawnPos = transform.position;

        if (shockwaveSpawnPoint != null)
        {
            spawnPos = shockwaveSpawnPoint.position;
        }

        spawnPos.z = 0f;

        if (spawnShockwaveBothSides)
        {
            SpawnOneShockwave(spawnPos, -1);
            SpawnOneShockwave(spawnPos, 1);
        }
        else
        {
            SpawnOneShockwave(spawnPos, GetDirectionToPlayer());
        }
    }

    private void SpawnOneShockwave(Vector3 spawnPos, int direction)
    {
        GameObject shockwave = Instantiate(
            shockwavePrefab,
            spawnPos,
            Quaternion.identity
        );

        shockwave.name = "BossShockwave_" + direction;
        shockwave.SetActive(true);

        Vector3 scale = shockwave.transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        shockwave.transform.localScale = scale;

        SpriteRenderer sr = shockwave.GetComponentInChildren<SpriteRenderer>();

        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingOrder = 999;
        }

        BossShockwave shockwaveScript = shockwave.GetComponent<BossShockwave>();

        if (shockwaveScript == null)
        {
            shockwaveScript = shockwave.GetComponentInChildren<BossShockwave>();
        }

        if (shockwaveScript != null)
        {
            shockwaveScript.Init(
                direction,
                shockwaveDamage,
                shockwaveSpeed,
                shockwaveLifeTime,
                shockwaveTargetLayer,
                this
            );
        }
        else
        {
            Rigidbody2D shockwaveRb = shockwave.GetComponent<Rigidbody2D>();

            if (shockwaveRb != null)
            {
                shockwaveRb.linearVelocity = new Vector2(direction * shockwaveSpeed, 0f);
            }

            Destroy(shockwave, shockwaveLifeTime);
        }
    }

    public void EndJumpSlamSkill()
    {
        isUsingSkill = false;
        isAttacking = false;
        skillCoroutine = null;

        ChangeState(BossState.Idle);

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
        }

        StopBossMove();
        UpdateBossAnimator();
    }

    private void ChangeState(BossState newState)
    {
        currentState = newState;
    }

    protected override void HandleAnimations()
    {
        UpdateBossAnimator();
    }

    protected override void HandleFlip()
    {
    }

    protected override void Die()
    {
        if (isDead)
            return;

        currentState = BossState.Dead;
        isAttacking = false;
        isUsingSkill = false;

        StopNormalAttackResetCoroutine();

        if (normalAttackDamageCoroutine != null)
        {
            StopCoroutine(normalAttackDamageCoroutine);
            normalAttackDamageCoroutine = null;
        }

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        if (rb != null)
        {
            rb.gravityScale = originalGravityScale;
            rb.linearVelocity = Vector2.zero;
        }

        base.Die();
    }

    protected override void OnDrawGizmos()
    {
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRanges)
            return;

        Vector3 center = transform.position;

        Collider2D bossCollider = GetComponent<Collider2D>();

        if (bossCollider == null)
        {
            bossCollider = GetComponentInChildren<Collider2D>();
        }

        if (bossCollider != null)
        {
            center = bossCollider.bounds.center;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        if (impactPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(impactPoint.position, impactRadius);
        }

        if (shockwaveSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(shockwaveSpawnPoint.position, 0.15f);
        }
    }
}