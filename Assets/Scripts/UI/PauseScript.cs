using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    private bool isPaused = false;

    [Header("Pause Buttons")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Tutorial")]
    [Tooltip("Add tutorial PNG textures here. You can add/remove list elements as needed.")]
    [SerializeField] private List<Texture2D> tutorialSlides = new List<Texture2D>();
    [SerializeField] private string tutorialButtonLabel = "Tutorial";

    private bool isTutorialOpen = false;
    private int tutorialSlideIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (isTutorialOpen)
                {
                    CloseTutorial();
                    return;
                }

                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (isPaused && isTutorialOpen)
        {
            if (Input.GetMouseButtonDown(0)
                || Input.GetKeyDown(KeyCode.Space)
                || Input.GetKeyDown(KeyCode.Return))
            {
                AdvanceTutorial();
            }
        }
    }

    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private int lastHoveredButtonId = -1;

    private void PlayPauseSound(bool hover)
    {
        if (hover)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayUIHoverSound();
                return;
            }

            if (UISoundManager.Instance != null)
                UISoundManager.Instance.PlayHoverSound();
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIClickSound();
            return;
        }

        if (UISoundManager.Instance != null)
            UISoundManager.Instance.PlayClickSound();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        DifficultyUI difficultyUI = FindFirstObjectByType<DifficultyUI>(FindObjectsInactive.Include);
        if (difficultyUI != null)
            difficultyUI.SetPausedState(true);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.enabled = false;
        }

        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Play open menu sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIOpenSound();
    }

    public void ResumeGame()
    {
        isTutorialOpen = false;
        tutorialSlideIndex = 0;

        DifficultyUI difficultyUI = FindFirstObjectByType<DifficultyUI>(FindObjectsInactive.Include);
        if (difficultyUI != null)
            difficultyUI.SetPausedState(false);

        isPaused = false;
        Time.timeScale = 1f;

        bool difficultyVisible = difficultyUI != null && difficultyUI.IsVisible();

        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = !difficultyVisible;

        if (difficultyVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = previousLockState;
            Cursor.visible = previousCursorVisible;
        }

        // Play close menu sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUICloseSound();
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerController.ClearRememberedScenePositions();

        if (PlayerController.Instance != null)
            Destroy(PlayerController.Instance.gameObject);

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        Debug.LogWarning("[PauseScript] Main menu scene name is empty.");
    }

    private void OpenTutorialFromPause()
    {
        if (tutorialSlides == null || tutorialSlides.Count <= 0)
        {
            Debug.LogWarning("[PauseScript] No tutorial slides assigned.");
            return;
        }

        isTutorialOpen = true;
        tutorialSlideIndex = 0;
    }

    private void CloseTutorial()
    {
        isTutorialOpen = false;
        tutorialSlideIndex = 0;
    }

    private void AdvanceTutorial()
    {
        if (tutorialSlides == null || tutorialSlides.Count <= 0)
        {
            CloseTutorial();
            return;
        }

        tutorialSlideIndex++;
        if (tutorialSlideIndex >= tutorialSlides.Count)
            CloseTutorial();
    }

    private void DrawTutorialOverlay()
    {
        Texture2D currentSlide = null;
        if (tutorialSlides != null && tutorialSlideIndex >= 0 && tutorialSlideIndex < tutorialSlides.Count)
            currentSlide = tutorialSlides[tutorialSlideIndex];

        if (currentSlide == null)
        {
            CloseTutorial();
            return;
        }

        GUI.color = new Color(0f, 0f, 0f, 0.9f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float slideWidth = currentSlide.width;
        float slideHeight = currentSlide.height;
        float maxWidth = Screen.width - 120f;
        float maxHeight = Screen.height - 180f;
        float scale = Mathf.Min(maxWidth / slideWidth, maxHeight / slideHeight);
        scale = Mathf.Max(0.01f, scale);

        float drawWidth = slideWidth * scale;
        float drawHeight = slideHeight * scale;
        Rect drawRect = new Rect((Screen.width - drawWidth) * 0.5f, (Screen.height - drawHeight) * 0.5f - 20f, drawWidth, drawHeight);

        GUI.DrawTexture(drawRect, currentSlide, ScaleMode.ScaleToFit, true);

        GUIStyle captionStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            normal = { textColor = Color.white }
        };

        string caption = $"Slide {tutorialSlideIndex + 1} / {tutorialSlides.Count} - Click, Space, or Enter to continue (Esc to close)";
        GUI.Label(new Rect(0f, Screen.height - 70f, Screen.width, 40f), caption, captionStyle);
    }

    private void OnGUI()
    {
        if (!isPaused) return;

        if (isTutorialOpen)
        {
            DrawTutorialOverlay();
            return;
        }

        Color originalColor = GUI.color;

        GUI.color = new Color(0, 0, 0, 0.5f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

        GUI.color = Color.white;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 36
        };

        GUI.Label(
            new Rect(0, Screen.height * 0.08f, Screen.width, 80f),
            "Press ESC to resume",
            titleStyle
        );

        float sliderWidth = 300f;
        float sliderHeight = 30f;
        float centerX = (Screen.width - sliderWidth) / 2f;
        float sliderBlockY = Screen.height * 0.38f;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 24,
            normal = { textColor = Color.white }
        };

        GUI.Label(new Rect(centerX, sliderBlockY - 40f, sliderWidth, 30f), "Music Volume", labelStyle);

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.MusicVolume =
                GUI.HorizontalSlider(new Rect(centerX, sliderBlockY - 10f, sliderWidth, sliderHeight),
                MusicManager.Instance.MusicVolume, 0f, 1f);
        }

        GUI.Label(new Rect(centerX, sliderBlockY + 40f, sliderWidth, 30f), "SFX Volume", labelStyle);

        float currentSfxVolume = 1f;
        if (SoundManager.Instance != null)
            currentSfxVolume = SoundManager.Instance.SfxVolume;
        else if (UISoundManager.Instance != null)
            currentSfxVolume = UISoundManager.Instance.SfxVolume;
        else
            currentSfxVolume = AudioListener.volume;

        float newSfxVolume = GUI.HorizontalSlider(
            new Rect(centerX, sliderBlockY + 70f, sliderWidth, sliderHeight),
            currentSfxVolume, 0f, 1f);

        if (SoundManager.Instance != null)
            SoundManager.Instance.SfxVolume = newSfxVolume;

        if (UISoundManager.Instance != null)
            UISoundManager.Instance.SfxVolume = newSfxVolume;

        // Keep global listener volume in sync for any one-off SFX paths not using the managers.
        AudioListener.volume = newSfxVolume;

        float halfScreenWidth = Screen.width / 2f;

        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            normal = { textColor = Color.white }
        };

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 16,
            normal = { textColor = Color.lightGray }
        };

        float slotsStartY = Screen.height * 0.65f;
        bool combatActive = PlayerController.Instance != null && PlayerController.Instance.IsCombatEnabled;
        SwitchCamera switchCamera = FindFirstObjectByType<SwitchCamera>();
        bool cookingActive = switchCamera != null && switchCamera.IsInKitchenCamera;
        bool saveLoadDisabled = combatActive || cookingActive;
        int currentHoveredId = -1;

        Rect mainMenuRect = new Rect(20f, 20f, 150f, 36f);
        if (mainMenuRect.Contains(Event.current.mousePosition))
            currentHoveredId = 2001;
        if (GUI.Button(mainMenuRect, "Main Menu"))
        {
            PlayPauseSound(hover: false);
            ReturnToMainMenu();
            GUI.color = originalColor;
            return;
        }

        Rect tutorialRect = new Rect(Screen.width - 170f, 20f, 150f, 36f);
        if (tutorialRect.Contains(Event.current.mousePosition))
            currentHoveredId = 2002;
        if (GUI.Button(tutorialRect, tutorialButtonLabel))
        {
            PlayPauseSound(hover: false);
            OpenTutorialFromPause();
        }

        for (int i = 1; i <= 3; i++)
        {
            float slotY = slotsStartY + (i - 1) * 55f;

            GUI.Label(new Rect(halfScreenWidth - 250f, slotY, 80f, 40f), "Slot " + i, textStyle);

            GUI.enabled = !saveLoadDisabled;
            Rect saveRect = new Rect(halfScreenWidth - 160f, slotY, 100f, 35f);
            if (!saveLoadDisabled && saveRect.Contains(Event.current.mousePosition)) currentHoveredId = i * 10;
            if (GUI.Button(saveRect, "Save"))
            {
                PlayPauseSound(hover: false);
                if (GameManager.Instance != null)
                    GameManager.Instance.SaveGame(i);
            }

            Rect loadRect = new Rect(halfScreenWidth - 50f, slotY, 100f, 35f);
            if (!saveLoadDisabled && loadRect.Contains(Event.current.mousePosition)) currentHoveredId = i * 10 + 1;
            if (GUI.Button(loadRect, "Load"))
            {
                PlayPauseSound(hover: false);
                if (GameManager.Instance != null)
                    GameManager.Instance.LoadGame(i);
            }
            GUI.enabled = true;

            string infoText = "Empty";

            if (PlayerPrefs.HasKey($"Cookverse_Difficulty_{i}"))
            {
                GameManager.Difficulty diff =
                    (GameManager.Difficulty)PlayerPrefs.GetInt($"Cookverse_Difficulty_{i}");

                int totalItems = 0;
                string invJson = PlayerPrefs.GetString($"Cookverse_Inventory_{i}", "");

                if (!string.IsNullOrEmpty(invJson))
                {
                    try
                    {
                        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(invJson);
                        if (data != null)
                        {
                            if (data.entries != null)
                            {
                                foreach (InventoryEntry entry in data.entries)
                                    totalItems += entry.amount;
                            }
                            else if (data.amounts != null)
                            {
                                foreach (int amt in data.amounts)
                                    totalItems += amt;
                            }
                        }
                    }
                    catch { }
                }

                infoText = $"Diff: {diff} | Items: {totalItems}";
            }

            GUI.Label(new Rect(halfScreenWidth + 60f, slotY, 200f, 40f), infoText, infoStyle);
        }

        if (saveLoadDisabled)
        {
            GUIStyle combatInfoStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                normal = { textColor = new Color(1f, 0.8f, 0.4f) }
            };

            string reasonText = combatActive && cookingActive
                ? "Save/Load disabled during combat and cooking"
                : combatActive
                    ? "Save/Load disabled during combat"
                    : "Save/Load disabled while cooking";

            GUI.Label(
                new Rect(halfScreenWidth - 250f, slotsStartY - 35f, 500f, 25f),
                reasonText,
                combatInfoStyle
            );
        }

        GUI.color = originalColor;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DrawUnlockAllTestingButton(ref currentHoveredId);
#endif

        if (currentHoveredId != lastHoveredButtonId)
        {
            if (currentHoveredId != -1)
                PlayPauseSound(hover: true);
            lastHoveredButtonId = currentHoveredId;
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void DrawUnlockAllTestingButton(ref int currentHoveredId)
    {
        const float buttonWidth = 150f;
        const float buttonHeight = 40f;
        const float margin = 20f;

        Rect unlockAllRect = new Rect(margin, Screen.height - buttonHeight - margin, buttonWidth, buttonHeight);
        if (unlockAllRect.Contains(Event.current.mousePosition))
            currentHoveredId = 999;

        if (GUI.Button(unlockAllRect, "Unlock All"))
        {
            PlayPauseSound(hover: false);

            if (PlayerRecipeUnlocks.Instance != null)
                PlayerRecipeUnlocks.Instance.UnlockAllRecipesForTesting();

            if (GameManager.Instance != null)
                GameManager.Instance.UnlockAllDifficultiesForTesting();

            DifficultyUI difficultyUI = FindFirstObjectByType<DifficultyUI>(FindObjectsInactive.Include);
            if (difficultyUI != null)
                difficultyUI.UnlockDifficulty(GameManager.Difficulty.Boss);
        }
    }
#endif

    [System.Serializable]
    private class InventorySaveData
    {
        public System.Collections.Generic.List<InventoryEntry> entries;
        public System.Collections.Generic.List<string> names;
        public System.Collections.Generic.List<int> amounts;
    }

    [System.Serializable]
    private class InventoryEntry
    {
        public string assetName;
        public string itemName;
        public int amount;
    }
}