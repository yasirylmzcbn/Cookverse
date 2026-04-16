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