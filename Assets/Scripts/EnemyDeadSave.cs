using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyDeadSave : MonoBehaviour
{
    private string key => SceneManager.GetActiveScene().name + "_" + gameObject.name;
    private void Start()
    {
        if (SaveSystem.currentData.defeatedEnemyIDs.Contains(key))
        {
            Destroy(gameObject);
        }
    }
    
    public void MarkAsDead()
    {
        if (!SaveSystem.currentData.defeatedEnemyIDs.Contains(key))
        {
            SaveSystem.currentData.defeatedEnemyIDs.Add(key);
            SaveSystem.SaveGame();
        }
    }
}
