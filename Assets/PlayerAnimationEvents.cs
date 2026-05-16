using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();

        if (player == null)
        {
            Debug.LogError("PlayerAnimationEvents không tìm thấy Player ở object cha.");
        }
    }

    public void DisableMovementAndJump()
    {
        if (player != null)
        {
            player.Animation_DisableMovementAndJump();
        }
    }

    public void EnableMovementAndJump()
    {
        if (player != null)
        {
            player.Animation_EnableMovementAndJump();
        }
    }

    public void OpenComboWindow()
    {
        if (player != null)
        {
            player.Animation_OpenComboWindow();
        }
    }

    public void CloseComboWindow()
    {
        if (player != null)
        {
            player.Animation_CloseComboWindow();
        }
    }

    public void FinishAttack()
    {
        if (player != null)
        {
            player.Animation_FinishAttack();
        }
    }

    public void DamageEnemies()
    {
        player.DamageEnemies(); 
    }

}