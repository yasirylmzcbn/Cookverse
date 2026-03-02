using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawners;
    [SerializeField] private GameObject enemy;
    private bool doneSpawning = false;

    private void Start()
    {
        StartCoroutine(SpawnEnemiesRoutine());
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        float duration = 10f;
        float interval = 5f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            // Spawn 10 enemies
            for (int i = 0; i < 10; i++)
            {
                SpawnEnemy();
            }

            // Wait 5 seconds before next wave
            yield return new WaitForSeconds(interval);
            elapsedTime += interval;
        }
        doneSpawning = true;
    }

    private void SpawnEnemy()
    {
        int randomInt = Random.Range(0, spawners.Length);
        Transform randomSpawner = spawners[randomInt];
        GameObject enemyObject = Instantiate(enemy, randomSpawner.position, randomSpawner.rotation);
    }

    public bool IsDoneSpawning()
    {
        return doneSpawning;
    }
}
