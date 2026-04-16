using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyUI : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraController firstPersonCameraController;

    [Header("Panels")]
    [SerializeField] private GameObject difficultyPanel;

    [Header("Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;
    [SerializeField] private Button bossButton;

    private bool storedVisible;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        easyButton.onClick.AddListener(OnEasyButtonClicked);
        SetMenuVisible(false);
        mediumButton.interactable = false;
        hardButton.interactable = false;
        bossButton.interactable = false;
    }

    public void SetMenuVisible(bool visible)
    {
        if (difficultyPanel != null)
            difficultyPanel.SetActive(visible);

        if (visible)
        {
            // Disable player movement
            if (playerController != null)
                playerController.enabled = false;

            // Disable camera look
            if (firstPersonCameraController != null)
                firstPersonCameraController.enabled = false;

            // Unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Re-enable player movement
            if (playerController != null)
                playerController.enabled = true;

            // Re-enable camera
            if (firstPersonCameraController != null)
                firstPersonCameraController.enabled = true;

            // Lock cursor back
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        storedVisible = visible;
    }

    void OnEasyButtonClicked()
    {
        Debug.Log("Easy Clicked");

        SetMenuVisible(false);
        playerController.EndInteraction();
        GameManager.Instance.SetEasyDifficulty();
    }

    void OnMediumButtonClicked()
    {
        SetMenuVisible(false);
        playerController.EndInteraction();
        GameManager.Instance.SetMediumDifficulty();
    }

    void OnHardButtonClicked()
    {
        SetMenuVisible(false);
        playerController.EndInteraction();
        GameManager.Instance.SetHardDifficulty();
    }

    void OnBossButtonClicked()
    {
        SetMenuVisible(false);
        playerController.EndInteraction();
        GameManager.Instance.SetBossDifficulty();
    }

    public bool IsVisible()
    {
        return storedVisible;
    }

    public void UnlockDifficulty(GameManager.Difficulty difficulty)
    {
        //Easy is unlocked on start
        if (difficulty >= GameManager.Difficulty.Medium)
        {
            mediumButton.interactable = true;
        }
        if (difficulty >= GameManager.Difficulty.Hard)
        {
            hardButton.interactable = true;
        }
        if (difficulty >= GameManager.Difficulty.Boss)
        {
            bossButton.interactable = true;
        }
    }
}
