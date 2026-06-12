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
    if (continueButton != null)
    {
        // 1. Kiểm tra chính xác xem có file dữ liệu cũ hay không
        bool hasSave = PlayerPrefs.HasKey("HasSavedGame") && PlayerPrefs.HasKey("LastSavedScene");

        // 2. Cho phép hoặc khóa bấm nút
        continueButton.interactable = hasSave;

        // 3. Ép độ mờ/sáng trực tiếp bằng CanvasGroup (Bỏ qua cơ chế Color trộn của Image)
        CanvasGroup canvasGroup = continueButton.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            // Nếu chưa có thì tự động thêm component này vào nút Continue
            canvasGroup = continueButton.gameObject.AddComponent<CanvasGroup>();
        }

        // 4. Thay đổi độ sáng hiển thị dựa trên việc có file save hay không
        if (hasSave)
        {
            canvasGroup.alpha = 1.0f; // Sáng rõ 100%, rực rỡ như ảnh gốc khi CÓ SAVE
            Debug.Log("Nút Continue phát sáng vì ĐÃ CÓ FILE SAVE!");
        }
        else
        {
            canvasGroup.alpha = 0.25f; // Tối sầm và mờ tịt hẳn đi khi KHÔNG CÓ SAVE
            Debug.Log("Nút Continue tối đen vì CHƯA CÓ FILE SAVE!");
        }
    }
    else
    {
        Debug.LogError("Cảnh báo: Bạn chưa kéo thả nút Continue vào ô biến trên Inspector!");
    }
}

    public void NewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("HasSavedGame", 1);
        PlayerPrefs.SetString("LastSavedScene", newGameSceneName);
        PlayerPrefs.Save();
        LoadingProgress.Instance.LoadScene(newGameSceneName);
    }

    public void ContinueGame()
    {
        if (PlayerPrefs.HasKey("LastSavedScene"))
        {
            string savedScene = PlayerPrefs.GetString("LastSavedScene");
            Debug.Log("Đang tải lại màn chơi cũ: " + savedScene);
            

            LoadingProgress.Instance.LoadScene(savedScene);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy dữ liệu màn chơi cũ!");
        }
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