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
}
