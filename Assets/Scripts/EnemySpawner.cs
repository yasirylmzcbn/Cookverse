using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawners;
    [SerializeField] private GameObject enemy;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnEnemy();
        }
    }
    private void SpawnEnemy()
    {
        int randomInt = Random.RandomRange(1, spawners.Length);
        Transform randomSpawner = spawners[randomInt];
        Instantiate(enemy, randomSpawner.position, randomSpawner.rotation);
    }
}
