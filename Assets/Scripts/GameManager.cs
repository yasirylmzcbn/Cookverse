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
    public static GameManager Instance;
    float waveMultiplierIncrement = 1.1f;
    private int currentWave = 1;
    private int lastWave = 2;
    [SerializeField] private Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
    public event Action OnInventoryChanged;
    private Difficulty lastCompletedDifficulty = Difficulty.None;
    private Difficulty currentDifficulty = Difficulty.None;

    [Header("Save/Load")]
    [Tooltip("Add all your ItemData assets here so they can be matched when loading.")]
    [SerializeField] private List<ItemData> allItemsDatabase = new List<ItemData>();
    private readonly Dictionary<string, ItemData> _itemLookup = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);

    public Difficulty GetLastCompletedDifficulty()
    {
        return lastCompletedDifficulty;
    }

    public void SaveGame(int slot)
    {
        PlayerPrefs.SetInt($"Cookverse_Difficulty_{slot}", (int)lastCompletedDifficulty);

        List<InventoryEntry> entries = new List<InventoryEntry>();

        foreach (var pair in items)
        {
            if (pair.Key != null)
            {
                entries.Add(new InventoryEntry
                {
                    assetName = pair.Key.name,
                    itemName = pair.Key.itemName,
                    amount = pair.Value
                });
            }
        }

        string saveJson = JsonUtility.ToJson(new InventorySaveData { entries = entries });
        PlayerPrefs.SetString($"Cookverse_Inventory_{slot}", saveJson);
        PlayerPrefs.Save();
        Debug.Log($"Game Saved to Slot {slot}!");
    }

    public void LoadGame(int slot)
    {
        if (PlayerPrefs.HasKey($"Cookverse_Difficulty_{slot}"))
        {
            lastCompletedDifficulty = (Difficulty)PlayerPrefs.GetInt($"Cookverse_Difficulty_{slot}");
        }

        if (PlayerPrefs.HasKey($"Cookverse_Inventory_{slot}"))
        {
            string saveJson = PlayerPrefs.GetString($"Cookverse_Inventory_{slot}");
            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(saveJson);

            items.Clear();
            if (data != null)
            {
                RebuildItemLookup();
                List<InventoryEntry> entriesToLoad = data.entries;

                if ((entriesToLoad == null || entriesToLoad.Count == 0)
                    && data.names != null && data.amounts != null)
                {
                    entriesToLoad = new List<InventoryEntry>();
                    int legacyCount = Mathf.Min(data.names.Count, data.amounts.Count);
                    for (int i = 0; i < legacyCount; i++)
                    {
                        entriesToLoad.Add(new InventoryEntry
                        {
                            assetName = data.names[i],
                            itemName = data.names[i],
                            amount = data.amounts[i]
                        });
                    }
                }

                if (entriesToLoad != null)
                {
                    foreach (InventoryEntry entry in entriesToLoad)
                    {
                        ItemData item = ResolveItem(entry.assetName, entry.itemName);
                        if (item != null)
                        {
                            if (items.ContainsKey(item))
                                items[item] += entry.amount;
                            else
                                items[item] = entry.amount;
                        }
                        else
                        {
                            Debug.LogWarning($"[GameManager] Could not resolve saved item '{entry.assetName}' / '{entry.itemName}'.");
                        }
                    }
                }
            }
            OnInventoryChanged?.Invoke();
        }
        Debug.Log($"Game Loaded from Slot {slot}!");
    }

    [Serializable]
    private class InventorySaveData
    {
        public List<InventoryEntry> entries;
        public List<string> names;
        public List<int> amounts;
    }

    [Serializable]
    private class InventoryEntry
    {
        public string assetName;
        public string itemName;
        public int amount;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RebuildItemLookup();
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
        currentWave = 30;
        lastWave = 40;
        currentDifficulty = Difficulty.Boss;
    }

    private void RebuildItemLookup()
    {
        _itemLookup.Clear();

        foreach (ItemData item in allItemsDatabase)
        {
            RegisterItem(item);
        }

        foreach (ItemData item in Resources.LoadAll<ItemData>(""))
        {
            RegisterItem(item);
        }

        foreach (ItemData item in Resources.FindObjectsOfTypeAll<ItemData>())
        {
            RegisterItem(item);
        }

        foreach (ItemPickup pickup in FindObjectsByType<ItemPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (pickup != null)
                RegisterItem(pickup.itemData);
        }

        foreach (ItemData item in items.Keys)
        {
            RegisterItem(item);
        }
    }

    private ItemData ResolveItem(string assetName, string displayName)
    {
        if (!string.IsNullOrWhiteSpace(assetName)
            && _itemLookup.TryGetValue(assetName, out ItemData byAssetName))
        {
            return byAssetName;
        }

        if (!string.IsNullOrWhiteSpace(displayName)
            && _itemLookup.TryGetValue(displayName, out ItemData byDisplayName))
        {
            return byDisplayName;
        }

        return null;
    }

    private void RegisterItem(ItemData item)
    {
        if (item == null)
            return;

        if (!string.IsNullOrWhiteSpace(item.name) && !_itemLookup.ContainsKey(item.name))
            _itemLookup[item.name] = item;

        if (!string.IsNullOrWhiteSpace(item.itemName) && !_itemLookup.ContainsKey(item.itemName))
            _itemLookup[item.itemName] = item;
    }
}