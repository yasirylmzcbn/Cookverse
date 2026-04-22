using UnityEngine;

public class PauseScript : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
        // Toggle pause state when Escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = true;
        }
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
                fontSize = 36,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Press ESC to unpause", style);

            GUI.color = originalColor;
        }
    }
}
