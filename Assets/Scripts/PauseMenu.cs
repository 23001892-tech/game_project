using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có để chuyển cảnh

public class PauseMenu : MonoBehaviour
{
    // Kéo cái PauseMenuPanel trong Unity vào đây
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    void Update()
    {
        // Kiểm tra nếu người chơi bấm nút ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // 1. Hàm xử lý nút RESUME (Chạy tiếp)
    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Ẩn Menu đi
        Time.timeScale = 1f;          // Cho thời gian game chạy bình thường trở lại
        isPaused = false;

        // Thêm dòng này nếu game của bạn có khóa chuột (bắn súng, góc nhìn thứ 1...)
        // Cursor.lockState = CursorLockMode.Locked; 
    }

    // Hàm gọi khi bấm ESC để dừng game
    void Pause()
    {
        pauseMenuUI.SetActive(true);  // Hiện Menu lên
        Time.timeScale = 0f;          // Đóng băng toàn bộ thời gian trong game
        isPaused = true;

        // Hiện chuột lên để người chơi còn click được nút
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenSettings()
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.Open();
    }

    // Gọi từ nút "Back" trong Settings Panel
    public void CloseSettings()
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.Close();
    }

    // 2. Hàm xử lý nút EXIT TO MAIN MENU
    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;

        Player player = FindAnyObjectByType<Player>(); // Unity mới dùng FindAnyObjectByType, bản cũ dùng FindObjectOfType
        if (player != null)
        {
            player.SaveCurrentState();
        }

        SceneManager.LoadScene("MainMenu");
    }
}