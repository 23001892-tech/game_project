using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class BossShockwave : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int direction = 1;

    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask targetLayer;

    [Header("Expand Visual")]
    [SerializeField] private bool expandWhileMoving = true;
    [SerializeField] private float expandSpeedX = 1.5f;
    [SerializeField] private float maxScaleX = 3f;

    [Header("Render")]
    [SerializeField] private bool forceVisibleOrder = true;
    [SerializeField] private int visibleOrder = 999;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Entity owner;

    private readonly HashSet<Entity> damagedTargets = new HashSet<Entity>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.isTrigger = true;
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        if (spriteRenderer != null && forceVisibleOrder)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = visibleOrder;
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Init(
        int newDirection,
        int newDamage,
        float newSpeed,
        float newLifeTime,
        LayerMask newTargetLayer,
        Entity newOwner
    )
    {
        direction = newDirection;
        damage = newDamage;
        speed = newSpeed;
        lifeTime = newLifeTime;
        targetLayer = newTargetLayer;
        owner = newOwner;

        Vector3 scale = originalScale;
        scale.x = Mathf.Abs(originalScale.x) * direction;
        transform.localScale = scale;

        if (spriteRenderer != null && forceVisibleOrder)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = visibleOrder;
        }
    }

    private void Update()
    {
        if (!expandWhileMoving)
            return;

        Vector3 scale = transform.localScale;

        float absX = Mathf.Abs(scale.x);
        absX += expandSpeedX * Time.deltaTime;
        absX = Mathf.Min(absX, maxScaleX);

        scale.x = absX * direction;
        transform.localScale = scale;
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(direction * speed, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) == 0)
            return;

        Entity target = other.GetComponentInParent<Entity>();

        if (target == null)
            return;

        if (target == owner)
            return;

        if (damagedTargets.Contains(target))
            return;

        damagedTargets.Add(target);

        target.TakeDamage(damage);
    }
}