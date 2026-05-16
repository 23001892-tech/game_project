using UnityEngine;

public class EnemyLumeniteCrawler : Enemy
{
    [Header("Crawler Settings")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float leapForce = 6f;
    [SerializeField] private float attackCooldown = 1.2f;

    private Transform player;
    private float attackTimer;
    private bool facingRight = true;

    protected override void Awake()
    {
        base.Awake();

        enemyName = "Lumenite Crawler";
        moveSpeed = 3.5f;
        maxHealth = 2;
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    
    }

    private void Update()
    {
        if (player == null)
            return;

        attackTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectRange)
        {
            ChasePlayer();
        }

        if (distanceToPlayer <= attackRange && attackTimer <= 0)
        {
            LeapAttack();
        }
    }

    private void ChasePlayer()
    {
        float direction = player.position.x - transform.position.x;

        if (direction > 0)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

            if (!facingRight)
                Flip();
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);

            if (facingRight)
                Flip();
        }
    }

    private void LeapAttack()
    {
        attackTimer = attackCooldown;

        float direction = player.position.x > transform.position.x ? 1 : -1;

        rb.linearVelocity = new Vector2(direction * leapForce, leapForce * 0.6f);

        Debug.Log(enemyName + " leap attacks!");
    }

    public override void TakeDamage()
    {
        base.TakeDamage();

        // Bị đánh thì hơi lùi lại
        if (player != null && rb != null)
        {
            float knockbackDirection = transform.position.x > player.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(knockbackDirection * 3f, 2f);
        }
    }

    protected override void Die()
    {
        Debug.Log(enemyName + " shattered into Lumenite fragments.");
        Destroy(gameObject);
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}