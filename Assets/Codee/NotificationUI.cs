using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    public static NotificationUI Instance;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayDuration = 1.5f;

    private Coroutine showCoroutine;

    private void Awake()
    {
        Instance = this;

        if (messageText != null)
            messageText.gameObject.SetActive(false);
        DontDestroyOnLoad(gameObject);
    }

    public void ShowMessage(string message)
    {
        if (messageText == null) return;

        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        showCoroutine = StartCoroutine(ShowMessageCoroutine(message));
    }

    private IEnumerator ShowMessageCoroutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        messageText.gameObject.SetActive(false);
        showCoroutine = null;
    }
}