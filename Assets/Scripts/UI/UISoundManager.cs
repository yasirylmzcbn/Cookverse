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
            Destroy(this);
            return;
        }

        // Extremely safe singleton pattern: If the user attached this to a very important scene object (like the Camera or Player),
        // we DO NOT want to apply DontDestroyOnLoad to it, because it breaks scene reloading, audio listeners, and interactions.
        // Instead, we spawn a completely invisible independent object, transfer our clips, and destroy this script from the host.
        if (gameObject.name != "GlobalUISoundManager")
        {
            GameObject bgGo = new GameObject("GlobalUISoundManager");
            DontDestroyOnLoad(bgGo);

            UISoundManager newManager = bgGo.AddComponent<UISoundManager>();
            newManager.hoverSound = this.hoverSound;
            newManager.clickSound = this.clickSound;

            Destroy(this);
            return;
        }

        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
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
        Selectable[] selectables = FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
