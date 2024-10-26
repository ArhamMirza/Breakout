using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float maxAlertness = 100f;
    private float alertness;

    [Header("Player Components")]
    public Inventory inventory;

    [Header("Alertness UI")]
    public Slider alertnessSlider;
    public Image alertnessFill;
    public Color lowAlertnessColor = Color.green;
    public Color highAlertnessColor = Color.red;

    [Header("Alertness Settings")]
    public float baseAlertnessIncrease = 10f;
    public float alertnessMultiplier = 1.0f;
    public float exponentialFactor = 2f;
    public float alertnessDecrementRate = 1f;

    private bool isCrouching = false;
    private bool isAlertnessIncreasing = false; // Track if alertness is increasing
    private float timeSinceLastIncrease = 0f; // Track time since last alertness increase
    public float decreaseDelay = 0.2f; // Time before alertness starts decreasing

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }
    public Direction currentDirection = Direction.Down; // Store the current direction

    void Start()
    {
        alertness = 0;
        
        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        if (alertnessSlider != null)
        {
            alertnessSlider.maxValue = maxAlertness;
            alertnessSlider.value = alertness;
            UpdateAlertnessUI();
        }
    }

    void Update()
    {
        Debug.Log(alertness);
        if(alertness > 0)
        {
        UpdateAlertness(Time.deltaTime);

        }
        // Debug.Log("Alertness: " + alertness);
        if (alertness == 100)
        {
            // Handle what happens when alertness is 100
        }
    }

    // Toggle crouch
    public void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        if (isCrouching)
        {
            Debug.Log("Player is crouching. Decreasing alertness rate.");
        }
        else
        {
            Debug.Log("Player stopped crouching.");
        }
    }

    // Interaction function
    public void Interact(GameObject target)
    {
        string targetTag = target.tag;
        Debug.Log(targetTag);

        // Check for specific tag types and handle accordingly
        if (targetTag == "Window")
        {
            // Handle Window interaction
            if (inventory.HasItem("Rope"))
            {
                Debug.Log("You can escape through the window!");
                // Escape through window logic here
            }
            else
            {
                Debug.Log("Cannot escape, no rope in inventory.");
            }
        }
        else if (targetTag.StartsWith("NPC_") || targetTag.StartsWith("Item_") || targetTag.StartsWith("Door_"))
        {
            // Split tag to get the object type and specific type
            string[] tagParts = targetTag.Split('_');
            string mainType = tagParts[0];       // "NPC", "Item", "Door"
            string specificType = tagParts.Length > 1 ? tagParts[1] : ""; // Specific type after underscore

            switch (mainType)
            {
                case "NPC":
                    // Handle NPC interaction
                    Debug.Log("Talking to NPC: " + specificType);
                    // NPC interaction logic here
                    break;

                case "Item":
                    // Add item to inventory
                    inventory.AddItem(specificType);
                    Debug.Log("Picked up item: " + specificType);
                    Destroy(target);
                    break;

                case "Door":
                    // Check for required item to unlock door
                    if (specificType == "Lockpick" && inventory.HasItem("Lockpick"))
                    {
                        Debug.Log("Unlocked door with Lockpick!");
                        // Door unlock logic here
                    }
                    else if (specificType == "Card" && inventory.HasItem("Card"))
                    {
                        Debug.Log("Unlocked door with Card!");
                        // Door unlock logic here
                    }
                    else
                    {
                        Debug.Log("Door requires " + specificType + " to unlock, but it’s not in inventory.");
                    }
                    break;

                default:
                    Debug.Log("Unknown interaction type.");
                    break;
            }
        }
        else
        {
            // Debug.Log("Invalid or unrecognized tag format.");
        }
    }

    public void SetAlertness(float value)
    {
        alertness = Mathf.Clamp(value, 0, maxAlertness);
        isAlertnessIncreasing = true; // Set the flag to indicate that alertness is increasing
        timeSinceLastIncrease = 0f; // Reset the timer
    }

    // Increase alertness
    public void IncreaseAlertness(float amount)
    {
        float increaseAmount = baseAlertnessIncrease * alertnessMultiplier * Mathf.Pow(exponentialFactor, amount);
        alertness += increaseAmount;
        alertness = Mathf.Clamp(alertness, 0, maxAlertness);
        UpdateAlertnessUI();
        isAlertnessIncreasing = true; // Indicate that alertness is increasing
        timeSinceLastIncrease = 0f; // Reset the timer
    }

    public void SetDirection(Direction direction)
    {
        currentDirection = direction;
    }

    // Update alertness over time
    private void UpdateAlertness(float deltaTime)
{
    // Update only if alertness is changing
    if (isAlertnessIncreasing)
    {
        timeSinceLastIncrease += deltaTime; // Increment timer
        // Debug.Log(timeSinceLastIncrease);
    }

    // Check if alertness should decrease
    if (timeSinceLastIncrease >= decreaseDelay)
    {
        // Decrease alertness only if the conditions are met
        alertness -= alertnessDecrementRate * deltaTime;
        alertness = Mathf.Clamp(alertness, 0, maxAlertness);
        UpdateAlertnessUI(); // Update UI only when there's a change
        isAlertnessIncreasing = false;

    }
}


    private void UpdateAlertnessUI()
    {
        if (alertnessSlider != null)
        {
            alertnessSlider.value = alertness;
            float alertnessFraction = alertness / maxAlertness;
            alertnessFill.color = Color.Lerp(lowAlertnessColor, highAlertnessColor, alertnessFraction);
        }
    }
}
