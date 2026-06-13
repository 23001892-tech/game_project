using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Player : Entity
{

    [Header("Player Mana")]

    [SerializeField] private int maxMana = 50;
    [SerializeField] private int currentMana;

    [Header("Player UI")]
    [SerializeField] private PlayerUI playerUI;

    [Header("Attack Combo")]
    [SerializeField] private int maxCombo = 4;
    [SerializeField] private float attack1Duration = 0.6f;
    [SerializeField] private float attack2Duration = 0.55f;
    [SerializeField] private float attack3Duration = 0.7f;
    [SerializeField] private float attack4Duration = 0.8f;

    private int comboStep = 0;
    private bool isAttacking = false;
    private int queuedComboClicks = 0;

    protected override void Awake()
    {
        base.Awake();

        currentMana = maxMana;
    }

    private void Start()
    {
        var sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) sprite.enabled = true;

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.UpdateHealthBar(currentHealth, maxHealth);
            PlayerUI.Instance.UpdateManaBar(currentMana, maxMana);
        }


        if (!GameSession.SessionStarted)
        {
            if (GameSession.CurrentGameState == GameState.NewGame || !SaveSystem.LoadGame())
            {
                currentHealth = maxHealth;
                currentMana = maxMana;
                // không đổi transform.position -> giữ vị trí spawn gốc trong scene
            }
            else
            {
                string currentScene = SceneManager.GetActiveScene().name;

                if (SaveSystem.currentData.lastSavedScene == currentScene)
                {
                    transform.position = new Vector3(SaveSystem.currentData.playerX, SaveSystem.currentData.playerY, 0f);
                }

                currentHealth = SaveSystem.currentData.currentHealth;
                currentMana = SaveSystem.currentData.currentMana;
            }

            GameSession.SessionStarted = true;
        }

        SaveSystem.currentData.lastSavedScene = SceneManager.GetActiveScene().name;
        SaveSystem.currentData.playerX = transform.position.x;
        SaveSystem.currentData.playerY = transform.position.y;
        SaveSystem.currentData.currentHealth = currentHealth;
        SaveSystem.currentData.currentMana = currentMana;

        SaveSystem.SaveGame();

        // 4. Cập nhật UI cuối cùng sau khi đã chốt xong xuôi máu mana
        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.UpdateHealthBar(currentHealth, maxHealth);
            PlayerUI.Instance.UpdateManaBar(currentMana, maxMana);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(5);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            UseMana(5);
        }
    }

    protected override void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryToJump();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            TryToAttack();
        }
    }

    protected override void HandleMovement()
    {
        if (canMove)
        {
            MoveX(xInput);
        }
        else
        {
            StopMove();
        }
    }

    protected override void TryToJump()
    {
        if (isGrounded && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void TryToAttack()
    {
        if (!isGrounded)
            return;

        if (!isAttacking)
        {
            queuedComboClicks = 0;
            comboStep = 1;
            StartCoroutine(ComboRoutine());
        }
        else
        {
            if (comboStep + queuedComboClicks < maxCombo)
            {
                queuedComboClicks++;
                Debug.Log("Queue next attack: " + queuedComboClicks);
            }
        }
    }

    private IEnumerator ComboRoutine()
    {
        isAttacking = true;
        EnableMovement(false);

        while (true)
        {
            Debug.Log("Play Attack " + comboStep);

            ResetAttackTriggers();

            if (anim != null)
            {
                anim.SetTrigger("attack" + comboStep);
            }

            yield return new WaitForSeconds(GetAttackDuration(comboStep));

            if (queuedComboClicks > 0 && comboStep < maxCombo)
            {
                queuedComboClicks--;
                comboStep++;
            }
            else
            {
                break;
            }
        }

        EndCombo();
    }

    private void ResetAttackTriggers()
    {
        if (anim == null)
            return;

        anim.ResetTrigger("attack1");
        anim.ResetTrigger("attack2");
        anim.ResetTrigger("attack3");
        anim.ResetTrigger("attack4");
    }

    private float GetAttackDuration(int step)
    {
        if (step == 1) return attack1Duration;
        if (step == 2) return attack2Duration;
        if (step == 3) return attack3Duration;
        if (step == 4) return attack4Duration;

        return 0.6f;
    }

    private void EndCombo()
    {
        Debug.Log("End Combo");

        isAttacking = false;
        queuedComboClicks = 0;
        comboStep = 0;

        EnableMovement(true);
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    public void UseMana(int amount)
    {
        currentMana -= amount;

        if (currentMana < 0)
            currentMana = 0;

        if (PlayerUI.Instance != null)
        {
            PlayerUI.Instance.UpdateManaBar(currentMana, maxMana);
        }
    }

    public override void Animation_DisableMovementAndJump()
    {
        EnableMovement(false);
        StopMove();
    }

    public override void Animation_EnableMovementAndJump()
    {
        if (!isAttacking)
        {
            EnableMovement(true);
        }
    }

    public override void Animation_OpenComboWindow()
    {
        Debug.Log("Open Combo Window");
    }

    public override void Animation_CloseComboWindow()
    {
        Debug.Log("Close Combo Window");
    }

    public override void Animation_FinishAttack()
    {
        Debug.Log("Animation Finish Attack");
    }

    public void SaveCurrentState()
    {
        SaveSystem.currentData.playerX = transform.position.x;
        SaveSystem.currentData.playerY = transform.position.y;
        SaveSystem.currentData.currentHealth = currentHealth;
        SaveSystem.currentData.currentMana = currentMana;
        SaveSystem.currentData.lastSavedScene = SceneManager.GetActiveScene().name;

        SaveSystem.SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentState();
    }
    protected override void Die()
    {
        if (isDead) return;
        isDead = true;
        canMove = false;
        canJump = false;

        // 1. Khóa vật lý
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
        if (col != null) col.enabled = false;

        // 2. Chạy Coroutine xử lý chuỗi sự kiện chết theo thời gian
        StartCoroutine(PlayerDeathRoutine());
    }

    private IEnumerator PlayerDeathRoutine()
    {
        yield return new WaitForSeconds(0.001f);

        if (GameManager.Instance != null) GameManager.Instance.ShowGameOverScreen();

        SpriteRenderer[] allSprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer s in allSprites)
        {
            if (s != null) s.enabled = false;
        }
    }
    public void syncBeforeLoad()
    {
        if (SaveSystem.LoadGame())
        {
            SaveSystem.currentData.currentHealth = currentHealth;
            SaveSystem.currentData.currentMana = currentMana;
        }
    }
}