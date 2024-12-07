using System.Collections.Generic; // Add this line

[System.Serializable]
public class PlayerData
{
    public float moveSpeed;
    public bool isCrouching;
    public float alertness;
    public float alertnessMultiplier;
    public float exponentialFactor;
    public bool disguiseOn;
    public string currentScene;
    public string lastScene;
    public string lastEnteredVent;
    public List<string> inventoryItems;

    public List<string> destroyedItems; // Serialized destroyed items

    public string currentDirection;

    // Player position
    public float positionX;
    public float positionY;
    public float positionZ;


}
