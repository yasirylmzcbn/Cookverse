using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private Dictionary<ItemData, int> items = new Dictionary<ItemData, int>(); //For debug purposes, remove later
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void AddItem(ItemData item, int amount = 1)
    {
        if (items.ContainsKey(item))
        {
            items[item] += amount;
        }
        else
        {
            items[item] = amount;
        }
    }

    public void RemoveItem(ItemData item, int amount = 1)
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
    }

    public int GetAmount(ItemData item)
    {
        return items.TryGetValue(item, out int amount) ? amount : 0;
    }

    //Debugging
    public string GetAllItemsAsString()
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
}