using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner[] spawners;
    private float timeBetweenWaves = 10f;
    private bool done = false;
    private void Start()
    {
        done = false;
        GameManager.Instance.EnsureStartingWave(); //called every time player enters other world
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

            // Small delay before next wave
            if (GameManager.Instance.CurrentWave() == GameManager.Instance.LastWave())
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
