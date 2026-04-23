using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        audioSource.ignoreListenerVolume = true;
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InjectSoundsIntoAllButtons();
    }

    public void InjectSoundsIntoAllButtons()
    {
        Selectable[] selectables = Resources.FindObjectsOfTypeAll<Selectable>();
        foreach (var sel in selectables)
        {
            // Only modify objects in the scene, not prefabs
            if (sel.gameObject.scene.name != null && sel.GetComponent<UIButtonSound>() == null)
            {
                sel.gameObject.AddComponent<UIButtonSound>();
            }
        }
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
