using UnityEngine;
using System;


public class PlayerLevelSystem : MonoBehaviour
{
    public static PlayerLevelSystem Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int baseExpToLevelUp = 100;

    [Tooltip("Nhân hệ số này vào exp yêu cầu sau mỗi lần lên cấp. VD: 1.5 = tăng 50%")]
    [SerializeField] private float expMultiplierPerLevel = 1.123f;

    [Header("Attribute Points")]
    [SerializeField] private int attributePoints = 0;

    [Header("Stat Bonuses per Point")]
    [SerializeField] private int hpPerPoint = 10;
    [SerializeField] private int manaPerPoint = 8;
    [SerializeField] private int atkPerPoint = 2;

    private int totalHpSpent = 0;
    private int totalManaSpent = 0;
    private int totalAtkSpent = 0;

    // Cached references
    private Player playerComponent;

    // Events để UI lắng nghe
    public event Action<int, int, int> OnExpChanged;       // currentExp, expToNext, level
    public event Action<int> OnLevelUp;                    // newLevel
    public event Action<int> OnAttributePointsChanged;     // attributePoints

    // Expose properties (readonly)
    public int CurrentLevel => currentLevel;
    public int CurrentExp => currentExp;
    public int AttributePoints => attributePoints;
    public int ExpToNextLevel => CalculateExpToLevelUp(currentLevel);

    private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    playerComponent = GetComponent<Player>();

    LoadLevelData(); // Chuyển vào đây
}

private void Start()
{
    // Bỏ LoadLevelData() ở đây
    OnExpChanged?.Invoke(currentExp, ExpToNextLevel, currentLevel);
    OnAttributePointsChanged?.Invoke(attributePoints);
}

    /// <summary>
    /// Tính lượng EXP cần để lên từ level hiện tại lên level tiếp theo.
    /// </summary>
    public int CalculateExpToLevelUp(int level)
    {
        // baseExpToLevelUp * (multiplier ^ (level - 1)), làm tròn
        return Mathf.RoundToInt(baseExpToLevelUp * Mathf.Pow(expMultiplierPerLevel, level - 1));
    }

    /// <summary>
    /// Nhận EXP. Tự động xử lý level up nếu đủ.
    /// </summary>
    public void GainExp(int amount)
    {
        if (amount <= 0) return;

        currentExp += amount;
        Debug.Log($"[LevelSystem] +{amount} EXP | Total: {currentExp}/{ExpToNextLevel} | Lv.{currentLevel}");

        // Kiểm tra level up (có thể level up nhiều lần liên tiếp)
        while (currentExp >= ExpToNextLevel)
        {
            currentExp -= ExpToNextLevel;
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, ExpToNextLevel, currentLevel);
        SaveLevelData();
    }

    private void LevelUp()
    {
        currentLevel++;
        attributePoints++;

        Debug.Log($"[LevelSystem] ===== LÊN CẤP {currentLevel}! ===== +1 Attribute Point (Total: {attributePoints})");

        OnLevelUp?.Invoke(currentLevel);
        OnAttributePointsChanged?.Invoke(attributePoints);

        // Hiệu ứng UI / âm thanh có thể trigger ở đây
        StatsPanel panel = FindFirstObjectByType<StatsPanel>();
        if (panel != null) panel.ShowLevelUpEffect(currentLevel);
    }

    // ─── Spend Attribute Points ───────────────────────────────────────────────

    /// <summary>
    /// Tăng MaxHP. Gọi từ StatsPanel.
    /// </summary>
    public bool SpendPointOnHP()
    {
        if (!HasPoints()) return false;
        attributePoints--;
        totalHpSpent += hpPerPoint;
        playerComponent?.AddHealth(hpPerPoint);
        OnAttributePointsChanged?.Invoke(attributePoints);
        SaveLevelData();
        return true;
    }

    /// <summary>
    /// Tăng MaxMana. Gọi từ StatsPanel.
    /// </summary>
    public bool SpendPointOnMana()
    {
        if (!HasPoints()) return false;
        attributePoints--;
        totalManaSpent += manaPerPoint;
        playerComponent?.AddMana(manaPerPoint);
        OnAttributePointsChanged?.Invoke(attributePoints);
        SaveLevelData();
        return true;
    }

    /// <summary>
    /// Tăng Attack Damage. Gọi từ StatsPanel.
    /// </summary>
    public bool SpendPointOnAttack()
    {
        if (!HasPoints()) return false;
        attributePoints--;
        totalAtkSpent += atkPerPoint;
        playerComponent?.AddAttackDamage(atkPerPoint);
        OnAttributePointsChanged?.Invoke(attributePoints);
        SaveLevelData();
        return true;
    }

    private bool HasPoints() => attributePoints > 0;


    private void SaveLevelData()
    {
        SaveSystem.currentData.playerLevel = currentLevel;
        SaveSystem.currentData.playerExp = currentExp;
        SaveSystem.currentData.attributePoints = attributePoints;
        SaveSystem.currentData.bonusMaxHp = totalHpSpent;
        SaveSystem.currentData.bonusMaxMana = totalManaSpent;
        SaveSystem.currentData.bonusAtk = totalAtkSpent;
        SaveSystem.SaveGame();
    }

    private void LoadLevelData()
    {
        if (GameSession.CurrentGameState == GameState.Continue && SaveSystem.LoadGame())
        {
            currentLevel = SaveSystem.currentData.playerLevel;
            currentExp = SaveSystem.currentData.playerExp;
            attributePoints = SaveSystem.currentData.attributePoints;
            totalHpSpent = SaveSystem.currentData.bonusMaxHp;
            totalManaSpent = SaveSystem.currentData.bonusMaxMana;
            totalAtkSpent = SaveSystem.currentData.bonusAtk;

            if (playerComponent != null)
            {
                if (totalHpSpent > 0) playerComponent.ApplyMaxHealthBonus(totalHpSpent);
                if (totalManaSpent > 0) playerComponent.ApplyMaxManaBonus(totalManaSpent);
                if (totalAtkSpent > 0) playerComponent.AddAttackDamage(totalAtkSpent);
            }
        }
        else
        {
            currentLevel = 1; currentExp = 0; attributePoints = 0;
            totalHpSpent = 0; totalManaSpent = 0; totalAtkSpent = 0;
        }
    }


    public int GetHpBonus() => hpPerPoint;
    public int GetManaBonus() => manaPerPoint;
    public int GetAtkBonus() => atkPerPoint;
}