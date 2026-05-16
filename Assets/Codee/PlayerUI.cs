using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Transform healthBarFill;
    [SerializeField] private Transform manaBarFill;

    public void UpdateHealthBar(float current, float max)
    {
        float value = current / max;

        Vector3 scale = healthBarFill.localScale;
        scale.x = value;
        healthBarFill.localScale = scale;
    }

    public void UpdateManaBar(float current, float max)
    {
        float value = current / max;

        Vector3 scale = manaBarFill.localScale;
        scale.x = value;
        manaBarFill.localScale = scale;
    }
}