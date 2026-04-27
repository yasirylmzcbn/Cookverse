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

    [Header("Initial Volume Sliders")]
    [SerializeField, Range(0f, 1f)] private float initialMusicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float initialSfxVolume = 1f;

    [Header("Pause UI Calibration")]
    [Tooltip("Inspector-only scale for pause menu buttons and other interactive controls. This is not an in-game setting.")]
    [SerializeField, Range(0.5f, 2f)] private float pauseUiButtonScale = 1f;
    [Tooltip("Inspector-only scale for the distance between pause menu elements. This is not an in-game setting.")]
    [SerializeField, Range(0.5f, 2f)] private float pauseUiSpacingScale = 1f;
    [Tooltip("Inspector-only scale for horizontal spacing between pause menu buttons. This is not an in-game setting.")]
    [SerializeField, Range(0.5f, 3f)] private float pauseUiHorizontalSpacingScale = 1f;
    [Tooltip("Inspector-only scale for pause menu text sizes. This is not an in-game setting.")]
    [SerializeField, Range(0.5f, 2f)] private float pauseUiTextScale = 1f;

    private bool isTutorialOpen = false;
    private int tutorialSlideIndex = 0;
    private float currentMusicVolume;
    private float currentSfxVolume;

    private void Start()
    {
        currentMusicVolume = Mathf.Clamp01(initialMusicVolume);
        currentSfxVolume = Mathf.Clamp01(initialSfxVolume);
        ApplyVolumeSettings();
    }

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

        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), currentSlide, ScaleMode.ScaleAndCrop, true);
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
            fontSize = Mathf.RoundToInt(36f * pauseUiTextScale)
        };

        GUI.Label(
            new Rect(0, Screen.height * 0.08f, Screen.width, 80f),
            "Press ESC to resume",
            titleStyle
        );

        float baseSliderWidth = 300f;
        float baseSliderHeight = 30f;
        float sliderWidth = baseSliderWidth * pauseUiButtonScale;
        float sliderHeight = baseSliderHeight * pauseUiButtonScale;
        float centerX = (Screen.width - sliderWidth) / 2f;
        float sliderBlockY = Screen.height * 0.38f;
        float sliderSpacing = 50f * pauseUiSpacingScale;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = Mathf.RoundToInt(24f * pauseUiTextScale),
            normal = { textColor = Color.white }
        };
        labelStyle.hover.textColor = labelStyle.normal.textColor;
        labelStyle.active.textColor = labelStyle.normal.textColor;
        labelStyle.focused.textColor = labelStyle.normal.textColor;

        GUI.Label(new Rect(centerX, sliderBlockY - sliderSpacing, sliderWidth, 30f), "Music Volume", labelStyle);

        currentMusicVolume = GUI.HorizontalSlider(
            new Rect(centerX, sliderBlockY - 10f, sliderWidth, sliderHeight),
            currentMusicVolume,
            0f,
            1f
        );

        GUI.Label(new Rect(centerX, sliderBlockY + sliderSpacing - 10f, sliderWidth, 30f), "SFX Volume", labelStyle);

        currentSfxVolume = GUI.HorizontalSlider(
            new Rect(centerX, sliderBlockY + sliderSpacing + 20f, sliderWidth, sliderHeight),
            currentSfxVolume,
            0f,
            1f
        );

        ApplyVolumeSettings();

        float halfScreenWidth = Screen.width / 2f;

        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(20f * pauseUiTextScale),
            normal = { textColor = Color.white }
        };

        GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = Mathf.RoundToInt(16f * pauseUiTextScale),
            normal = { textColor = Color.lightGray }
        };

        float slotsStartY = Screen.height * 0.65f;
        float slotSpacing = 55f * pauseUiSpacingScale;
        float horizontalSpacing = 10f * pauseUiHorizontalSpacingScale;
        bool combatActive = PlayerController.Instance != null && PlayerController.Instance.IsCombatEnabled;
        SwitchCamera switchCamera = FindFirstObjectByType<SwitchCamera>();
        bool cookingActive = switchCamera != null && switchCamera.IsInKitchenCamera;
        bool saveLoadDisabled = combatActive || cookingActive;
        int currentHoveredId = -1;

        float menuButtonWidth = 150f * pauseUiButtonScale;
        float menuButtonHeight = 36f * pauseUiButtonScale;

        Rect mainMenuRect = new Rect(20f, 20f, menuButtonWidth, menuButtonHeight);
        if (mainMenuRect.Contains(Event.current.mousePosition))
            currentHoveredId = 2001;
        if (GUI.Button(mainMenuRect, "Main Menu"))
        {
            PlayPauseSound(hover: false);
            ReturnToMainMenu();
            GUI.color = originalColor;
            return;
        }

        Rect tutorialRect = new Rect(Screen.width - horizontalSpacing - menuButtonWidth, 20f, menuButtonWidth, menuButtonHeight);
        if (tutorialRect.Contains(Event.current.mousePosition))
            currentHoveredId = 2002;
        if (GUI.Button(tutorialRect, tutorialButtonLabel))
        {
            PlayPauseSound(hover: false);
            OpenTutorialFromPause();
        }

        float saveButtonWidth = 100f * pauseUiButtonScale;
        float saveButtonHeight = 35f * pauseUiButtonScale;
        float saveButtonGap = horizontalSpacing * pauseUiSpacingScale;
        float slotLabelWidth = 80f * pauseUiTextScale;

        for (int i = 1; i <= 3; i++)
        {
            float slotY = slotsStartY + (i - 1) * slotSpacing;

            GUI.Label(new Rect(halfScreenWidth - 250f, slotY, slotLabelWidth, 40f), "Slot " + i, textStyle);

            GUI.enabled = !saveLoadDisabled;
            Rect saveRect = new Rect(halfScreenWidth - 160f, slotY, saveButtonWidth, saveButtonHeight);
            if (!saveLoadDisabled && saveRect.Contains(Event.current.mousePosition)) currentHoveredId = i * 10;
            if (GUI.Button(saveRect, "Save"))
            {
                PlayPauseSound(hover: false);
                if (GameManager.Instance != null)
                    GameManager.Instance.SaveGame(i);
            }

            Rect loadRect = new Rect(saveRect.xMax + saveButtonGap, slotY, saveButtonWidth, saveButtonHeight);
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
                fontSize = Mathf.RoundToInt(16f * pauseUiTextScale),
                normal = { textColor = new Color(1f, 0.8f, 0.4f) }
            };

            string reasonText = combatActive && cookingActive
                ? "Save/Load disabled during combat and cooking"
                : combatActive
                    ? "Save/Load disabled during combat"
                    : "Save/Load disabled while cooking";

            GUI.Label(
                new Rect(halfScreenWidth - 250f, slotsStartY - 35f * pauseUiSpacingScale, 500f, 25f),
                reasonText,
                combatInfoStyle
            );
        }

        GUI.color = originalColor;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DrawUnlockAllTestingButton(ref currentHoveredId, pauseUiButtonScale);
#endif

        if (currentHoveredId != lastHoveredButtonId)
        {
            if (currentHoveredId != -1)
                PlayPauseSound(hover: true);
            lastHoveredButtonId = currentHoveredId;
        }
    }

    private void ApplyVolumeSettings()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.MusicVolume = currentMusicVolume;

        if (SoundManager.Instance != null)
            SoundManager.Instance.SfxVolume = currentSfxVolume;

        if (UISoundManager.Instance != null)
            UISoundManager.Instance.SfxVolume = currentSfxVolume;

        // Keep global listener volume in sync for any one-off SFX paths not using the managers.
        AudioListener.volume = currentSfxVolume;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void DrawUnlockAllTestingButton(ref int currentHoveredId, float buttonScale)
    {
        float buttonWidth = 150f * buttonScale;
        float buttonHeight = 40f * buttonScale;
        float margin = 20f * buttonScale;

        Rect unlockAllRect = new Rect(margin, Screen.height - buttonHeight - margin, buttonWidth, buttonHeight);
        if (unlockAllRect.Contains(Event.current.mousePosition))
            currentHoveredId = 999;

        if (GUI.Button(unlockAllRect, "Unlock All"))
        {
            PlayPauseSound(hover: false);

            if (PlayerRecipeUnlocks.Instance != null)
                PlayerRecipeUnlocks.Instance.UnlockAllRecipesForTesting();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnlockAllDifficultiesForTesting();

                // Give 90 of each ingredient
                ItemData[] allItems = Resources.FindObjectsOfTypeAll<ItemData>();
                foreach (ItemData item in allItems)
                {
                    if (item != null)
                        GameManager.Instance.AddInventoryItem(item, 90);
                }
            }

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