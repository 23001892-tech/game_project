using UnityEngine;


public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private GameObject settingsPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Open()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void Close()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}