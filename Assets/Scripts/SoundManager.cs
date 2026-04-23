using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Centralized sound manager for all game audio.
/// All sound effects are defined here for easy access and management.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Player Sounds")]
    [Tooltip("Sound played when the player takes damage")]
    [SerializeField] private AudioClip playerHitSound;
    [Tooltip("Sound played when the player walks")]
    [SerializeField] private AudioClip footstepSound;
    [Tooltip("Sound played when the player jumps")]
    [SerializeField] private AudioClip jumpSound;

    [Header("Enemy Sounds")]
    [Tooltip("Sound played when an enemy takes damage")]
    [SerializeField] private AudioClip enemyHitSound;
    [Tooltip("Sound played when an enemy is defeated")]
    [SerializeField] private AudioClip enemyDefeatedSound;

    [Header("UI Sounds")]
    [Tooltip("Sound played when hovering over a UI element")]
    [SerializeField] private AudioClip uiHoverSound;
    [Tooltip("Sound played when clicking a UI button")]
    [SerializeField] private AudioClip uiClickSound;
    [Tooltip("Sound played when opening a menu")]
    [SerializeField] private AudioClip uiOpenSound;
    [Tooltip("Sound played when closing a menu")]
    [SerializeField] private AudioClip uiCloseSound;
    [Tooltip("Sound played when starting to drag an item")]
    [SerializeField] private AudioClip uiDragStartSound;
    [Tooltip("Sound played when dropping an item in a slot")]
    [SerializeField] private AudioClip uiDropSound;
    [Tooltip("Sound played when switching modes (cooking, difficulty, etc.)")]
    [SerializeField] private AudioClip uiSwitchSound;

    [Header("Wave Sounds")]
    [Tooltip("Sound played during wave countdown (ticking)")]
    [SerializeField] private AudioClip waveTickSound;
    [Tooltip("Sound played when a wave ends")]
    [SerializeField] private AudioClip waveEndSound;
    [Tooltip("Sound played when the last wave finishes and portal appears")]
    [SerializeField] private AudioClip waveFinalSound;

    [Header("Spell Sounds")]
    [Tooltip("Sound played when speed spell is cast")]
    [SerializeField] private AudioClip spellSpeedSound;

    [Header("Cooking Sounds")]
    [Tooltip("Sound played when food is sizzling")]
    [SerializeField] private AudioClip cookingSizzleSound;
    [Tooltip("Sound played when food is ready")]
    [SerializeField] private AudioClip cookingReadySound;
    [Tooltip("Sound played when food is burnt")]
    [SerializeField] private AudioClip cookingBurntSound;

    [Header("Other Sounds")]
    [Tooltip("Sound played when entering a portal")]
    [SerializeField] private AudioClip portalEnterSound;
    [Tooltip("Sound played when picking up an item")]
    [SerializeField] private AudioClip pickupSound;
    [Tooltip("Sound played when reloading")]
    [SerializeField] private AudioClip reloadSound;
    [Tooltip("Sound played when shooting")]
    [SerializeField] private AudioClip shootSound;

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
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.ignoreListenerVolume = true;
        audioSource.spatialBlend = 0f; // Global 2D sound

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Optional: Inject sounds into UI buttons
    }

    #region Player Sounds
    public void PlayPlayerHitSound() => PlayOneShot(playerHitSound);
    public void PlayFootstepSound() => PlayOneShot(footstepSound);
    public void PlayJumpSound() => PlayOneShot(jumpSound);
    #endregion

    #region Enemy Sounds
    public void PlayEnemyHitSound() => PlayOneShot(enemyHitSound);
    public void PlayEnemyDefeatedSound() => PlayOneShot(enemyDefeatedSound);
    #endregion

    #region UI Sounds
    public void PlayUIHoverSound() => PlayOneShot(uiHoverSound);
    public void PlayUIClickSound() => PlayOneShot(uiClickSound);
    public void PlayUIOpenSound() => PlayOneShot(uiOpenSound);
    public void PlayUICloseSound() => PlayOneShot(uiCloseSound);
    public void PlayUIDragStartSound() => PlayOneShot(uiDragStartSound);
    public void PlayUIDropSound() => PlayOneShot(uiDropSound);
    public void PlayUISwitchSound() => PlayOneShot(uiSwitchSound);
    #endregion

    #region Wave Sounds
    public void PlayWaveTickSound() => PlayOneShot(waveTickSound);
    public void PlayWaveEndSound() => PlayOneShot(waveEndSound);
    public void PlayWaveFinalSound() => PlayOneShot(waveFinalSound);
    #endregion

    #region Spell Sounds
    public void PlaySpellSpeedSound() => PlayOneShot(spellSpeedSound);
    #endregion

    #region Cooking Sounds
    public void PlayCookingSizzleSound() => PlayOneShot(cookingSizzleSound);
    public void PlayCookingReadySound() => PlayOneShot(cookingReadySound);
    public void PlayCookingBurntSound() => PlayOneShot(cookingBurntSound);
    #endregion

    #region Other Sounds
    public void PlayPortalEnterSound() => PlayOneShot(portalEnterSound);
    public void PlayPickupSound() => PlayOneShot(pickupSound);
    public void PlayReloadSound() => PlayOneShot(reloadSound);
    public void PlayShootSound() => PlayOneShot(shootSound);
    #endregion

    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // For sounds that need volume control
    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}