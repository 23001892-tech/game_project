using UnityEngine;

/// <summary>
/// Gắn vào Enemy. Khi enemy chết sẽ tự động cho Player nhận exp.
/// </summary>
public class ExpDrop : MonoBehaviour
{
    [Header("EXP Settings")]
    [SerializeField] private int expAmount = 10;

    private bool hasDropped = false;

    private void Update()
    {
        // Detect khi Entity bị destroy (isDead) bằng cách check component
        Entity entity = GetComponent<Entity>();
        if (entity == null) return;

        // Dùng reflection-free approach: hook vào OnDestroy
    }

    public void DropExp()
    {
        if (hasDropped) return;
        hasDropped = true;

        // Tìm Player trong scene
        PlayerLevelSystem playerLevel = FindFirstObjectByType<PlayerLevelSystem>();
        if (playerLevel != null)
        {
            playerLevel.GainExp(expAmount);
            Debug.Log($"[ExpDrop] {gameObject.name} dropped {expAmount} EXP.");
        }
        else
        {
            Debug.LogWarning("[ExpDrop] Không tìm thấy PlayerLevelSystem trong scene!");
        }
    }

    private void OnDestroy()
    {
        // Gọi khi GameObject bị Destroy (tức là sau khi enemy chết và delay kết thúc)
        DropExp();
    
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (expAmount < 0) expAmount = 0;
    }
#endif
}