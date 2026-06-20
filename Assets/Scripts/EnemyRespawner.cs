using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float minDistanceFromPlayer = 3f;

    [Header("Spawn Timing")]
    [SerializeField] private float startCooldown = 3f;
    [SerializeField] private float minCooldown = 1f;
    [SerializeField] private float cooldownDecreaseRate = 0.05f;

    [Header("Difficulty")]
    [SerializeField] private int maxEnemiesAlive = 8;
    [SerializeField] private bool increaseDifficulty = true;

    private float currentCooldown;
    private float timer;

    private Transform player;
    private readonly List<GameObject> aliveEnemies = new List<GameObject>();

    private void Awake()
    {
        currentCooldown = startCooldown;
        timer = startCooldown;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("EnemyRespawner không tìm thấy Player. Hãy đặt Tag của Player là Player.");
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        CleanDeadEnemies();

        if (aliveEnemies.Count >= maxEnemiesAlive)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();

            timer = currentCooldown;

            if (increaseDifficulty)
            {
                currentCooldown = Mathf.Max(
                    minCooldown,
                    currentCooldown - cooldownDecreaseRate
                );
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("Chưa gán Enemy Prefabs cho EnemyRespawner.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("Chưa gán Spawn Points cho EnemyRespawner.");
            return;
        }

        GameObject prefab = GetRandomEnemyPrefab();
        Transform spawnPoint = GetValidSpawnPoint();

        if (prefab == null || spawnPoint == null)
            return;

        GameObject newEnemy = Instantiate(
            prefab,
            spawnPoint.position,
            Quaternion.identity
        );

        aliveEnemies.Add(newEnemy);
    }

    private GameObject GetRandomEnemyPrefab()
    {
        int index = Random.Range(0, enemyPrefabs.Length);
        return enemyPrefabs[index];
    }

    private Transform GetValidSpawnPoint()
    {
        // Thử chọn điểm spawn xa Player vài lần
        for (int i = 0; i < 10; i++)
        {
            int index = Random.Range(0, spawnPoints.Length);
            Transform point = spawnPoints[index];

            if (point == null)
                continue;

            float distance = Vector2.Distance(point.position, player.position);

            if (distance >= minDistanceFromPlayer)
            {
                return point;
            }
        }

        // Nếu không tìm được điểm đủ xa, lấy tạm điểm ngẫu nhiên
        int fallbackIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[fallbackIndex];
    }

    private void CleanDeadEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (spawnPoints == null)
            return;

        Gizmos.color = Color.yellow;

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.4f);
            }
        }
    }
}