using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    // Singleton Instance
    public static Player Instance { get; private set; }

    // Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    private float defaultMoveSpeed; 
    private bool isCrouching = false;
    private bool isMoving = false; 

    // Alertness Settings
    [Header("Alertness Settings")]
    public float maxAlertness = 100f;
    [SerializeField] private float baseAlertnessIncrease = 10f;
    [SerializeField] private float alertnessMultiplier = 1.0f;
    [SerializeField] private float exponentialFactor = 2f;
    [SerializeField] private float alertnessDecrementRate = 1f;
    [SerializeField] private float decreaseDelay = 0.2f; 
    private float alertness;
    private float defaultAlertnessMultiplier;
    private float defaultExponentialFactor;
    private bool isAlertnessIncreasing = false; 
    private float timeSinceLastIncrease = 0f; 

    // Alertness UI Settings
    [Header("Alertness UI Settings")]
    [SerializeField] private Color lowAlertnessColor = Color.green;
    [SerializeField] private Color mediumAlertnessColor = Color.yellow;
    [SerializeField] private Color highAlertnessColor = Color.red;
    [SerializeField] private Slider alertnessSlider; 
    [SerializeField] private Image fillImage; 

    // Audio Settings
    [Header("Audio Settings")]
    [SerializeField] private AudioSource alertnessAudioSource; 
    [SerializeField] private AudioClip highAlertnessAudioClip; 
    [SerializeField] private AudioClip itemPickupAudioClip; 

    // Inventory and Interaction
    [Header("Inventory and Interaction")]
    public Inventory inventory;
    private Transform spawnVent1;
    private Dictionary<string, System.Action<GameObject>> interactionHandlers;
    private string lastEnteredVent = ""; 
    private string lastScene = "";
    private string currentScene;


    // Gameplay Objects
    [Header("Gameplay Objects")]
    [SerializeField] private GameObject caughtPopup; 
    [SerializeField] private Button restartButton; 
    [SerializeField] private Button quitButton; 
    private GameObject stairsBasementToGround; 
    private GameObject stairsGroundToBasement; 
    private GameObject stairsGroundToTop;
    private GameObject stairsTopToGround; 
 

    private GameSceneManager gameSceneManager;


    // Direction Settings
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }
    public Direction currentDirection = Direction.Down; 

    void Awake()
    {
        gameSceneManager = FindObjectOfType<GameSceneManager>();
        currentScene = gameSceneManager.getCurrentScene();
        stairsGroundToBasement = GameObject.Find("Stairs_GroundToBasement");
        stairsGroundToTop = GameObject.Find("Stairs_GroundToTop");
        stairsBasementToGround = null;
        stairsTopToGround = null;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
        }
    
       
       if (stairsBasementToGround != null)
        {
            DontDestroyOnLoad(stairsBasementToGround);
        }
        if (stairsGroundToBasement != null)
        {
            DontDestroyOnLoad(stairsGroundToBasement);
        }
         if (stairsGroundToTop != null)
        {
            DontDestroyOnLoad(stairsGroundToTop);
        }
         if (stairsTopToGround != null)
        {
            DontDestroyOnLoad(stairsTopToGround);
        }

        if (inventory != null && inventory.gameObject != null)
        {
            DontDestroyOnLoad(inventory.gameObject);
        }

        if (alertnessAudioSource != null)
        {
            DontDestroyOnLoad(alertnessAudioSource);
        }
 
    }

    void Start()
    {
        alertness = 0f;
        alertnessSlider.value = alertness; 
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
            { "Door_", HandleDoorInteraction },
            { "Vent1", target => HandleVentInteraction("Vent1", target) }, 
            { "Vent2", target => HandleVentInteraction("Vent2", target) } ,
            { "Vent4", target => HandleVentInteraction("Vent4", target) }  

        };
        fillImage = alertnessSlider.fillRect.GetComponent<Image>();

        caughtPopup.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
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
            ShowCaughtPopup(); 
        }

        alertnessSlider.value = alertness; 
        UpdateSliderColor(alertness);

        if (alertness > maxAlertness * 0.66f)
        {
            if (!alertnessAudioSource.isPlaying)
            {
                alertnessAudioSource.PlayOneShot(highAlertnessAudioClip); 
            }
        }
        
        if (gameSceneManager.getCurrentScene() == "GroundFloor")
        {
            gameSceneManager.RoomTransition(transform, stairsGroundToBasement, "Basement");
            gameSceneManager.RoomTransition(transform, stairsGroundToTop, "TopFloor");

        }
        else if(gameSceneManager.getCurrentScene() == "Basement")
        {
            if(stairsBasementToGround == null)
            {
                stairsBasementToGround = GameObject.Find("Stairs_BasementToGround");
            }

            gameSceneManager.RoomTransition(transform, stairsBasementToGround, "GroundFloor");

        }
        else if(gameSceneManager.getCurrentScene() == "TopFloor")
        {
            if(stairsTopToGround == null)
            {
                stairsTopToGround = GameObject.Find("Stairs_TopToGround");
            }

            gameSceneManager.RoomTransition(transform, stairsTopToGround, "GroundFloor");

        }
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

    //Not fully optimized yet
    public void Interact(GameObject target)
    {
        string targetTag = target.tag;

        if (interactionHandlers.ContainsKey(targetTag))
        {
            interactionHandlers[targetTag].Invoke(target);
        }
        else
        {
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

        if (itemPickupAudioClip != null)
        {
            alertnessAudioSource.PlayOneShot(itemPickupAudioClip);
        }

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

    // Handle vent interation
    private void HandleVentInteraction(string ventId, GameObject target)
    {
        if (inventory.HasItem("Screwdriver"))
        {
            Debug.Log($"{ventId} opened with screwdriver.");

            lastEnteredVent = ventId;

            gameSceneManager.SetLastEnteredVent(lastEnteredVent);

            gameSceneManager.SetLastScene();

            lastScene = gameSceneManager.GetLastScene();

            if (lastScene != "Vents")
            {
                gameSceneManager.LoadScene("Vents");
                // SceneManager.LoadScene("Vents", LoadSceneMode.Single);
            }
            else
            {   
                gameSceneManager.LoadScene("GroundFloor");
                // SceneManager.LoadScene("GroundFloor", LoadSceneMode.Single);
            }
        }
        else
        {
            Debug.Log($"Cannot open {ventId} without a screwdriver.");
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
            fillImage.color = lowAlertnessColor;
        }
        else if (currentAlertness <= maxAlertness * 0.66f)
        {
            fillImage.color = mediumAlertnessColor;
        }
        else
        {
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

        if (alertnessAudioSource != null)
        {
            Destroy(alertnessAudioSource.gameObject); 
        }
        
        if (Instance != null)
        {
            Destroy(Instance.gameObject); 
        }


        PersistentCanvas.Instance?.DestroyCanvas();

        gameSceneManager.RestartScene();

        // GameSceneManager.SceneManager.LoadScene(GameSceneManager.SceneManager.GetActiveScene().buildIndex);
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
