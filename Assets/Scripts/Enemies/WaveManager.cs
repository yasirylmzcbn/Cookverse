using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner[] spawners;
    private float timeBetweenWaves = 3f;
    private int currentWave = 1;
    private int lastWave = 2;
    private bool done = false;
    private void Start()
    {
        done = false;
        StartCoroutine(WaveLoop());
    }

    private IEnumerator WaveLoop()
    {
        while (true)
        {
            foreach (var spawner in spawners)
            {
                spawner.SpawnEnemy(GameManager.Instance.GetWaveMultiplier());
            }

            // Increase difficulty
            GameManager.Instance.WaveCompleted();
            currentWave += 1;

            // Small delay before next wave
            if (currentWave == lastWave)
            {
                done = true;
                yield break;
            }
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    public bool IsDoneSpawning()
    {
        return done;
    }
}
