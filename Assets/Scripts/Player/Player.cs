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
    if (stairsGroundToTop == null)
    {
        Debug.LogError("Stairs_GroundToOutside not found.");
    }

    stairsBasementToGround = null;
    stairsTopToGround = null;
    stairsOutsideToGround = null;
    if (Instance == null && FindObjectOfType<Player>())
    {
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
        if (stairsGroundToOutside != null)
        {
            DontDestroyOnLoad(stairsGroundToOutside);
        }
        if (stairsOutsideToGround != null)
        {
            DontDestroyOnLoad(stairsOutsideToGround);
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
        Instance = this;
        Debug.Log("Dont Destroy Player");
        if (gameObject != null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    else
    {
        Debug.Log("Resetting Player");
        if (gameObject != null)
        {
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

    // Toggle crouch
    public void ToggleCrouch()
{
    isCrouching = !isCrouching;

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
            Debug.Log("You can escape through the window!");
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
            Debug.Log("Cannot escape, no rope in inventory.");
        }
    }

    private void HandlePowerInteraction(GameObject target)
    {
        Debug.Log("Power Shut down!");
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
        if(itemType == "Disguise")
        {
            PutOnDisguise();
            Destroy(target);
            return;
        } 
        inventory.AddItem(itemType);
        Debug.Log("Picked up item: " + itemType);

        if (itemPickupAudioClip != null)
        {
            alertnessAudioSource.PlayOneShot(itemPickupAudioClip);
        }
        
        Destroy(target);
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
                Destroy(target);
                
            }
            else
            {
                Debug.Log("Door does not open from this side");
            }
            return;
        }
        if (inventory.HasItem(doorType))
        {
            Debug.Log($"Unlocked door with {doorType}!");
            Destroy(target);
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
        float increaseAmount = baseAlertnessIncrease * alertnessMultiplier * Mathf.Pow(amount,exponentialFactor);
        alertness += increaseAmount;
        alertness = Mathf.Clamp(alertness, 0, maxAlertness);
        isAlertnessIncreasing = true;
        timeSinceLastIncrease = 0f;
    }

    public void SetDirection(Direction direction)
    {
        currentDirection = direction;

        if (disguiseOn)
        {
            switch (direction)
            {
                case Direction.Left:
                    spriteRenderer.flipX = true; // Flip sprite for left movement
                    spriteRenderer.sprite = disguiseSprite; // Use disguise sprite
                    break;
                case Direction.Right:
                    spriteRenderer.flipX = false; // Reset sprite flip for right movement
                    spriteRenderer.sprite = disguiseSprite; // Use disguise sprite
                    break;
                case Direction.Up:
                    spriteRenderer.flipX = false; // No flipping for up movement
                    spriteRenderer.sprite = disguiseSpriteBack; // Use back disguise sprite
                    break;
                case Direction.Down:
                    spriteRenderer.flipX = false; // No flipping for down movement
                    spriteRenderer.sprite = disguiseSprite; // Use front disguise sprite
                    break;
            }
        }
        else
        {
            switch (direction)
            {
                case Direction.Left:
                    spriteRenderer.flipX = true; // Flip sprite for left movement
                    spriteRenderer.sprite = originalSprite; // Use original sprite
                    break;
                case Direction.Right:
                    spriteRenderer.flipX = false; // Reset sprite flip for right movement
                    spriteRenderer.sprite = originalSprite; // Use original sprite
                    break;
                case Direction.Up:
                    spriteRenderer.flipX = false; // No flipping for up movement
                    spriteRenderer.sprite = upMovementSprite; // Use upward movement sprite
                    break;
                case Direction.Down:
                    spriteRenderer.flipX = false; // No flipping for down movement
                    spriteRenderer.sprite = originalSprite; // Use original sprite
                    break;
            }
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
            currentDirection = currentDirection.ToString(),
            positionX = transform.position.x,
            positionY = transform.position.y,
            positionZ = transform.position.z
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Application.persistentDataPath + "/playerSave.json", json);
        Debug.Log("Game Saved: " + Application.persistentDataPath + "/playerSave.json");
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

            // Restore position
            transform.position = new Vector3(data.positionX, data.positionY, data.positionZ);

            Debug.Log("Game Loaded");
        }
        else
        {
            Debug.LogWarning("Save file not found");
        }
    }


}  
