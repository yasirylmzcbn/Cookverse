using UnityEngine;

public class PauseScript : MonoBehaviour
{
    private bool isPaused = false;
    private bool wasAnyUIOpenLastFrame = false;

    void Update()
    {
        bool isAnyUIOpen = IsAnyOtherUIOpen();

        // Toggle pause state when Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                // Prevent pausing if a UI is open, OR if it was open last frame 
                // but another script already processed ESC and closed it this frame.
                if (isAnyUIOpen || wasAnyUIOpenLastFrame)
                    return;

                PauseGame();
            }
        }

        wasAnyUIOpenLastFrame = isAnyUIOpen;
    }

    private bool IsAnyOtherUIOpen()
    {
        SpellMenuUI spellMenu = FindFirstObjectByType<SpellMenuUI>();
        if (spellMenu != null && spellMenu.menuOpen)
            return true;

        DifficultyUI difficultyUI = FindFirstObjectByType<DifficultyUI>();
        if (difficultyUI != null && difficultyUI.IsVisible())
            return true;

        return false;
    }

    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
        }

        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = true;
        }

        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;
    }

    private void OnGUI()
    {
        if (isPaused)
        {
            // Draw a 50% opaque black background over the entire screen
            Color originalColor = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            // Draw the centered text
            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 36
            };
            GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 100f), "Press ESC to unpause", style);

            // Volume Sliders
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 24
            };
            labelStyle.normal.textColor = Color.white;

            float sliderWidth = 300f;
            float sliderHeight = 30f;
            float centerX = (Screen.width - sliderWidth) / 2f;
            float centerY = Screen.height / 2f;

            // Music Volume
            GUI.Label(new Rect(centerX, centerY, sliderWidth, 30f), "Music Volume", labelStyle);
            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.MusicVolume = GUI.HorizontalSlider(new Rect(centerX, centerY + 35f, sliderWidth, sliderHeight), MusicManager.Instance.MusicVolume, 0f, 1f);
            }

            // SFX Volume (Master Volume)
            GUI.Label(new Rect(centerX, centerY + 80f, sliderWidth, 30f), "SFX Volume", labelStyle);
            AudioListener.volume = GUI.HorizontalSlider(new Rect(centerX, centerY + 115f, sliderWidth, sliderHeight), AudioListener.volume, 0f, 1f);

            GUI.color = originalColor;
        }
    }
}
