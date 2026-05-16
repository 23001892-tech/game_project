using UnityEngine;

public class Enemy : MonoBehaviour // Thay vì kế thừa MonoBehaviour, hãy kế thừa Entity [3]
{
    public float moveSpeed;
    public string enemyName;

    private void MoveAround()
    {
        Debug.Log(enemyName + " moves at speed" +  moveSpeed);

    }

    private void Attack()
    {
        Debug.Log(enemyName + " attacks!");
    }
    public void TakeDamage()
    {
        
    }
}