using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // Thao tác chuyển Scene (màn chơi)
using UnityEngine.UI; // Thao tác với các thành phần UI truyền thống

public class MainMenu : MonoBehaviour
{
    [Header("Cấu hình Tên Màn Chơi Mới")]
    [SerializeField] private string newGameSceneName = "SampleScene";

    [Header("Các nút bấm")]
    [SerializeField] private Button continueButton;

    private void Start()
    {
        AudioManager.Instance.PlayMainMenuMusic();
        if (continueButton != null)
        {
            // 1. Kiểm tra chính xác xem có file JSON tồn tại hay không thông qua SaveSystem
            // Đồng thời check xem trong file đó đã từng lưu scene nào chưa
            bool hasSave = SaveSystem.LoadGame() && !string.IsNullOrEmpty(SaveSystem.currentData.lastSavedScene);

            continueButton.interactable = hasSave;

            CanvasGroup canvasGroup = continueButton.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = continueButton.gameObject.AddComponent<CanvasGroup>();
            }

            if (hasSave)
            {
                canvasGroup.alpha = 1.0f; // 
                Debug.Log("Nút Continue phát sáng vì ĐÃ CÓ FILE SAVE JSON!");
            }
            else
            {
                canvasGroup.alpha = 0.25f; // 
                Debug.Log("Nút Continue tối đen vì CHƯA CÓ FILE SAVE JSON!");
            }
        }
        else
        {
            Debug.LogError("Cảnh báo: Bạn chưa kéo thả nút Continue vào ô biến trên Inspector!");
        }
    }

    public void NewGame()
    {
        SaveSystem.ClearSaveData();

        SaveSystem.currentData.lastSavedScene = newGameSceneName;
        SaveSystem.SaveGame();


        GameSession.CurrentGameState = GameState.NewGame;
        GameSession.SessionStarted = false; // Đặt lại trạng thái session để khi vào scene mới sẽ biết là bắt đầu mới chứ không phải tiếp tục


        // 4. Load vào màn chơi mới bằng LoadingProgress xịn của bạn
        if (LoadingProgress.Instance != null)
        {
            LoadingProgress.Instance.LoadScene(newGameSceneName);
        }
        else
        {
            SceneManager.LoadScene(newGameSceneName);
        }
    }

    public void ContinueGame()
    {
        GameSession.CurrentGameState = GameState.Continue;
        GameSession.SessionStarted = false; // Đặt lại trạng thái session để khi vào scene mới sẽ biết là tiếp tục chứ không phải bắt đầu mới

        if (SaveSystem.LoadGame() && !string.IsNullOrEmpty(SaveSystem.currentData.lastSavedScene))
        {
            string savedScene = SaveSystem.currentData.lastSavedScene;
            Debug.Log("Đang tải lại màn chơi cũ từ JSON: " + savedScene);

            if (LoadingProgress.Instance != null)
            {
                LoadingProgress.Instance.LoadScene(savedScene);
            }
            else
            {
                SceneManager.LoadScene(savedScene);
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy dữ liệu file JSON hoặc tên Scene trống rỗng!");
        }
    }

    // Gọi từ nút "Settings" trong Main Menu
    public void OpenSettings()
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.Open();
    }

    // Gọi từ nút "Back" trong Settings Panel
    public void CloseSettings()
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.Close();
    }

    // Hàm gọi khi nhấn vào nút EXIT
    public void ExitGame()
    {
        Debug.Log("Đang thoát game...");

        // Thoát ứng dụng
        Application.Quit();

#if UNITY_EDITOR
        // Nếu đang chạy thử bằng Unity Editor thì dừng chế độ Play
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}