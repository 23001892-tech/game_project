using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDoor : MonoBehaviour
{
    [Header("Door Animation")]
    [SerializeField] private Animator anim;
    [SerializeField] private string openStateName = "DoorOpen";

    [Header("Interact")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string nextSceneName = "Map2";

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private bool isOpen;
    private bool playerInside;

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        // Ban đầu tắt Animator để cửa không tự chạy animation mở
        if (anim != null)
            anim.enabled = false;
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (!playerInside)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            LoadNextScene();
        }
    }

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        if (showDebugLog)
            Debug.Log("BossDoor: Cửa đã mở, có thể ấn E để đi tiếp.");

        if (anim != null)
        {
            anim.enabled = true;
            anim.Play(openStateName, 0, 0f);
        }
        else
        {
            Debug.LogWarning("BossDoor: Không tìm thấy Animator!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (showDebugLog)
            Debug.Log("Player đang đứng trong vùng cửa. Nếu cửa đã mở thì ấn E để đi tiếp.");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (showDebugLog)
            Debug.Log("Player rời khỏi vùng cửa.");
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("BossDoor: Chưa điền Next Scene Name!");
            return;
        }

        if (showDebugLog)
            Debug.Log("BossDoor: Chuyển sang scene " + nextSceneName);

        SceneManager.LoadScene(nextSceneName);
    }
}