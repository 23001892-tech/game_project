using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroStoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "Map1";

    [Header("Story Pages")]
    [TextArea(5, 10)]
    [SerializeField] private string[] storyPages =
    {
        "Năm 2034, một loại khoáng sản xanh tím được tìm thấy dưới một khu mỏ bỏ hoang.\n\nNgười ta gọi nó là Lumenite.",

        "Ban đầu, Lumenite được xem là phép màu của nhân loại.\n\nNó có thể chữa lành những vết thương không thể phục hồi, tái tạo tế bào chết, thậm chí đánh thức năng lực vượt xa giới hạn con người.",

        "Những người được chữa trị bằng nó được gọi là The Ascended — Những Kẻ Thăng Hoa.\n\nNhưng Lumenite không thật sự chữa lành.\n\nNó chỉ đang chuẩn bị vật chủ.",

        "Sau một thời gian ủ bệnh, những người từng được cứu bắt đầu nghe thấy tiếng thì thầm từ đá.\n\nTừ các vết thương đã lành, tinh thể xanh tím mọc xuyên qua da thịt. Xương khớp biến dạng. Ký ức tan vỡ. Bản ngã bị nuốt dần.",

        "Những người từng là bệnh nhân, bác sĩ, công nhân và siêu nhân giờ trở thành những sinh vật bảo vệ Lumenite bằng bản năng.\n\nKhu điều trị bị phong tỏa. Hầm mỏ bị bỏ lại. Không ai còn dám xuống nơi mọi thứ bắt đầu.",

        "Bạn từng là một bệnh nhân trong chương trình điều trị ấy.\n\nHồ sơ ghi rằng bạn không còn khả năng sống sót.\n\nNhưng bạn đã tỉnh lại.",

        "Trong lồng ngực bạn xuất hiện một vết nứt phát sáng hình dấu gạch chéo.\n\nKhi những người khác mất trí, bạn vẫn giữ được bản ngã.\n\nKhi Lumenite cố nuốt chửng cơ thể bạn, nó bị kẹt lại.",

        "Bạn là vật chủ lỗi.\n\nHoặc là chìa khóa cuối cùng.",

        "Muốn biết vì sao mình chưa biến thành quái vật, bạn phải quay lại nơi mọi thứ bắt đầu.\n\nKhu điều trị bỏ hoang.\n\nMiệng mỏ cũ.\n\nNơi Lumenite vẫn đang thở."
    };

    [Header("Typewriter")]
    [SerializeField] private bool useTypewriterEffect = true;
    [SerializeField] private float characterDelay = 0.025f;

    private int currentPageIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (introPanel != null)
            introPanel.SetActive(true);

        if (titleText != null)
            titleText.text = "MỞ ĐẦU";

        if (hintText != null)
            hintText.text = "Nhấn Space để tiếp tục";

        currentPageIndex = 0;

        if (storyPages == null || storyPages.Length == 0)
        {
            LoadNextScene();
            return;
        }

        ShowCurrentPage();
    }

    private void Update()
    {


        if (
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetMouseButtonDown(0)
        )
        {
            ContinueIntro();
        }
    }

    private void ContinueIntro()
    {
        if (isTyping)
        {
            CompleteCurrentPageImmediately();
            return;
        }

        currentPageIndex++;

        if (currentPageIndex >= storyPages.Length)
        {
            LoadNextScene();
            return;
        }

        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        string page = storyPages[currentPageIndex];

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (useTypewriterEffect)
        {
            typingCoroutine = StartCoroutine(TypePage(page));
        }
        else
        {
            storyText.text = page;
        }
    }

    private IEnumerator TypePage(string page)
    {
        isTyping = true;
        storyText.text = "";

        for (int i = 0; i < page.Length; i++)
        {
            storyText.text += page[i];
            yield return new WaitForSeconds(characterDelay);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void CompleteCurrentPageImmediately()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        storyText.text = storyPages[currentPageIndex];
        isTyping = false;
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}