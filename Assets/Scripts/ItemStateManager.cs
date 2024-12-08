using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemStateManager : MonoBehaviour
{
    // Singleton instance
    public static ItemStateManager Instance;

    // Dictionary to store destroyed items by scene
    private Dictionary<string, HashSet<string>> destroyedItemsByScene;
    private GameSceneManager gameSceneManager;

   private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }

    // Initialize the dictionary
    if (destroyedItemsByScene == null)
    {
        destroyedItemsByScene = new Dictionary<string, HashSet<string>>();
    }


}


    // Add an item to the destroyed list for the current scene
    public void MarkItemAsDestroyed(string itemId)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Ensure currentScene is valid (shouldn't be null or empty)
        if (string.IsNullOrEmpty(currentScene))
        {
            Debug.LogError("Current scene name is invalid.");
            return;
        }

        // If the scene doesn't exist in the dictionary, add it
        if (!destroyedItemsByScene.ContainsKey(currentScene))
        {
            destroyedItemsByScene[currentScene] = new HashSet<string>();
        }

        // Add the itemId to the set of destroyed items for the current scene
        destroyedItemsByScene[currentScene].Add(itemId);
        Debug.Log("Marked item as destroyed: " + itemId + " in scene: " + currentScene);
    }

    // Check if an item has been destroyed in the current scene
    public bool IsItemDestroyed(string itemId)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (string.IsNullOrEmpty(currentScene))
        {
            Debug.LogError("Current scene name is invalid.");
            return false;
        }

        if (destroyedItemsByScene.ContainsKey(currentScene))
        {
            return destroyedItemsByScene[currentScene].Contains(itemId);
        }
        return false;
    }

    // Clear all destroyed items for the current scene
    public void ClearDestroyedItemsForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (string.IsNullOrEmpty(currentScene))
        {
            Debug.LogError("Current scene name is invalid.");
            return;
        }

        if (destroyedItemsByScene.ContainsKey(currentScene))
        {
            destroyedItemsByScene[currentScene].Clear();
            Debug.Log("Cleared destroyed items for scene: " + currentScene);
        }
    }

    // Get destroyed items for a specific scene
    public HashSet<string> GetDestroyedItemsForScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            return new HashSet<string>(); // Return an empty set if sceneName is invalid
        }

        if (destroyedItemsByScene.ContainsKey(sceneName))
        {
            return destroyedItemsByScene[sceneName];
        }

        Debug.LogWarning("No destroyed items found for scene: " + sceneName);
        return new HashSet<string>(); // Return an empty set if no items are destroyed in the scene
    }

    // Optional: Reset all data when restarting the game or resetting
    public void ClearAllDestroyedItems()
    {
        destroyedItemsByScene.Clear();
        Debug.Log("Cleared all destroyed items.");
    }
    public List<string> SerializeDestroyedItems()
    {
        List<string> serializedItems = new List<string>();

        foreach (var sceneEntry in destroyedItemsByScene)
        {
            foreach (var itemId in sceneEntry.Value)
            {
                serializedItems.Add($"{sceneEntry.Key}:{itemId}");
            }
        }

        return serializedItems;
    }
    public void DeserializeDestroyedItems(List<string> serializedItems)
{
    destroyedItemsByScene = new Dictionary<string, HashSet<string>>();

    // Deserialize the items into the dictionary
    foreach (var entry in serializedItems)
    {
        string[] parts = entry.Split(':');
        if (parts.Length == 2)
        {
            string sceneName = parts[0];
            string itemId = parts[1];

            // Ensure the scene exists in the dictionary
            if (!destroyedItemsByScene.ContainsKey(sceneName))
            {
                destroyedItemsByScene[sceneName] = new HashSet<string>();
            }

            // Add the item to the destroyed list for the scene
            destroyedItemsByScene[sceneName].Add(itemId);
        }
    }

    // Disable items for the current scene
    string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

    if (destroyedItemsByScene.ContainsKey(currentScene))
    {
        HashSet<string> destroyedItems = destroyedItemsByScene[currentScene];

        foreach (string itemId in destroyedItems)
        {
            GameObject item = GameObject.Find(itemId);

            if (item != null)
            {
                // Disable the item
                item.SetActive(false);
                Debug.Log($"Item {itemId} in scene {currentScene} has been disabled.");
            }
            else
            {
                Debug.LogWarning($"Item {itemId} not found in scene {currentScene}.");
            }
        }
    }
    else
    {
        Debug.Log($"No destroyed items to disable for the current scene: {currentScene}.");
    }
}



}
