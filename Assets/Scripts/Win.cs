using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Win : MonoBehaviour
{
    private TMP_Text winText;
    private bool hasWon = false;
    public WaveManager waveManager;

    [SerializeField] private GameObject portal;

    void Start()
    {
        winText = GetComponent<TMP_Text>();
        Debug.Log(winText != null);
        if (winText != null)
        {
            winText.enabled = false;
        }
        waveManager = FindObjectsByType<WaveManager>(FindObjectsSortMode.None)?[0];
    }

    void Update()
    {
        //Debug.Log(!hasWon);
        //Debug.Log(spawner.IsDoneSpawning());
        //Debug.Log(spawner.EnemyCount());
        if (!hasWon && waveManager.IsDoneSpawning() && FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length == 0)
        {
            ShowWin();
            portal.SetActive(true);
        }
    }

    void ShowWin()
    {
        hasWon = true;
        winText.enabled = true;
    }
}