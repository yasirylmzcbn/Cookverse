using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Serializable]
    private struct SceneTrack
    {
        public string sceneName;
        public AudioClip clip;
    }

    [Header("References")]
    [SerializeField] private AudioSource musicSource;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            if (musicSource != null)
                musicSource.volume = musicVolume;
        }
    }

    [Header("Scene Music")]
    [SerializeField] private List<SceneTrack> sceneTracks = new List<SceneTrack>();

    private readonly Dictionary<string, AudioClip> trackByScene = new Dictionary<string, AudioClip>();
    private string currentSceneName = string.Empty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
        {
            Debug.LogError("MusicManager requires an AudioSource.");
            enabled = false;
            return;
        }

        musicSource.ignoreListenerVolume = true;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        RebuildTrackLookup();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        PlayForScene(SceneManager.GetActiveScene().name);
    }

    private void OnValidate()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;

        if (!Application.isPlaying)
            return;

        RebuildTrackLookup();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    private void RebuildTrackLookup()
    {
        trackByScene.Clear();

        for (int i = 0; i < sceneTracks.Count; i++)
        {
            SceneTrack track = sceneTracks[i];

            if (string.IsNullOrWhiteSpace(track.sceneName) || track.clip == null)
                continue;

            trackByScene[track.sceneName] = track.clip;
        }
    }

    private void PlayForScene(string sceneName)
    {
        if (string.Equals(currentSceneName, sceneName, StringComparison.Ordinal))
            return;

        if (!trackByScene.TryGetValue(sceneName, out AudioClip clip) || clip == null)
        {
            currentSceneName = sceneName;
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            currentSceneName = sceneName;
            return;
        }

        musicSource.clip = clip;
        musicSource.Play();
        currentSceneName = sceneName;
    }
}
