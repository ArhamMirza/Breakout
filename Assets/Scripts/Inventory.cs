using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Inventory item list
    public List<string> items = new List<string>();

    // Optional: set a maximum inventory capacity
    public int maxCapacity = 10;

    // Add an item to the inventory if there's space
    public void AddItem(string item)
    {
        if (items.Count < maxCapacity)
        {
            items.Add(item);
            Debug.Log("Item added: " + item);
        }
        else
        {
            Debug.Log("Inventory is full! Cannot add: " + item);
        }
    }

    // Remove an item from the inventory
    public void RemoveItem(string item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log("Item removed: " + item);
        }
        else
        {
            Debug.Log("Item not found in inventory: " + item);
        }
    }

    // Check if the inventory has a specific item
    public bool HasItem(string item)
    {
        return items.Contains(item);
    }

    // Get the number of items in the inventory
    public int GetItemCount()
    {
        return items.Count;
    }

    // Clear all items from the inventory
    public void ClearInventory()
    {
        items.Clear();
        Debug.Log("Inventory cleared.");
    }
}
