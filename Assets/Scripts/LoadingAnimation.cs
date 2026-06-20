using UnityEngine;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour
{
    [SerializeField] private Image crystalFillImage;
    [SerializeField] private float fillSpeed = 0.5f; // tốc độ mượt

    private float targetProgress = 0f;
    private float currentProgress = 0f;

    public void UpdateProgress(float value)
    {
        targetProgress = Mathf.Clamp01(value);
    }

    private void Update()
    {
        currentProgress = Mathf.MoveTowards(
            currentProgress,
            targetProgress,
            fillSpeed * Time.deltaTime
        );

        if (crystalFillImage != null)
        {
            crystalFillImage.fillAmount = currentProgress;
        }
    }
    public float GetCurrentProgress()
    {
        return currentProgress;
    }
}   