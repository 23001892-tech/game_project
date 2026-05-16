using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Info")]
    public string enemyName;
    public float moveSpeed = 1.5f;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 3;
    protected int currentHealth;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    public virtual void TakeDamage()
    {
        currentHealth--;

        Debug.Log(enemyName + " took damage. HP: " + currentHealth);

        if (sr != null)
        {
            sr.color = Color.red;
            Invoke(nameof(ResetColor), 0.15f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log(enemyName + " died.");
        Destroy(gameObject);
    }

    private void ResetColor()
    {
        if (sr != null)
        {
            sr.color = Color.white;
        }
    }
}