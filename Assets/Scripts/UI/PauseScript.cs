using UnityEngine;

public class PauseScript : MonoBehaviour
{
    private bool isPaused = false;
    private bool wasAnyUIOpenLastFrame = false;

    void Update()
    {
        bool isAnyUIOpen = IsAnyOtherUIOpen();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
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

    private int lastHoveredButtonId = -1;

    private void PlayPauseHoverSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIHoverSound();
            return;
        }

        if (UISoundManager.Instance != null)
            UISoundManager.Instance.PlayHoverSound();
    }

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

        // Play open menu sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIOpenSound();
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

        // Play close menu sound
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUICloseSound();
    }

    private void OnGUI()
    {
        if (!isPaused) return;

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

        AudioListener.volume =
            GUI.HorizontalSlider(new Rect(centerX, sliderBlockY + 70f, sliderWidth, sliderHeight),
            AudioListener.volume, 0f, 1f);

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

        for (int i = 1; i <= 3; i++)
        {
            float slotY = slotsStartY + (i - 1) * 55f;

            GUI.Label(new Rect(halfScreenWidth - 250f, slotY, 80f, 40f), "Slot " + i, textStyle);

            GUI.enabled = !saveLoadDisabled;
            Rect saveRect = new Rect(halfScreenWidth - 160f, slotY, 100f, 35f);
            if (!saveLoadDisabled && saveRect.Contains(Event.current.mousePosition)) currentHoveredId = i * 10;
            if (GUI.Button(saveRect, "Save"))
            {
                if (UISoundManager.Instance != null) UISoundManager.Instance.PlayClickSound();
                if (GameManager.Instance != null)
                    GameManager.Instance.SaveGame(i);
            }

            Rect loadRect = new Rect(halfScreenWidth - 50f, slotY, 100f, 35f);
            if (!saveLoadDisabled && loadRect.Contains(Event.current.mousePosition)) currentHoveredId = i * 10 + 1;
            if (GUI.Button(loadRect, "Load"))
            {
                if (UISoundManager.Instance != null) UISoundManager.Instance.PlayClickSound();
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

        if (currentHoveredId != lastHoveredButtonId)
        {
            if (currentHoveredId != -1)
                PlayPauseHoverSound();
            lastHoveredButtonId = currentHoveredId;
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