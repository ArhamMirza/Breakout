using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    private Dictionary<string, int> items = new Dictionary<string, int>();
    public Dictionary<string, Sprite> itemSprites = new Dictionary<string, Sprite>();

    public GameObject inventoryItemPrefab;  // Prefab for each inventory item
    public Transform inventoryContent;      // Container within the Scroll View

    private void Start()
    {
        LoadSprites();
    }

    // Load all sprites from the Resources/Sprites folder
    private void LoadSprites()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites");
        foreach (Sprite sprite in sprites)
        {
            itemSprites[sprite.name] = sprite;
        }
    }

    public void AddItem(string item)
    {
        if (!itemSprites.ContainsKey(item)) return;

        if (items.ContainsKey(item))
        {
            items[item]++;
        }
        else
        {
            items.Add(item, 1);
        }

        UpdateInventoryUI();
    }

    public bool HasItem(string item)
    {
        return items.ContainsKey(item) && items[item] > 0;
    }

    // Update the inventory UI by clearing it and adding each item again
    private void UpdateInventoryUI()
    {
        // Clear the current inventory UI content
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }

        // Populate the UI with updated items
        foreach (var item in items)
        {
            GameObject newItem = Instantiate(inventoryItemPrefab, inventoryContent);

            // Set item text (name and quantity)
            Text itemText = newItem.GetComponentInChildren<Text>();
            if (itemText != null)
            {
                Debug.Log(item.Value);
                itemText.text = $"{item.Key} x{item.Value}";
            }

            // Set item image (sprite)
            Image itemImage = newItem.GetComponentInChildren<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = itemSprites[item.Key];
            }
        }
    }
}
