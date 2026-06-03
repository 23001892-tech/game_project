using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomTransition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [Header("Cấu hìn chuyển phòng")]
    [SerializeField] private string sceneToLoad; // Tên của scene tiếp theo

    [header("Giao diện tương tác chuyển phòng")]
    [SerializeField] private GameObject interactMessageUI; // UI hiển thị khi người chơi có thể chuyển phòng
    private bool playerInZone = false; // Biến để kiểm tra xem người chơi có đang trong vùng trigger hay không
    
    private void Start()
    {
        if (interactMessageUI != null)
            interactMessageUI.SetActive(false); // Ẩn UI khi bắt đầu
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;

            if (interactMessageUI != null)
                interactMessageUI.SetActive(true); // Hiển thị UI khi người chơi vào vùng trigger
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            if (interactMessageUI != null)
                interactMessageUI.SetActive(false); // Ẩn UI khi người chơi rời khỏi vùng trigger
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (playerInZone && Input.GetKeyDown(KeyCode.E))
        {
            EnterNewRoom();
        }
    }

    private void EnterNewRoom()
    {
        // Gọi hàm chuyển scene ở đây
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
        else 
            Debug.LogWarning("Tên scene không được để trống!");
    }
}
