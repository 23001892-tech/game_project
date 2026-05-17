using UnityEngine;

public class Enemy : Entity
{
    private bool playerDetected; // Biến đánh dấu có phát hiện người chơi không

    protected override void Update()
    {
        // Cố ý không gọi base.Update() để bỏ qua hàm nhận nút bấm HandleInput
        HandleCollision();
        HandleAnimations();
        HandleMovement();
        HandleFlip();
        
        HandleAttack(); // Gọi liên tục mỗi khung hình để kiểm tra và chém
    }

    protected override void HandleMovement()
    {
        if (canMove)
        {
            rb.linearVelocity = new Vector2(moveSpeed * facingDirection, rb.linearVelocity.y);
        }
    }

    protected override void HandleCollision()
    {
        base.HandleCollision(); // Vẫn gọi base để giữ lại tính năng quét mặt đất
        
        // Quét quanh attackPoint, nếu chạm vào layer whatIsTarget (Player) thì gán là true
        playerDetected = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsTarget);
    }

    protected override void HandleAttack()
    {
        // Nếu phát hiện người chơi, tự động kích hoạt hoạt ảnh chém
        if (playerDetected)
        {
            anim.SetTrigger("attack"); 
        }
    }
}