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

    [Header("Hitbox")]
    [SerializeField] private Vector2 hitboxScale = new Vector2(0.75f, 0.75f);
    [SerializeField] private Vector2 hitboxOffset = Vector2.zero;

    [Header("Render")]
    [SerializeField] private bool forceVisibleOrder = true;
    [SerializeField] private int visibleOrder = 999;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Entity owner;
    private BoxCollider2D boxCollider;
    private CircleCollider2D circleCollider;
    private CapsuleCollider2D capsuleCollider;
    private Vector2 originalBoxSize;
    private float originalCircleRadius;
    private Vector2 originalCapsuleSize;

    private readonly HashSet<Entity> damagedTargets = new HashSet<Entity>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;

        boxCollider = GetComponent<BoxCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        Collider2D col = boxCollider != null ? boxCollider : circleCollider != null ? (Collider2D)circleCollider : capsuleCollider;

        if (col != null)
        {
            col.isTrigger = true;
        }

        if (boxCollider != null)
        {
            originalBoxSize = boxCollider.size;
        }
        else if (circleCollider != null)
        {
            originalCircleRadius = circleCollider.radius;
        }
        else if (capsuleCollider != null)
        {
            originalCapsuleSize = capsuleCollider.size;
        }

        ApplyHitboxScale();

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

        ApplyHitboxScale();
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

    private void ApplyHitboxScale()
    {
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(originalBoxSize.x * hitboxScale.x, originalBoxSize.y * hitboxScale.y);
            boxCollider.offset = hitboxOffset;
        }
        else if (circleCollider != null)
        {
            circleCollider.radius = originalCircleRadius * Mathf.Max(hitboxScale.x, hitboxScale.y);
            circleCollider.offset = hitboxOffset;
        }
        else if (capsuleCollider != null)
        {
            capsuleCollider.size = new Vector2(originalCapsuleSize.x * hitboxScale.x, originalCapsuleSize.y * hitboxScale.y);
            capsuleCollider.offset = hitboxOffset;
        }
    }
}