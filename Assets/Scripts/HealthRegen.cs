using UnityEngine;
[RequireComponent(typeof(Player))]

public class HealthRegen : MonoBehaviour
{
    [Header("Health Regen Settings")]
    [Tooltip("Số HP hồi phục mỗi giây khi không bị tấn công")]
    [SerializeField] private float regenPercenPerTick = 0.01f; // 1% HP mỗi giây

    [Tooltip("Giãn cách thời gian tính bằng giây để hồi phục HP một lần")]
    [SerializeField] private float regenInterval = 1f;

    [Tooltip("Thời gian tính bằng giây sau khi bị tấn công mà HP mới bắt đầu hồi phục")]
    [SerializeField] private float combatCooldown = 5f;

    private Player player;
    private float lastDamageTime;
    private float regenTimer;// Timer để tính thời gian hồi phục

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (player == null && player.IsDead)
            return;
        if (Time.time < lastDamageTime + combatCooldown)
        {
            regenTimer = 0f; // Reset timer nếu vẫn đang trong thời gian combat cooldown
            return;
        }

        if (player.GetCurrentHealth() >= player.GetMaxHealth())
        {
            regenTimer = 0f; // Reset timer nếu HP đã đầy
            return; // Không cần hồi phục nếu đã đầy HP
        }

        regenTimer += Time.deltaTime;// Tăng timer theo thời gian
        if (regenTimer >= regenInterval)
        {
            regenTimer -= regenInterval; // Reset timer sau mỗi lần hồi phục
            float regenAmount = Mathf.Max(1, Mathf.RoundToInt(player.GetMaxHealth() * regenPercenPerTick));
            player.Heal(regenAmount);
        }
    }

    public void NotifyCombat()
    {
        lastDamageTime = Time.time;
    }
}
