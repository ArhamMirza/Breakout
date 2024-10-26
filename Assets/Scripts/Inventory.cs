using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Example placeholder for inventory items
    public List<string> items = new List<string>();

    public void AddItem(string item)
    {
        items.Add(item);
        Debug.Log("Item added: " + item);
    }

    public void RemoveItem(string item)
    {
        items.Remove(item);
        Debug.Log("Item removed: " + item);
    }
}
