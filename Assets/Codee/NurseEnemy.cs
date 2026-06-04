using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NurseEnemy : Enemy
{
    [Header("Normal Stab")]
    [SerializeField] private int normalAttackDamage = 8;
    [SerializeField] private float normalAttackCooldown = 0.8f;
    [SerializeField] private float normalAttackRadius = 0.55f;
    [SerializeField] private float normalAttackRange = 1f;
    [SerializeField] private float normalAttackLockTime = 0.8f;

    [Header("Skill - Resonance Injection")]
    [SerializeField] private int skillDamage = 15;
    [SerializeField] private float skillRadius = 0.75f;

    [SerializeField] private float skillMinRange = 2f;
    [SerializeField] private float skillMaxRange = 5f;
    [SerializeField] private float skillCooldown = 6f;
    [SerializeField] private float skillLockTime = 1.2f;

    [Header("Skill Condition")]
    [SerializeField] private bool useHpCondition = false;
    [SerializeField] private float skillHpPercent = 0.7f;

    [Header("Skill Dash")]
    [SerializeField] private float skillDashSpeed = 8f;
    [SerializeField] private float skillDashTime = 0.18f;

    [Header("Crystal Infection Damage Over Time")]
    [SerializeField] private bool applyInfection = true;
    [SerializeField] private int infectionDamagePerTick = 2;
    [SerializeField] private int infectionTicks = 3;
    [SerializeField] private float infectionTickDelay = 1f;

    [Header("Animator Params")]
    [SerializeField] private string xVelocityParam = "xVelocity";
    [SerializeField] private string yVelocityParam = "yVelocity";
    [SerializeField] private string isGroundedParam = "isGrounded";
    [SerializeField] private string isMovingParam = "isMoving";

    [SerializeField] private string attackTriggerParam = "attack";
    [SerializeField] private string skillTriggerParam = "skill";

    private float lastNormalAttackTime = -999f;
    private float lastSkillTime = -999f;

    private bool isUsingSkill;
    private Coroutine attackStateResetCoroutine;
    private Coroutine skillDashCoroutine;

    private HashSet<string> animatorParams = new HashSet<string>();

    protected override void Awake()
    {
        base.Awake();
        CacheAnimatorParams();
    }

    protected override void Update()
    {
        if (isDead)
            return;

        if (player == null)
        {
            FindPlayer();
            StopNurseMove();
            UpdateNurseAnimator();
            return;
        }

        // Cực quan trọng: cập nhật ground check như Entity/Enemy gốc
        HandleCollision();

        if (isHurt)
        {
            StopNurseMove();
            UpdateNurseAnimator();
            return;
        }

        // Khi đang skill thì không StopMove liên tục,
        // nếu không cú lao tới sẽ bị chặn.
        if (isUsingSkill)
        {
            UpdateNurseAnimator();
            return;
        }

        if (isAttacking)
        {
            StopNurseMove();
            UpdateNurseAnimator();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        FaceTarget(player);

        if (CanUseSkill(distance))
        {
            StartSkill();
            UpdateNurseAnimator();
            return;
        }

        if (distance <= normalAttackRange)
        {
            TryNormalAttack();
            UpdateNurseAnimator();
            return;
        }

        if (distance <= detectRange)
        {
            ChasePlayer();
            UpdateNurseAnimator();
            return;
        }

        StopNurseMove();
        UpdateNurseAnimator();
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
            anim.SetBool(paramName, value);
    }

    private void SetFloatSafe(string paramName, float value)
    {
        if (HasAnimParam(paramName))
            anim.SetFloat(paramName, value);
    }

    private void SetTriggerSafe(string paramName)
    {
        if (HasAnimParam(paramName))
        {
            anim.SetTrigger(paramName);
        }
        else
        {
            Debug.LogWarning(gameObject.name + " thiếu Animator Trigger: " + paramName);
        }
    }

    private void ResetTriggerSafe(string paramName)
    {
        if (HasAnimParam(paramName))
            anim.ResetTrigger(paramName);
    }

    private void UpdateNurseAnimator()
    {
        if (anim == null || rb == null)
            return;

        float xVel = rb.linearVelocity.x;
        float yVel = rb.linearVelocity.y;

        // Dùng cho Animator giống Enemy/Entity gốc
        SetFloatSafe(xVelocityParam, xVel);
        SetFloatSafe(yVelocityParam, yVel);
        SetBoolSafe(isGroundedParam, isGrounded);

        // Dùng thêm nếu Animator của Nurse nối bằng isMoving
        SetBoolSafe(isMovingParam, Mathf.Abs(xVel) > 0.05f);
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void ChasePlayer()
    {
        FaceTarget(player);

        // Dùng MoveX của Entity để giống quái thường
        MoveX(facDir);
    }

    private void StopNurseMove()
    {
        StopMove();
    }

    private void TryNormalAttack()
    {
        if (Time.time < lastNormalAttackTime + normalAttackCooldown)
            return;

        isAttacking = true;
        isUsingSkill = false;

        lastNormalAttackTime = Time.time;

        StopNurseMove();

        ResetTriggerSafe(skillTriggerParam);
        SetTriggerSafe(attackTriggerParam);

        StartAttackResetCoroutine(normalAttackLockTime);
    }

    private bool CanUseSkill(float distance)
    {
        bool inSkillRange = distance >= skillMinRange && distance <= skillMaxRange;
        bool cooldownReady = Time.time >= lastSkillTime + skillCooldown;

        bool hpCondition = true;

        if (useHpCondition)
        {
            hpCondition = currentHealth <= maxHealth * skillHpPercent;
        }

        return inSkillRange && cooldownReady && hpCondition;
    }

    private void StartSkill()
    {
        isUsingSkill = true;
        isAttacking = true;

        lastSkillTime = Time.time;

        StopNurseMove();

        ResetTriggerSafe(attackTriggerParam);
        SetTriggerSafe(skillTriggerParam);

        StartAttackResetCoroutine(skillLockTime);
    }

    public void NurseSkillLunge()
    {
        if (player == null)
            return;

        if (skillDashCoroutine != null)
        {
            StopCoroutine(skillDashCoroutine);
        }

        skillDashCoroutine = StartCoroutine(SkillDashCoroutine());
    }

    private IEnumerator SkillDashCoroutine()
    {
        float timer = 0f;

        FaceTarget(player);

        float direction = facDir;

        while (timer < skillDashTime)
        {
            rb.linearVelocity = new Vector2(direction * skillDashSpeed, rb.linearVelocity.y);

            UpdateNurseAnimator();

            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        UpdateNurseAnimator();

        skillDashCoroutine = null;
    }

    public void NurseNormalAttackDamage()
    {
        DamagePlayer(normalAttackDamage, normalAttackRadius, false);
    }

    public void NurseSkillDamage()
    {
        DamagePlayer(skillDamage, skillRadius, applyInfection);
    }

    private void DamagePlayer(int damage, float radius, bool infection)
    {
        if (attackPoint == null)
        {
            Debug.LogWarning(gameObject.name + " chưa gắn AttackPoint cho NurseEnemy.");
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(
            attackPoint.position,
            radius,
            whatIsTarget
        );

        if (hit == null)
        {
            Debug.Log(gameObject.name + " đánh trượt.");
            return;
        }

        Entity target = hit.GetComponentInParent<Entity>();

        if (target != null && target != this)
        {
            target.TakeDamage(damage);

            Debug.Log(gameObject.name + " đánh trúng Player, damage: " + damage);

            if (infection)
            {
                StartCoroutine(ApplyCrystalInfection(target));
            }
        }
    }

    private IEnumerator ApplyCrystalInfection(Entity target)
    {
        for (int i = 0; i < infectionTicks; i++)
        {
            yield return new WaitForSeconds(infectionTickDelay);

            if (target == null)
                yield break;

            target.TakeDamage(infectionDamagePerTick);
        }
    }

    public void EndNurseAttack()
    {
        isAttacking = false;
        isUsingSkill = false;

        StopAttackResetCoroutine();
        StopNurseMove();
        UpdateNurseAnimator();
    }

    public void EndNurseSkill()
    {
        isAttacking = false;
        isUsingSkill = false;

        StopAttackResetCoroutine();
        StopNurseMove();
        UpdateNurseAnimator();
    }

    private void StartAttackResetCoroutine(float duration)
    {
        StopAttackResetCoroutine();
        attackStateResetCoroutine = StartCoroutine(ResetAttackState(duration));
    }

    private void StopAttackResetCoroutine()
    {
        if (attackStateResetCoroutine != null)
        {
            StopCoroutine(attackStateResetCoroutine);
            attackStateResetCoroutine = null;
        }
    }

    private IEnumerator ResetAttackState(float duration)
    {
        yield return new WaitForSeconds(duration);

        isAttacking = false;
        isUsingSkill = false;
        attackStateResetCoroutine = null;

        UpdateNurseAnimator();
    }

    public override void Animation_FinishAttack()
    {
        isAttacking = false;
        isUsingSkill = false;

        StopAttackResetCoroutine();
        StopNurseMove();
        UpdateNurseAnimator();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, normalAttackRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackPoint.position, skillRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, skillMinRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, skillMaxRange);
    }
}