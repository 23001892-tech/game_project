using UnityEngine;

public class EntityAnimationEvents : MonoBehaviour
{
    private Entity entity;

    private void Awake()
    {
        entity = GetComponentInParent<Entity>();

        if (entity == null)
        {
            Debug.LogError("EntityAnimationEvents không tìm thấy Entity ở object cha.");
        }
    }

    public void DisableMovementAndJump()
    {
        if (entity != null)
        {
            entity.Animation_DisableMovementAndJump();
        }
    }

    public void EnableMovementAndJump()
    {
        if (entity != null)
        {
            entity.Animation_EnableMovementAndJump();
        }
    }

    public void OpenComboWindow()
    {
        if (entity != null)
        {
            entity.Animation_OpenComboWindow();
        }
    }

    public void CloseComboWindow()
    {
        if (entity != null)
        {
            entity.Animation_CloseComboWindow();
        }
    }

    public void FinishAttack()
    {
        if (entity != null)
        {
            entity.Animation_FinishAttack();
        }
    }

    public void DamageEnemies()
    {
        if (entity != null)
        {
            entity.Animation_DamageTargets();
        }
    }

}