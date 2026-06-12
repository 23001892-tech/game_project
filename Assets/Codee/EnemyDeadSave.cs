using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyDeadSave : MonoBehaviour
{
    private void Awake()
    {
        String key = SceneManager.GetActiveScene().name + "_" + gameObject.name;
        if (PlayerPrefs.GetInt(key, 0) == 1)
        {
            Destroy(gameObject);
        }
    }
    
    public void MarkAsDead()
    {
        String key = SceneManager.GetActiveScene().name + "_" + gameObject.name;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }
}
