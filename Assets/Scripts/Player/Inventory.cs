using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Inventory item list
    private List<string> items = new List<string>();

    public void AddItem(string item)
    {
        
        items.Add(item);
        Debug.Log("Item added: " + item);
       
    }

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

    public bool HasItem(string item)
    {
        return items.Contains(item);
    }

    public int GetItemCount()
    {
        return items.Count;
    }

    public void ClearInventory()
    {
        items.Clear();
        Debug.Log("Inventory cleared.");
    }
}
