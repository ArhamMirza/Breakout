using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private Dictionary<string, int> items = new Dictionary<string, int>();

    public GameObject inventoryItemPrefab;

    public Transform inventoryContent;

    public void AddItem(string item)
    {
        if (items.ContainsKey(item))
        {
            items[item]++;
        }
        else
        {
            items.Add(item, 1);
        }

        Debug.Log("Item added: " + item);
        UpdateInventoryUI(); // Update the UI whenever an item is added
    }

    public void RemoveItem(string item)
    {
        if (items.ContainsKey(item))
        {
            items[item]--;
            if (items[item] <= 0)
            {
                items.Remove(item);
            }
            Debug.Log("Item removed: " + item);
            UpdateInventoryUI(); // Update the UI whenever an item is removed
        }
        else
        {
            Debug.Log("Item not found in inventory: " + item);
        }
    }

    public bool HasItem(string item)
    {
        return items.ContainsKey(item) && items[item] > 0;
    }

    public int GetItemCount(string item)
    {
        if (items.ContainsKey(item))
        {
            return items[item];
        }
        return 0;
    }

    public int GetTotalUniqueItems()
    {
        return items.Count;
    }

    public void ClearInventory()
    {
        items.Clear();
        Debug.Log("Inventory cleared.");
        UpdateInventoryUI(); // Update the UI when the inventory is cleared
    }

    public void PrintInventory()
    {
        if (items.Count == 0)
        {
            Debug.Log("Inventory is empty.");
        }
        else
        {
            foreach (var item in items)
            {
                Debug.Log("Item: " + item.Key + ", Count: " + item.Value);
            }
        }
    }

    // Method to update the scrollable inventory UI
    private void UpdateInventoryUI()
    {
        // Clear existing UI elements in the inventory content area
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }

        // Populate the inventory UI with updated items
        foreach (var item in items)
        {
            GameObject newItem = Instantiate(inventoryItemPrefab, inventoryContent);
            Text itemText = newItem.GetComponentInChildren<Text>();
            itemText.text = $"{item.Key} x{item.Value}";
        }
    }
}
