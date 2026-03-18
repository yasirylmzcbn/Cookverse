using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    private PlayerController _playerController;
    private Potato_Shooter _potatoShooter;
    [SerializeField] private QuestManager questManager;
    private QuestManager _questManager;

    [Header("Runtime")]
    [Tooltip("How often to retry finding player/shooter references when missing.")]
    [SerializeField] private float resolveRetryIntervalSeconds = 0.25f;
    private float _nextResolveTime;

    private void OnEnable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        _nextResolveTime = 0f;
        ResolveReferences();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Player may be recreated/changed depending on scene setup.
        _playerController = null;
        _potatoShooter = null;
        _nextResolveTime = 0f;
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (_playerController == null)
        {
            _playerController = playerController != null
                ? playerController
                : (PlayerController.Instance != null ? PlayerController.Instance : FindFirstObjectByType<PlayerController>());

            playerController = _playerController;
        }

        if (_potatoShooter == null && _playerController != null)
            _potatoShooter = _playerController.GetComponentInChildren<Potato_Shooter>(true);

        if (_questManager == null)
        {
            _questManager = questManager != null ? questManager : QuestManager.Instance;
            if (_questManager == null)
                _questManager = FindFirstObjectByType<QuestManager>();

            questManager = _questManager;
        }
    }

    [Header("Health UI")]
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Ammo UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Quests UI")]
    [SerializeField] private TextMeshProUGUI questText;

    [Header("UI Transparency")]
    [SerializeField, Range(0f, 1f)] private float dimmedAlpha = 0.3f;

    void Start()
    {
        ResolveReferences();
    }

    void Update()
    {
        if ((_playerController == null || _potatoShooter == null) && Time.unscaledTime >= _nextResolveTime)
        {
            _nextResolveTime = Time.unscaledTime + Mathf.Max(0.05f, resolveRetryIntervalSeconds);
            ResolveReferences();
        }

        if (_playerController != null)
        {
            if (healthBarSlider != null)
            {
                healthBarSlider.maxValue = _playerController.maxHealth;
                healthBarSlider.value = _playerController.currentHealth;
            }

            if (healthText != null)
                healthText.text = _playerController.currentHealth + " / " + _playerController.maxHealth;
        }

        if (_potatoShooter != null && ammoText != null)
        {
            ammoText.text = _potatoShooter.ammo + " / " + _potatoShooter.initialAmmo;

            // more transparent text during reload
            var c = ammoText.color;
            c.a = (_potatoShooter.ammo <= 0 || _potatoShooter.IsReloading) ? dimmedAlpha : 1f;
            ammoText.color = c;
        }

        if (questText != null)
        {
            if (_questManager != null)
                questText.text = _questManager.GetQuestDisplayText();
            else
                questText.text = "> No active quest";

            var qc = questText.color;
            qc.a = (_questManager != null && _questManager.IsCompleted) ? dimmedAlpha : 1f;
            questText.color = qc;
        }
    }
}
