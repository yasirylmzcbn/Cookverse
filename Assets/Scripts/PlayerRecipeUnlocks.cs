using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRecipeUnlocks : MonoBehaviour
{
    [Header("Starting unlocks")]
    [SerializeField] private List<Recipe> unlockedOnStart = new List<Recipe>();

    [Header("Persistence")]
    [SerializeField] private bool persistToPlayerPrefs = true;
    [SerializeField] private string playerPrefsKey = "cookverse.unlockedRecipes";

    private readonly HashSet<Recipe> _unlocked = new HashSet<Recipe>();

    public event Action<Recipe> RecipeUnlocked;

    private void Awake()
    {
        _unlocked.Clear();

        if (persistToPlayerPrefs)
            LoadFromPrefs();

        for (int i = 0; i < unlockedOnStart.Count; i++)
            _unlocked.Add(unlockedOnStart[i]);
    }

    public bool IsUnlocked(Recipe recipe) => _unlocked.Contains(recipe);

    public bool Unlock(Recipe recipe)
    {
        if (!_unlocked.Add(recipe))
            return false;

        if (persistToPlayerPrefs)
            SaveToPrefs();

        RecipeUnlocked?.Invoke(recipe);
        return true;
    }

    public void ClearAllUnlocks()
    {
        _unlocked.Clear();
        if (persistToPlayerPrefs)
            PlayerPrefs.DeleteKey(playerPrefsKey);
    }

    private void SaveToPrefs()
    {
        // Store as comma-separated ints.
        List<int> values = new List<int>(_unlocked.Count);
        foreach (Recipe r in _unlocked)
            values.Add((int)r);

        string serialized = string.Join(",", values);
        PlayerPrefs.SetString(playerPrefsKey, serialized);
        PlayerPrefs.Save();
    }

    private void LoadFromPrefs()
    {
        if (!PlayerPrefs.HasKey(playerPrefsKey))
            return;

        string serialized = PlayerPrefs.GetString(playerPrefsKey, "");
        if (string.IsNullOrWhiteSpace(serialized))
            return;

        string[] parts = serialized.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], out int value))
                continue;

            if (!Enum.IsDefined(typeof(Recipe), value))
                continue;

            _unlocked.Add((Recipe)value);
        }
    }
}
