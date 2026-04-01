using System;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // Difficulty, Inventory, and Saving will be here
    public static GameManager Instance;
    float waveMultiplier = 1;
    float waveMultiplierIncrement = 1.1f;
    [SerializeField] private Dictionary<ItemData, int> items = new Dictionary<ItemData, int>(); //For debug purposes, hide later
    public event Action OnInventoryChanged;
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

    public void WaveCompleted()
    {
        waveMultiplier *= waveMultiplierIncrement;
    }

    public float GetWaveMultiplier()
    {
        return waveMultiplier;
    }
}