using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BossDashTrail : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.25f;
    [SerializeField] private float fadeSpeed = 6f;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (sr != null)
        {
            Color color = sr.color;
            color.a -= fadeSpeed * Time.deltaTime;
            sr.color = color;
        }

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetSprite(Sprite sprite, bool flipX)
    {
        if (sr == null)
            return;

        sr.sprite = sprite;
        sr.flipX = flipX;

        Color color = sr.color;
        color.a = 0.55f;
        sr.color = color;
    }
}