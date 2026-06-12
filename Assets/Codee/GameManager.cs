using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Scene Config")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RestartGame()
    {

        string currentSceneName = SceneManager.GetActiveScene().name;
        
        if (LoadingProgress.Instance != null)
        {
            LoadingProgress.Instance.LoadScene(currentSceneName);
        }
        else
        {
            SceneManager.LoadScene(currentSceneName);
        }
    }

    // Gắn hàm này vào Nút MAIN MENU trên UI
    public void ExitToMainMenu()
    {
        if (LoadingProgress.Instance != null)
        {
            LoadingProgress.Instance.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}