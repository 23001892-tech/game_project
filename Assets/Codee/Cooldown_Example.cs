using UnityEngine;

public class Cooldown_Example : MonoBehaviour
{
    private SpriteRenderer sr;
    
    [Header("Damage Feedback")]
    [SerializeField] private float redColorDuration = 0.2f; // Thời gian kẻ địch chớp đỏ
    private float timer;

    private void Awake()
    {
        // Lấy component hình ảnh để can thiệp đổi màu
        sr = GetComponent<SpriteRenderer>(); 
    }

    private void Update()
    {
        // Liên tục trừ đi khoảng thời gian giữa các khung hình (Time.deltaTime) để đếm ngược
        timer -= Time.deltaTime; 

        // Nếu bộ đếm thời gian đã tụt xuống dưới 0 và kẻ địch vẫn đang đổi màu -> Trả lại màu trắng gốc
        if (timer < 0 && sr.color != Color.white) 
        {
            sr.color = Color.white; 
        }
    }

    // Hàm này được Player gọi khi thanh kiếm chạm vào
    public void TakeDamage()
    {
        Debug.Log(gameObject.name + " took some damage"); // In ra Console báo hiệu bị chém trúng
        
        sr.color = Color.red;       // Lập tức đổi màu kẻ địch sang đỏ
        timer = redColorDuration;   // Đặt lại bộ đếm thời gian (ví dụ: 0.2 giây)
    }
} 