using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 
using System.Collections.Generic;
using System.IO; 
using System; 


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
    
    private GameObject stairsBasementToGround; 
    private GameObject stairsGroundToBasement; 
    private GameObject stairsGroundToTop;
    private GameObject stairsTopToGround; 
    private GameObject stairsOutsideToGround;
    private GameObject stairsGroundToOutside;


    private GameSceneManager gameSceneManager;

    private UIManager uiManager;

    [SerializeField] private Sprite upMovementSprite; // Assign this sprite in the Inspector

    [SerializeField] private Sprite disguiseSprite; // New sprite for the disguise

    [SerializeField] private Sprite disguiseSpriteBack;
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component

    private bool disguiseOn;

    private Sprite originalSprite; // Store the original sprite

    [SerializeField] private Sprite crouchSprite;

    private ItemStateManager itemStateManager;

    private Animator animator;



    

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
    uiManager = FindObjectOfType<UIManager>();
    GameObject itemManagerObject = GameObject.Find("ItemStateManager");

    if (itemManagerObject != null)
    {
        itemStateManager = itemManagerObject.GetComponent<ItemStateManager>();
    }
    else
    {
        Debug.LogError("ItemManager GameObject not found!");
    }
    
    spriteRenderer = GetComponent<SpriteRenderer>();
    originalSprite = spriteRenderer.sprite;

    if (gameSceneManager == null)
    {
        Debug.LogError("GameSceneManager not found.");
    }
    else
    {
        currentScene = gameSceneManager.getCurrentScene();
    }

    if (spriteRenderer == null)
    {
        Debug.LogError("SpriteRenderer component not found.");
    }

    stairsGroundToBasement = GameObject.Find("Stairs_GroundToBasement");
    if (stairsGroundToBasement == null)
    {
        Debug.LogError("Stairs_GroundToBasement not found.");
    }

    stairsGroundToTop = GameObject.Find("Stairs_GroundToTop");
    if (stairsGroundToTop == null)
    {
        Debug.LogError("Stairs_GroundToTop not found.");
    }

    stairsGroundToOutside = GameObject.Find("Stairs_GroundToOutside");
    if (stairsGroundToOutside == null)
    {
        Debug.LogError("Stairs_GroundToOutside not found.");
    }

    stairsBasementToGround = null;
    stairsTopToGround = null;
    stairsOutsideToGround = null;

    // Singleton logic
    if (Instance == null)
    {
        Instance = this;

        if (FindObjectOfType<Player>())
        {
            DontDestroyOnLoad(gameObject);

            // Preserve other objects
            if (stairsBasementToGround != null) DontDestroyOnLoad(stairsBasementToGround);
            if (stairsGroundToBasement != null) DontDestroyOnLoad(stairsGroundToBasement);
            if (stairsGroundToTop != null) DontDestroyOnLoad(stairsGroundToTop);
            if (stairsGroundToOutside != null) DontDestroyOnLoad(stairsGroundToOutside);
            if (stairsOutsideToGround != null) DontDestroyOnLoad(stairsOutsideToGround);
            if (stairsTopToGround != null) DontDestroyOnLoad(stairsTopToGround);
            if (inventory != null && inventory.gameObject != null) DontDestroyOnLoad(inventory.gameObject);
            if (alertnessAudioSource != null) DontDestroyOnLoad(alertnessAudioSource);

            Debug.Log("Dont Destroy Player");
        }
    }
    else
    {
        if (gameObject != null)
        {
            Debug.Log("Resetting Player");
            Destroy(gameObject);
        }
    }

    // Load disguise state from JSON and set the sprite accordingly
    LoadPlayer();
    SetInitialSprite();
}


private void SetInitialSprite()
{
    if (disguiseOn)
    {
        spriteRenderer.sprite = disguiseSprite; // Set disguise sprite
    }
    else
    {
        spriteRenderer.sprite = originalSprite; // Set original sprite
    }

    // Optionally set initial direction
    SetDirection(Direction.Right); // or any default direction you prefer
}



    void Start()
    {
        alertness = 0f;
        defaultMoveSpeed = moveSpeed; 
        Time.timeScale = 1; 
        animator = GetComponent<Animator>();


        if (inventory == null)
        {
            inventory = GetComponent<Inventory>();
        }

        interactionHandlers = new Dictionary<string, System.Action<GameObject>>()
        {
            { "Window", HandleWindowInteraction },
            { "Power", HandlePowerInteraction },
            { "Save", HandleSaveInteraction},
            { "NPC_", HandleNPCInteraction },
            { "Item_", HandleItemInteraction },
            { "Door_", HandleDoorInteraction },
            { "Vent1", target => HandleVentInteraction("Vent1", target) }, 
            { "Vent2", target => HandleVentInteraction("Vent2", target) } ,
            { "Vent3", target => HandleVentInteraction("Vent3", target) },  
            { "Vent4", target => HandleVentInteraction("Vent4", target) }  

        };
        // fillImage = alertnessSlider.fillRect.GetComponent<Image>();

        // caughtPopup.SetActive(false);

        // restartButton.onClick.AddListener(RestartGame);
        // quitButton.onClick.AddListener(QuitGame);
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
            // ShowCaughtPopup(); 
        }

        uiManager.UpdateSliderColor(alertness);

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
            gameSceneManager.RoomTransition(transform, stairsGroundToOutside, "Outside");

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
        else if(gameSceneManager.getCurrentScene() == "Outside")
        {
            if(stairsOutsideToGround == null)
            {
                stairsOutsideToGround = GameObject.Find("Stairs_OutsideToGround");
            }

            gameSceneManager.RoomTransition(transform, stairsOutsideToGround, "GroundFloor");

        }
    }

    // void LateUpdate()
    // {
    //     if(isCrouching)
    //     {
    //         spriteRenderer.sprite = crouchSprite;
    //         originalSprite = crouchSprite;
    //     }
        
    // }

    // Toggle crouch
    public void ToggleCrouch()
{
    isCrouching = !isCrouching;
    animator.SetBool("isCrouching", isCrouching);

    if (isCrouching)
    {
        Debug.Log("Player crouching.");

        // Adjust player stats when crouching
        moveSpeed = defaultMoveSpeed / 2; 

       
    }
    else
    {
        Debug.Log("Player stopped crouching.");


        // Restore player stats to default values
        moveSpeed = defaultMoveSpeed; 
     
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

    public bool DisguiseOn
    {
        get { return disguiseOn; }
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

    //Save interaction
    private void HandleSaveInteraction(GameObject target)
    {
        SavePlayer();
    }

    // Handle window interaction
    private void HandleWindowInteraction(GameObject target)
    {
        if (inventory.HasItem("Rope"))
        {
            uiManager.ShowMessageAndPause("You can escape through the window!");
            gameSceneManager.SetLastScene();

            lastScene = gameSceneManager.GetLastScene();

            if (lastScene == "TopFloor")
            {
                gameSceneManager.RoomTransition(transform, target, "Outside");
            }
            else if (lastScene == "Outside")
            {
                gameSceneManager.RoomTransition(transform, target, "TopFloor");
            }
        }
        else
        {
            uiManager.ShowMessageAndPause("Cannot escape, no rope in inventory.");
        }
    }

    private void HandlePowerInteraction(GameObject target)
    {
        uiManager.ShowMessageAndPause("Power Shut down!");
        GameObject blackout = GameObject.Find("BlackOut");
        
        if (blackout == null) 
        {
            // Search all objects (including inactive ones)
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "BlackOut")
                {
                    blackout = obj;
                    break;
                }
            }
        }

        if (blackout != null)
        {
            blackout.SetActive(true); // Enable the GameObject
            Debug.Log("BlackOut GameObject activated.");
        }
        else
        {
            Debug.LogError("BlackOut GameObject not found!");
        }
        gameSceneManager.TurnOffPower();
    
    }

    // Handle NPC interaction
    private void HandleNPCInteraction(GameObject target)
    {
        // Extract NPC type from the tag
        string npcType = target.tag.Substring(4); 
        Debug.Log("Talking to NPC: " + npcType);

        // Check if the NPC is the Janitor
        if (npcType == "Janitor")
        {
            // Call a method on the Janitor script to handle the interaction
            Janitor janitorScript = target.GetComponent<Janitor>();
            if (janitorScript != null)
            {
                janitorScript.StartInteraction(); // Trigger Janitor's interaction logic
                Debug.Log("Interacting with Janitor");
            }
            else
            {
                Debug.LogWarning("Janitor script not found on the NPC!");
            }
        }
    }


    // Handle item interaction
    private void HandleItemInteraction(GameObject target)
    {
        string itemType = target.tag.Substring(5);

        if (itemType == "Disguise")
        {
            PutOnDisguise();
            Destroy(target);
            ItemStateManager.Instance.MarkItemAsDestroyed(target.name); // Mark the item as destroyed

            // Send message to UIManager
            uiManager.ShowMessageAndPause("Disguise acquired!");

            return;
        }

        // For other items
        inventory.AddItem(itemType);
        Debug.Log("Picked up item: " + itemType);

        if (itemPickupAudioClip != null)
        {
            alertnessAudioSource.PlayOneShot(itemPickupAudioClip);
        }

        Destroy(target);
        ItemStateManager.Instance.MarkItemAsDestroyed(target.name); // Mark the item as destroyed

        // Send message to UIManager
        uiManager.ShowMessageAndPause("Picked up item: " + itemType);
    }


    private void PutOnDisguise()
    {
        if (disguiseSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = disguiseSprite; 
            Debug.Log("Disguise applied!");
            disguiseOn = true;
            baseAlertnessIncrease /=3;
            alertnessMultiplier/=3;
        }
        else
        {
            Debug.LogWarning("DisguiseSprite or SpriteRenderer is not set.");
        }
    }

    // Handle door interaction
    private void HandleDoorInteraction(GameObject target)
    {
        string doorType = target.tag.Substring(5); 
        
        if(doorType == "OneSide")
        {
            GameObject openDoorSide = GameObject.FindWithTag("OpenDoorThisSide");
            Debug.Log(openDoorSide);

            if ((transform.position - openDoorSide.transform.position).sqrMagnitude <= 0.5f)
            {
                Debug.Log("Opened door!");
                uiManager.ShowMessageAndPause("Door Opened!");
                Destroy(target);
                ItemStateManager.Instance.MarkItemAsDestroyed(target.name); // Mark the item as destroyed
            }
            else
            {
                uiManager.ShowMessageAndPause("Door does not open from this side");
            }
            
            return;
        }

        if (inventory.HasItem(doorType))
        {
            uiManager.ShowMessageAndPause($"Unlocked door with {doorType}!");
            
            // If the item used is a lockpick, remove it from the inventory
            if (doorType == "Lockpick") 
            {
                inventory.RemoveItem(doorType); // Assuming RemoveItem is a method in your inventory class
            }

            Destroy(target);
            ItemStateManager.Instance.MarkItemAsDestroyed(target.name); // Mark the item as destroyed
        }
        else
        {
            uiManager.ShowMessageAndPause($"Door requires {doorType} to unlock.");
        }
    }


    // Handle vent interation
    private void HandleVentInteraction(string ventId, GameObject target)
    {
        if (inventory.HasItem("Screwdriver"))
        {
            // uiManager.ShowMessageAndPause($"{ventId} opened with screwdriver.");

            lastEnteredVent = ventId;

            gameSceneManager.SetLastEnteredVent(lastEnteredVent);

            gameSceneManager.SetLastScene();

            lastScene = gameSceneManager.GetLastScene();

            if (lastScene != "Vents")
            {
                gameSceneManager.LoadScene("Vents");
                // SceneManager.LoadScene("Vents", LoadSceneMode.Single);
            }
            else if(lastEnteredVent == "Vent3")
            {
                gameSceneManager.LoadScene("Outside");
            }
            else
            {   
                gameSceneManager.LoadScene("GroundFloor");
                // SceneManager.LoadScene("GroundFloor", LoadSceneMode.Single);
            }
        }
        else
        {
            // uiManager.ShowMessageAndPause($"Cannot open {ventId} without a screwdriver.");
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
        float increaseAmount = baseAlertnessIncrease * alertnessMultiplier * Mathf.Pow(amount,exponentialFactor);
        alertness += increaseAmount;
        alertness = Mathf.Clamp(alertness, 0, maxAlertness);
        isAlertnessIncreasing = true;
        timeSinceLastIncrease = 0f;
    }

    public void SetDirection(Direction direction)
{
    currentDirection = direction;

    // Determine which sprite to use based on the current direction
    Sprite targetSprite = originalSprite; // Default to original sprite
    if (disguiseOn)
    {
        targetSprite = disguiseSprite; // Use disguise sprite when disguise is on
    }
    
    // Handle sprite flipping and setting
    switch (direction)
    {
        case Direction.Left:
            spriteRenderer.flipX = true; // Flip sprite for left movement
            spriteRenderer.sprite = targetSprite; // Use target sprite
            break;

        case Direction.Right:
            spriteRenderer.flipX = false; // Reset sprite flip for right movement
            spriteRenderer.sprite = targetSprite; // Use target sprite
            break;

        case Direction.Up:
            break;

        case Direction.Down:
            break;
    }
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


    public void SavePlayer()
    {
        PlayerData data = new PlayerData
        {
            disguiseOn = disguiseOn,
            currentScene = currentScene,
            lastScene = lastScene,
            lastEnteredVent = lastEnteredVent,
            inventoryItems = inventory.SerializeInventory(),
            destroyedItems = ItemStateManager.Instance.SerializeDestroyedItems(),
            currentDirection = currentDirection.ToString(),
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Application.persistentDataPath + "/playerSave.json", json);
        uiManager.ShowMessageAndPause("Game Saved!");
    }
    public void LoadPlayer()
{
    string path = Application.persistentDataPath + "/playerSave.json";
    if (File.Exists(path))
    {
        string json = File.ReadAllText(path);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);

        disguiseOn = data.disguiseOn;
        currentScene = data.currentScene;
        lastScene = data.lastScene;
        lastEnteredVent = data.lastEnteredVent;
        currentDirection = Enum.TryParse(data.currentDirection, out Direction direction)
            ? direction
            : Direction.Down;

        // Restore inventory
        inventory.DeserializeInventory(data.inventoryItems);

        // Restore destroyed items
        transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);
        itemStateManager.DeserializeDestroyedItems(data.destroyedItems); 

        Debug.Log("Game Loaded");
    }
    else
    {
        Debug.LogWarning("Save file not found");
    }
}


} 
