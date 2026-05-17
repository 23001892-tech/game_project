using UnityEngine;

public class Enemy : Entity
{
    [Header("Enemy AI")]
    [SerializeField] private float detectRange = 5f;
    [SerializeField] private float attackCooldown = 1.5f;

    private Transform player;
    private bool playerDetected;
    private bool playerInAttackRange;
    private float attackTimer;

    protected override void Awake()
    {
        base.Awake();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Player. Hãy đặt Tag của Player là Player.");
        }
    }

    protected override void Update()
    {
        HandleCollision();
        HandleInput();
        HandleMovement();
        HandleAnimations();
        HandleFlip();
        HandleAttack();
    }

    protected override void HandleInput()
    {
        // Enemy không dùng input.
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();

        if (player == null)
        {
            playerDetected = false;
            playerInAttackRange = false;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        playerDetected = distanceToPlayer <= detectRange;

        if (attackPoint != null)
        {
            playerInAttackRange = Physics2D.OverlapCircle(
                attackPoint.position,
                attackRadius,
                whatIsTarget
            ) != null;
        }
        else
        {
            playerInAttackRange = false;
        }
    }

    protected override void HandleMovement()
    {
        if (player == null)
        {
            StopMove();
            return;
        }

        if (!canMove)
        {
            StopMove();
            return;
        }

        if (playerDetected && !playerInAttackRange)
        {
            FaceTarget(player);

            MoveX(facDir);
        }
        else
        {
            StopMove();
        }
    }

    protected override void HandleAttack()
    {
        attackTimer -= Time.deltaTime;

        if (!playerInAttackRange)
            return;

        if (attackTimer > 0)
            return;

        attackTimer = attackCooldown;

        FaceTarget(player);
        StopMove();

        if (anim != null)
        {
            anim.SetTrigger("attack");
        }

        Debug.Log(gameObject.name + " attack!");
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}