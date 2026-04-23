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
        Win winScript = FindAnyObjectByType<Win>();
        TMPro.TMP_Text winText = winScript?.GetComponent<TMPro.TMP_Text>();

        if (winScript != null && GameManager.Instance != null)
        {
            int maxWave = GameManager.Instance.DisplayWaveMax();
            int displayWave = GameManager.Instance.DisplayWaveCurrent();
            winScript.UpdateWaveText(displayWave, maxWave);
        }

        while (true)
        {
            int absoluteMaxWave = Mathf.Max(1, GameManager.Instance.LastWave());
            int currentWave = Mathf.Clamp(GameManager.Instance.CurrentWave(), 1, absoluteMaxWave);

            if (winScript != null)
            {
                winScript.UpdateWaveText(GameManager.Instance.DisplayWaveCurrent(), GameManager.Instance.DisplayWaveMax());
            }

            foreach (var spawner in spawners)
            {
                spawner.SpawnEnemy(GameManager.Instance.GetWaveMultiplier());
            }

            // Wait for all enemies to be defeated
            while (FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length > 0)
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (currentWave >= absoluteMaxWave)
            {
                GameManager.Instance.WaveCompleted();
                done = true;
                yield break;
            }

            // Increase difficulty for the next wave
            GameManager.Instance.WaveCompleted();

            // Countdown to next wave
            float countdown = timeBetweenWaves;
            if (winText != null) 
            {
                winText.enabled = true;
            }

            while (countdown > 0)
            {
                if (winText != null)
                {
                    winText.text = $"Next wave in {Mathf.CeilToInt(countdown)} seconds";
                }
                yield return new WaitForSeconds(1f);
                countdown -= 1f;
            }

            if (winText != null) 
            {
                winText.enabled = false;
            }
        }
    }

    public bool IsDoneSpawning()
    {
        return done;
    }
}
