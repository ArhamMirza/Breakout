using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float maxAlertness = 100f;

    [SerializeField] private float moveSpeed = 5f;
    private float defaultMoveSpeed; // Store default move speed

    private float alertness;
    public Inventory inventory;
    [SerializeField] private Slider alertnessSlider;
    [SerializeField] private Image alertnessFill;
    [SerializeField] private Color lowAlertnessColor = Color.green;
    [SerializeField] private Color highAlertnessColor = Color.red;
    [SerializeField] private float baseAlertnessIncrease = 10f;
    [SerializeField] private float alertnessMultiplier = 1.0f;
    [SerializeField] private float exponentialFactor = 2f;
    [SerializeField] private float alertnessDecrementRate = 1f;

    private float defaultAlertnessMultiplier;
    private float defaultExponentialFactor;

    private bool isCrouching = false; 

    private bool isAlertnessIncreasing = false; // Track if alertness is increasing
    private float timeSinceLastIncrease = 0f; // Track time since last alertness increase
    [SerializeField] private float decreaseDelay = 0.2f; // Time before alertness starts decreasing

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
        defaultMoveSpeed = moveSpeed; // Initialize default move speed

        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        if (alertnessSlider != null)
        {
            alertnessSlider.maxValue = maxAlertness;
            alertnessSlider.value = alertness;
        }

        defaultAlertnessMultiplier = alertnessMultiplier;
        defaultExponentialFactor = exponentialFactor;
    }

    void Update()
    {
        if (alertness > 0)
        {
            // Debug.Log(alertness);
        }
        if (alertness >= 0)
        {
            UpdateAlertness(Time.deltaTime);
        }

        if (alertness == 100)
        {
        }
    }

    // Toggle crouch
    public void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        if (isCrouching)
        {
            Debug.Log("Player crouching.");
            moveSpeed = defaultMoveSpeed / 2; // Reduce move speed by half
            exponentialFactor = 0.5f;
            alertnessMultiplier = 0.75f;
        }
        else
        {
            Debug.Log("Player stopped crouching.");
            moveSpeed = defaultMoveSpeed; // Restore to default speed
            exponentialFactor = defaultExponentialFactor;
            alertnessMultiplier = defaultAlertnessMultiplier;
        }
    }

    public float MoveSpeed
    {
        get { return moveSpeed; }
    }

    public bool IsCrouching
    {
        get {return isCrouching;}
    }

    public void Interact(GameObject target)
    {
        string targetTag = target.tag;
        Debug.Log(targetTag);

        if (targetTag == "Window")
        {
            if (inventory.HasItem("Rope"))
            {
                Debug.Log("You can escape through the window!");
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
            isAlertnessIncreasing = false;
        }
    }


}
