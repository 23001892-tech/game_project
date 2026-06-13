using UnityEngine;

public class PlayerUI : MonoBehaviour
{

    public static PlayerUI Instance { get; private set; }
    [SerializeField] private Transform healthBarFill;
    [SerializeField] private Transform manaBarFill;

    private void Awake()
    {
        Instance = this;
    }
    public void UpdateHealthBar(float current, float max)
    {
        if (healthBarFill == null || max <= 0) return;

        float value = current / max;
        Vector3 scale = healthBarFill.localScale;
        scale.x = value;
        healthBarFill.localScale = scale;
    }

    public void UpdateManaBar(float current, float max)
    {
        if (manaBarFill == null || max <= 0) return;

        float value = current / max;
        Vector3 scale = manaBarFill.localScale;
        scale.x = value;
        manaBarFill.localScale = scale;
    }
}