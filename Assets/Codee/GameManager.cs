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
        SaveSystem.LoadGame(); // Tải lại dữ liệu từ JSON để đảm bảo trạng thái chuẩn được nạp lên RAM trước khi hồi sinh
        SaveSystem.currentData.currentHealth = 100; // Đặt lại HP về 100 khi hồi sinh
        SaveSystem.currentData.currentMana = 50; // Đặt lại Mana về 50 khi hồi sinh


        string currentSceneName = SceneManager.GetActiveScene().name;
        SaveSystem.currentData.lastSavedScene = currentSceneName; // Cập nhật lại tên scene hiện tại vào JSON để khi load lại sẽ biết phải vào scene nào
        SaveSystem.SaveGame(); // Lưu lại trạng thái chuẩn vào JSON sau khi đã chốt số HP và Mana thực tại

        GameSession.CurrentGameState = GameState.Continue;
        GameSession.SessionStarted = false; // Đặt lại trạng thái session để khi vào scene mới sẽ biết là tiếp tục chứ không phải bắt đầu mới
        
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