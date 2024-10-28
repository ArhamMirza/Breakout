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

    private float defaultAlertnessMultiplier;
    private float defaultExponentialFactor;

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

        // Store the default values for resetting later
        defaultAlertnessMultiplier = alertnessMultiplier;
        defaultExponentialFactor = exponentialFactor;
    }

    void Update()
    {
        if (alertness > 0)
        {
            Debug.Log(alertness);
        }
        if (alertness >= 0)
        {
            UpdateAlertness(Time.deltaTime);
        }

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
            Debug.Log("Player crouching.");
            exponentialFactor = 0.3f;
            alertnessMultiplier = 0.5f;
        }
        else
        {
            Debug.Log("Player stopped crouching.");
            // Reset alertness settings to default values
            exponentialFactor = defaultExponentialFactor;
            alertnessMultiplier = defaultAlertnessMultiplier;
        }
    }

    // Interaction function
    public void Interact(GameObject target)
    {
        string targetTag = target.tag;
        Debug.Log(targetTag);

        if (targetTag == "Window")
        {
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
            string[] tagParts = targetTag.Split('_');
            string mainType = tagParts[0];
            string specificType = tagParts.Length > 1 ? tagParts[1] : "";

            switch (mainType)
            {
                case "NPC":
                    Debug.Log("Talking to NPC: " + specificType);
                    break;
                case "Item":
                    inventory.AddItem(specificType);
                    Debug.Log("Picked up item: " + specificType);
                    Destroy(target);
                    break;
                case "Door":
                    if (specificType == "Lockpick" && inventory.HasItem("Lockpick"))
                    {
                        Debug.Log("Unlocked door with Lockpick!");
                    }
                    else if (specificType == "Card" && inventory.HasItem("Card"))
                    {
                        Debug.Log("Unlocked door with Card!");
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
    }

    public void SetAlertness(float value)
    {
        alertness = Mathf.Clamp(value, 0, maxAlertness);
        isAlertnessIncreasing = true;
        timeSinceLastIncrease = 0f;
    }

    public void IncreaseAlertness(float amount)
    {
        float increaseAmount = baseAlertnessIncrease * alertnessMultiplier * Mathf.Pow(exponentialFactor, amount);
        alertness += increaseAmount;
        alertness = Mathf.Clamp(alertness, 0, maxAlertness);
        UpdateAlertnessUI();
        isAlertnessIncreasing = true;
        timeSinceLastIncrease = 0f;
    }

    public void SetDirection(Direction direction)
    {
        currentDirection = direction;
    }

    private void UpdateAlertness(float deltaTime)
    {
        if (isAlertnessIncreasing)
        {
            timeSinceLastIncrease += deltaTime;
        }

        if (timeSinceLastIncrease >= decreaseDelay)
        {
            alertness -= alertnessDecrementRate * deltaTime;
            alertness = Mathf.Clamp(alertness, 0, maxAlertness);
            UpdateAlertnessUI();
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
