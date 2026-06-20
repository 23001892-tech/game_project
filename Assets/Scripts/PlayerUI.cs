using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; private set; }

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;

    private void Awake()
    {
        Instance = this;
    }

    private void InitSlider(Slider slider, float max)
    {
        if (slider == null) return;
        slider.minValue = 0;
        slider.maxValue = max;
        slider.value = max; // Bắt đầu full
    }

    public void InitBars(float maxHealth, float maxMana)
    {
        InitSlider(healthSlider, maxHealth);
        InitSlider(manaSlider, maxMana);
    }

    public void UpdateHealthBar(float current, float max)
{
    if (healthSlider == null) return;

    healthSlider.maxValue = max;
    healthSlider.value = current; 

    // Tự động ẩn cái chấm trắng (thanh Fill) khi máu bằng 0
    if (healthSlider.fillRect != null)
    {
        healthSlider.fillRect.gameObject.SetActive(current > 0);
    }
}

public void UpdateManaBar(float current, float max)
{
    if (manaSlider == null) return;

    manaSlider.maxValue = max;
    manaSlider.value = current;

    // Tự động ẩn thanh Fill của Mana khi mana bằng 0
    if (manaSlider.fillRect != null)
    {
        manaSlider.fillRect.gameObject.SetActive(current > 0);
    }
}
}