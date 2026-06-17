using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Bảng chỉ số mở/đóng bằng phím G.
/// Attach vào một Panel GameObject trong Canvas.
/// </summary>
public class StatsPanel : MonoBehaviour
{
    [Header("Panel Reference")]
    [SerializeField] private GameObject panelRoot;

    [Header("Level & EXP")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private Slider expBar;

    [Header("Attribute Points")]
    [SerializeField] private TextMeshProUGUI attributePointsText;

    [Header("HP Row")]
    [SerializeField] private TextMeshProUGUI hpValueText;
    [SerializeField] private Button hpUpButton;
    [SerializeField] private TextMeshProUGUI hpBonusHint;   // e.g. "+10 HP"

    [Header("Mana Row")]
    [SerializeField] private TextMeshProUGUI manaValueText;
    [SerializeField] private Button manaUpButton;
    [SerializeField] private TextMeshProUGUI manaBonusHint; // e.g. "+8 Mana"

    [Header("ATK Row")]
    [SerializeField] private TextMeshProUGUI atkValueText;
    [SerializeField] private Button atkUpButton;
    [SerializeField] private TextMeshProUGUI atkBonusHint;  // e.g. "+2 ATK"

    [Header("Level Up Effect")]
    [SerializeField] private GameObject levelUpBanner;   // Optional: "LEVEL UP!" text/panel
    [SerializeField] private float levelUpBannerDuration = 2f;

    private PlayerLevelSystem levelSystem;
    private Player player;
    private bool isPanelOpen = false;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        // Wire buttons
        if (hpUpButton != null)   hpUpButton.onClick.AddListener(OnHpUp);
        if (manaUpButton != null) manaUpButton.onClick.AddListener(OnManaUp);
        if (atkUpButton != null)  atkUpButton.onClick.AddListener(OnAtkUp);

        if (levelUpBanner != null)
            levelUpBanner.SetActive(false);
    }

    private void Start()
    {
        levelSystem = PlayerLevelSystem.Instance;
        player = FindFirstObjectByType<Player>();

        if (levelSystem == null)
        {
            Debug.LogWarning("[StatsPanel] Không tìm thấy PlayerLevelSystem!");
            return;
        }

        // Subscribe events
        levelSystem.OnExpChanged += HandleExpChanged;
        levelSystem.OnLevelUp += HandleLevelUp;
        levelSystem.OnAttributePointsChanged += HandleAttributePointsChanged;

        // Show bonus hints
        if (hpBonusHint != null)   hpBonusHint.text   = $"+{levelSystem.GetHpBonus()} HP";
        if (manaBonusHint != null) manaBonusHint.text  = $"+{levelSystem.GetManaBonus()} Mana";
        if (atkBonusHint != null)  atkBonusHint.text   = $"+{levelSystem.GetAtkBonus()} ATK";

        // Initial refresh
        RefreshAll();
    }

    private void OnDestroy()
    {
        if (levelSystem != null)
        {
            levelSystem.OnExpChanged -= HandleExpChanged;
            levelSystem.OnLevelUp -= HandleLevelUp;
            levelSystem.OnAttributePointsChanged -= HandleAttributePointsChanged;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            TogglePanel();
        }
    }

    // ─── Toggle ──────────────────────────────────────────────────────────────

    private void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;

        if (panelRoot != null)
            panelRoot.SetActive(isPanelOpen);

        if (isPanelOpen)
        {
            RefreshAll();

            // Pause movement input while panel is open (optional)
            Time.timeScale = 0f; // Comment dòng này nếu không muốn pause game
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void ClosePanel()
    {
        if (!isPanelOpen) return;
        isPanelOpen = false;
        if (panelRoot != null) panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    // ─── Refresh UI ──────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (levelSystem == null) return;

        // Level
        if (levelText != null)
            levelText.text = $"Lv. {levelSystem.CurrentLevel}";

        // EXP
        int cur  = levelSystem.CurrentExp;
        int next = levelSystem.ExpToNextLevel;

        if (expText != null)
            expText.text = $"{cur} / {next}";

        if (expBar != null)
        {
            expBar.minValue = 0;
            expBar.maxValue = next;
            expBar.value    = cur;
        }

        // Attribute points
        int pts = levelSystem.AttributePoints;
        if (attributePointsText != null)
            attributePointsText.text = $"Điểm nâng chỉ số: {pts}";

        bool hasPoint = pts > 0;
        if (hpUpButton   != null) hpUpButton.interactable   = hasPoint;
        if (manaUpButton != null) manaUpButton.interactable = hasPoint;
        if (atkUpButton  != null) atkUpButton.interactable  = hasPoint;

        // Stat values (from Player)
        RefreshStatValues();
    }

    private void RefreshStatValues()
    {
        if (player == null) return;

        if (hpValueText != null)
            hpValueText.text = $"HP: {player.GetCurrentHealth()} / {player.GetMaxHealth()}";

        if (manaValueText != null)
            manaValueText.text = $"Mana: {player.GetCurrentMana()} / {player.GetMaxMana()}";

        if (atkValueText != null)
            atkValueText.text = $"ATK: {player.GetAttackDamage()}";
    }

    // ─── Event Handlers ──────────────────────────────────────────────────────

    private void HandleExpChanged(int cur, int next, int level)
    {
        if (!isPanelOpen) return;
        RefreshAll();
    }

    private void HandleLevelUp(int newLevel)
    {
        RefreshAll();
    }

    private void HandleAttributePointsChanged(int pts)
    {
        if (attributePointsText != null)
            attributePointsText.text = $"Điểm chỉ số: {pts}";

        bool hasPoint = pts > 0;
        if (hpUpButton   != null) hpUpButton.interactable   = hasPoint;
        if (manaUpButton != null) manaUpButton.interactable = hasPoint;
        if (atkUpButton  != null) atkUpButton.interactable  = hasPoint;

        RefreshStatValues();
    }

    // ─── Button Callbacks ────────────────────────────────────────────────────

    private void OnHpUp()
    {
        if (levelSystem != null && levelSystem.SpendPointOnHP())
        {
            RefreshAll();
        }
    }

    private void OnManaUp()
    {
        if (levelSystem != null && levelSystem.SpendPointOnMana())
        {
            RefreshAll();
        }
    }

    private void OnAtkUp()
    {
        if (levelSystem != null && levelSystem.SpendPointOnAttack())
        {
            RefreshAll();
        }
    }

    // ─── Level Up Banner ─────────────────────────────────────────────────────

    public void ShowLevelUpEffect(int newLevel)
    {
        StartCoroutine(LevelUpBannerRoutine(newLevel));
    }

    private IEnumerator LevelUpBannerRoutine(int newLevel)
    {
        if (levelUpBanner == null) yield break;

        // Cập nhật text nếu banner có TMPro con
        TextMeshProUGUI bannerText = levelUpBanner.GetComponentInChildren<TextMeshProUGUI>();
        if (bannerText != null)
            bannerText.text = $"LEVEL UP!\nLv. {newLevel}";

        levelUpBanner.SetActive(true);

        // Nếu game đang pause thì dùng WaitForSecondsRealtime
        yield return new WaitForSecondsRealtime(levelUpBannerDuration);

        levelUpBanner.SetActive(false);
    }
}