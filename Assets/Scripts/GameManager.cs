using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Difficulty
    {
        None,
        Easy,
        Medium,
        Hard,
        Boss
    }
    // Difficulty, Inventory, and Saving will be here
    public static GameManager Instance;
    float waveMultiplierIncrement = 1.1f;
    private int currentWave = 1;
    private int lastWave = 2;
    [SerializeField] private Dictionary<ItemData, int> items = new Dictionary<ItemData, int>(); //For debug purposes, hide later
    public event Action OnInventoryChanged;
    private Difficulty lastCompletedDifficulty = Difficulty.None;
    private Difficulty currentDifficulty = Difficulty.None;

    [Header("Save/Load")]
    [Tooltip("Add all your ItemData assets here so they can be matched when loading.")]
    [SerializeField] private List<ItemData> allItemsDatabase = new List<ItemData>();

    public Difficulty GetLastCompletedDifficulty()
    {
        return lastCompletedDifficulty;
    }

    public void SaveGame()
    {
        PlayerPrefs.SetInt("Cookverse_Difficulty", (int)lastCompletedDifficulty);

        List<string> itemNames = new List<string>();
        List<int> itemAmounts = new List<int>();

        foreach (var pair in items)
        {
            if (pair.Key != null)
            {
                itemNames.Add(pair.Key.name); // Using the asset name or itemName
                itemAmounts.Add(pair.Value);
            }
        }

        string saveJson = JsonUtility.ToJson(new InventorySaveData { names = itemNames, amounts = itemAmounts });
        PlayerPrefs.SetString("Cookverse_Inventory", saveJson);
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("Cookverse_Difficulty"))
        {
            lastCompletedDifficulty = (Difficulty)PlayerPrefs.GetInt("Cookverse_Difficulty");
        }

        if (PlayerPrefs.HasKey("Cookverse_Inventory"))
        {
            string saveJson = PlayerPrefs.GetString("Cookverse_Inventory");
            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(saveJson);

            items.Clear();
            if (data != null && data.names != null && data.amounts != null)
            {
                for (int i = 0; i < data.names.Count; i++)
                {
                    ItemData item = allItemsDatabase.Find(x => x.name == data.names[i] || x.itemName == data.names[i]);
                    if (item != null)
                    {
                        items[item] = data.amounts[i];
                    }
                }
            }
            OnInventoryChanged?.Invoke();
        }
        Debug.Log("Game Loaded!");
    }

    [Serializable]
    private class InventorySaveData
    {
        public List<string> names;
        public List<int> amounts;
    }

    void Awake()
    {
        // If one already exists and it's not us then destroy the copy
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetEasyDifficulty();
    }

    public void AddInventoryItem(ItemData item, int amount = 1)
    {
        if (items.ContainsKey(item))
        {
            items[item] += amount;
        }
        else
        {
            items[item] = amount;
        }
        OnInventoryChanged?.Invoke();
        Debug.Log(GetInventoryItemsAsString());
    }

    public void RemoveInventoryItem(ItemData item, int amount = 1)
    {
        if (!items.ContainsKey(item))
        {
            return;
        }

        items[item] -= amount;

        if (items[item] <= 0)
        {
            items.Remove(item);
        }
        OnInventoryChanged?.Invoke();
        Debug.Log(GetInventoryItemsAsString());
    }

    public int GetInventoryAmount(ItemData item)
    {
        return items.TryGetValue(item, out int amount) ? amount : 0;
    }

    //Debugging
    public string GetInventoryItemsAsString()
    {
        if (items.Count == 0)
            return "Inventory is empty.";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Inventory Contents:");

        foreach (KeyValuePair<ItemData, int> pair in items)
        {
            string itemName = pair.Key != null ? pair.Key.itemName : "NULL ITEM";
            sb.AppendLine($"{itemName} x{pair.Value}");
        }

        return sb.ToString();
    }

    public IReadOnlyDictionary<ItemData, int> GetItems()
    {
        return new Dictionary<ItemData, int>(items);
    }

    public void EnsureStartingWave()
    {
        switch (currentDifficulty)
        {
            case Difficulty.None:
            case Difficulty.Easy:
                SetEasyDifficulty();
                break;

            case Difficulty.Medium:
                SetMediumDifficulty();
                break;

            case Difficulty.Hard:
                SetHardDifficulty();
                break;

            case Difficulty.Boss:
                SetBossDifficulty();
                break;
        }
    }

    public void WaveCompleted()
    {
        currentWave++;
        if (currentWave >= lastWave)
        {
            if (currentDifficulty > lastCompletedDifficulty)
            {
                lastCompletedDifficulty = currentDifficulty;
            }
        }
    }

    public float GetWaveMultiplier()
    {
        return MathF.Pow(waveMultiplierIncrement, currentWave);
    }

    public int CurrentWave()
    {
        return currentWave;
    }

    public int LastWave()
    {
        return lastWave;
    }

    public void SetEasyDifficulty()
    {
        currentWave = 1;
        lastWave = 5;
        currentDifficulty = Difficulty.Easy;
    }

    public void SetMediumDifficulty()
    {
        currentWave = 10;
        lastWave = 20;
        currentDifficulty = Difficulty.Medium;
    }

    public void SetHardDifficulty()
    {
        currentWave = 20;
        lastWave = 30;
        currentDifficulty = Difficulty.Hard;
    }

    public void SetBossDifficulty()
    {
        //add a boss bool to spawn in one wave and then turn it off
        currentWave = 30;
        lastWave = 40;
        currentDifficulty = Difficulty.Boss;
    }
}