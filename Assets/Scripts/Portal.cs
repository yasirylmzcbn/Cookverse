using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private enum ReturnOffsetDirection
    {
        Forward,
        Backward,
    }

    public string sceneToLoad;
    [Tooltip("Scene name with the kitchen.")]
    [SerializeField] private string cookingSceneName = "MainScene";

    [Tooltip("Scene name with the combat.")]
    [SerializeField] private string combatSceneName = "EnemyScene";

    [Header("Return Spawn Offset")]
    [Tooltip("How far to offset the saved return position so the player does not spawn inside this portal trigger.")]
    [SerializeField, Min(0f)] private float returnSpawnOffset = 2f;

    [Tooltip("Which direction to offset relative to this portal's forward vector.")]
    [SerializeField] private ReturnOffsetDirection returnOffsetDirection = ReturnOffsetDirection.Forward;

    [Header("Audio")]
    [SerializeField] private AudioClip enterPortalSfx;
    [SerializeField, Range(0f, 1f)] private float enterPortalSfxVolume = 1f;

    private void Start()
    {
        // If the portal has a looping ambient audio source attached to it, make sure it is global (2D) as requested.
        AudioSource ambientSource = GetComponent<AudioSource>();
        if (ambientSource != null)
        {
            ambientSource.spatialBlend = 0f; // 0 is fully 2D (Global)
        }
    }

    private Vector3 GetSafeReturnOffset()
    {
        Vector3 direction = returnOffsetDirection == ReturnOffsetDirection.Forward ? transform.forward : -transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector3.forward;

        return direction.normalized * returnSpawnOffset;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Portal triggered by: " + other.name);
        if (other.CompareTag("Player"))
        {
            PlayPortalEnterSfxGlobal();
            Transform playerTransform = null;

            // Collider may be on a child of the player, so resolve from parent.
            PlayerController playerController = other.GetComponentInParent<PlayerController>();
            if (playerController != null)
            {
                if (sceneToLoad == cookingSceneName)
                    playerController.DisableCombat();
                else if (sceneToLoad == combatSceneName)
                {
                    playerController.EnableCombat();
                }

                playerController.ResetCombatState();
            }
            Potato_Shooter shooter = other.GetComponentInParent<Potato_Shooter>();
            if (shooter == null)
            {
                Transform root = other.transform.root;
                if (root != null)
                    shooter = root.GetComponentInChildren<Potato_Shooter>(true); // include inactive
            }

            playerTransform = shooter != null ? shooter.transform.root : other.transform.root;
            if (playerTransform == null)
                playerTransform = other.transform;

            Scene activeScene = SceneManager.GetActiveScene();
            Vector3 safeReturnPosition = playerTransform.position + GetSafeReturnOffset();
            Vector3 awayFromPortal = safeReturnPosition - transform.position;
            awayFromPortal.y = 0f;
            if (awayFromPortal.sqrMagnitude < 0.0001f)
            {
                awayFromPortal = GetSafeReturnOffset();
                awayFromPortal.y = 0f;
            }

            Quaternion spawnRotation = awayFromPortal.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(awayFromPortal.normalized, Vector3.up)
                : playerTransform.rotation;

            PlayerController.RememberPositionForScene(activeScene.name, safeReturnPosition, spawnRotation);

            if (shooter != null)
            {
                Debug.Log("Shooter enabled");
                if (!string.IsNullOrEmpty(sceneToLoad) && sceneToLoad == cookingSceneName)
                    shooter.gameObject.SetActive(false);
                else if (!string.IsNullOrEmpty(sceneToLoad) && sceneToLoad == combatSceneName)
                    shooter.gameObject.SetActive(true);
            }

            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void PlayPortalEnterSfxGlobal()
    {
        if (enterPortalSfx == null)
            return;

        GameObject audioGo = new GameObject("PortalEnterSfx");
        DontDestroyOnLoad(audioGo);

        AudioSource audioSource = audioGo.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Global 2D pan
        audioSource.ignoreListenerVolume = true; // Ignore global volume muffling during transitions
        audioSource.clip = enterPortalSfx;

        float sfxVolume = SoundManager.Instance != null
            ? SoundManager.Instance.SfxVolume
            : PlayerPrefs.GetFloat(SoundManager.SFX_VOLUME_PREF_KEY, 1f);
        audioSource.volume = enterPortalSfxVolume * Mathf.Clamp01(sfxVolume);
        audioSource.Play();

        Destroy(audioGo, enterPortalSfx.length + 0.1f);
    }
}
