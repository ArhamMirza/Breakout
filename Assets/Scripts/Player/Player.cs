using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    public float maxAlertness = 100f;
    [SerializeField] private float moveSpeed = 5f;
    private float defaultMoveSpeed; // Store default move speed
    private float alertness;
    public Inventory inventory;
    [SerializeField] private Color lowAlertnessColor = Color.green;
    [SerializeField] private Color highAlertnessColor = Color.red;
    [SerializeField] private Color mediumAlertnessColor = Color.yellow;
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

    private Image fillImage; 
    private bool isMoving = false; // New field to track movement

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }
    public Direction currentDirection = Direction.Down; // Store the current direction

    private Dictionary<string, System.Action<GameObject>> interactionHandlers;

    [Header("UI References")]
    [SerializeField] private Slider alertnessSlider; // Reference to the UI Slider
    [SerializeField] private GameObject caughtPopup; // Reference to the popup
    [SerializeField] private Button restartButton; // Button to restart the game
    [SerializeField] private Button quitButton; // Button to quit the game

    void Start()
    {
        alertness = 0;
        defaultMoveSpeed = moveSpeed; 
        Time.timeScale = 1; 

        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        defaultAlertnessMultiplier = alertnessMultiplier;
        defaultExponentialFactor = exponentialFactor;

        interactionHandlers = new Dictionary<string, System.Action<GameObject>>()
        {
            { "Window", HandleWindowInteraction },
            { "NPC_", HandleNPCInteraction },
            { "Item_", HandleItemInteraction },
            { "Door_", HandleDoorInteraction }
        };
        fillImage = alertnessSlider.fillRect.GetComponent<Image>();

        caughtPopup.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        // Check if the player is moving
        isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

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
            ShowCaughtPopup(); // Show the popup when alertness reaches 100
        }

        // Update the alertness slider value
        alertnessSlider.value = alertness; 
        UpdateSliderColor(alertness);
    }

    // Toggle crouch
    public void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        if (isCrouching)
        {
            Debug.Log("Player crouching.");
            moveSpeed = defaultMoveSpeed / 2; 
            exponentialFactor = 0.5f;
            alertnessMultiplier = 0.75f;
        }
        else
        {
            Debug.Log("Player stopped crouching.");
            moveSpeed = defaultMoveSpeed; 
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
        get { return isCrouching; }
    }

    public bool IsMoving
    {
        get { return isMoving; }
    }

    public float GetAlertness()
    {
        return alertness;  
    }

    public void Interact(GameObject target)
    {
        string targetTag = target.tag;

        if (interactionHandlers.ContainsKey(targetTag))
        {
            interactionHandlers[targetTag].Invoke(target);
        }
        else
        {
            // Check for prefix match for "NPC_", "Item_", and "Door_"
            foreach (var key in interactionHandlers.Keys)
            {
                if (targetTag.StartsWith(key))
                {
                    interactionHandlers[key].Invoke(target);
                    break; 
                }
            }
        }
    }

    // Handle window interaction
    private void HandleWindowInteraction(GameObject target)
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

    // Handle NPC interaction
    private void HandleNPCInteraction(GameObject target)
    {
        string npcType = target.tag.Substring(4); 
        Debug.Log("Talking to NPC: " + npcType);
    }

    // Handle item interaction
    private void HandleItemInteraction(GameObject target)
    {
        string itemType = target.tag.Substring(5); 
        inventory.AddItem(itemType);
        Debug.Log("Picked up item: " + itemType);
        Destroy(target);
    }

    // Handle door interaction
    private void HandleDoorInteraction(GameObject target)
    {
        string doorType = target.tag.Substring(5); 
        if (inventory.HasItem(doorType))
        {
            Debug.Log($"Unlocked door with {doorType}!");
        }
        else
        {
            Debug.Log($"Door requires {doorType} to unlock.");
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

    private void UpdateSliderColor(float currentAlertness)
    {
        if (currentAlertness <= maxAlertness * 0.33f)
        {
            // Low alertness - Green zone
            fillImage.color = lowAlertnessColor;
        }
        else if (currentAlertness <= maxAlertness * 0.66f)
        {
            // Medium alertness - Yellow zone
            fillImage.color = mediumAlertnessColor;
        }
        else
        {
            // High alertness - Red zone
            fillImage.color = highAlertnessColor;
        }
    }

    private void ShowCaughtPopup()
    {
        caughtPopup.SetActive(true); 
        Time.timeScale = 0; 
    }

    // Restart the game
    private void RestartGame()
    {
        Time.timeScale = 1; 
        Debug.Log("hello");
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
    }

    // Quit the game
    private void QuitGame()
    {
        Time.timeScale = 1; 
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #else
        Application.Quit(); 
        #endif
    }
}
