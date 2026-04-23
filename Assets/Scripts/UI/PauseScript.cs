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

        for (int i = 1; i <= 3; i++)
        {
            float slotY = slotsStartY + (i - 1) * 55f;

            GUI.Label(new Rect(halfScreenWidth - 250f, slotY, 80f, 40f), "Slot " + i, textStyle);

            if (GUI.Button(new Rect(halfScreenWidth - 160f, slotY, 100f, 35f), "Save"))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.SaveGame(i);
            }

            if (GUI.Button(new Rect(halfScreenWidth - 50f, slotY, 100f, 35f), "Load"))
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.LoadGame(i);
            }

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

        GUI.color = originalColor;
    }

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