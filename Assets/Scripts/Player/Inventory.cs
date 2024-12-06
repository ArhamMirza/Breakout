using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public Dictionary<string, int> items = new Dictionary<string, int>();
    public Dictionary<string, Sprite> itemSprites = new Dictionary<string, Sprite>();

    public GameObject inventoryItemPrefab;  
    public Transform inventoryContent;      

    private void Start()
    {
        LoadSprites();
    }

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

    private void UpdateInventoryUI()
    {
        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in items)
        {
            GameObject newItem = Instantiate(inventoryItemPrefab, inventoryContent);

            Text itemText = newItem.GetComponentInChildren<Text>();
            if (itemText != null)
            {
                Debug.Log(item.Value);
                itemText.text = $"{item.Key} x{item.Value}";
            }

            Image itemImage = newItem.GetComponentInChildren<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = itemSprites[item.Key];
            }
        }
    }

    public List<string> SerializeInventory()
    {
        List<string> serializedItems = new List<string>();
        foreach (var item in items)
        {
            serializedItems.Add($"{item.Key}:{item.Value}");
        }
        return serializedItems;
    }

    public void DeserializeInventory(List<string> serializedItems)
    {
        items.Clear(); // Clear existing inventory
        foreach (var entry in serializedItems)
        {
            string[] parts = entry.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int count))
            {
                items[parts[0]] = count;

                // Ensure sprite for the item is loaded into the itemSprites dictionary
                if (!itemSprites.ContainsKey(parts[0]))
                {
                    // Load the sprite if it's not already loaded
                    Sprite itemSprite = Resources.Load<Sprite>("Sprites/" + parts[0]);
                    if (itemSprite != null)
                    {
                        itemSprites[parts[0]] = itemSprite;
                    }
                    else
                    {
                        Debug.LogWarning("Sprite for item " + parts[0] + " not found.");
                    }
                }
            }
        }

        // After deserialization, update the UI to reflect the changes
        UpdateInventoryUI();
    }


}
