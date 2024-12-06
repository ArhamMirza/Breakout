using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Reference to the Canvas GameObject
    public Canvas UICanvas; // Reference to the Canvas

    private GameObject caughtPopup; // Reference to GameOver child
    private Button restartButton; // Reference to Restart button
    private Button quitButton; // Reference to Quit button

    [Header("Alertness UI Settings")]
    [SerializeField] private Color lowAlertnessColor = Color.green;
    [SerializeField] private Color mediumAlertnessColor = Color.yellow;
    [SerializeField] private Color highAlertnessColor = Color.red;
    private Slider alertnessSlider; 
    private Image fillImage; 
    private GameObject player; // Reference to the player object
    private Player playerScript; // Reference to the player's script


    void Awake()
    {
        // Check if an instance of UIManager already exists
        if (Instance == null)
        {
            // Set the current instance as the singleton
            Instance = this;

            // Prevent the UIManager from being destroyed when switching scenes
            DontDestroyOnLoad(gameObject);
            
            // Ensure the Canvas persists across scenes
            PersistCanvas();
        }
        else
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }

        
    }
    void Start()
    {
        // Locate GameOver popup and add null check
        caughtPopup = UICanvas.transform.Find("GameOver")?.gameObject;
        if (caughtPopup == null)
        {
            Debug.LogError("GameOver popup not found! Make sure it exists in the UICanvas.");
        }

        // Locate Slider and add null check
        var sliderTransform = UICanvas.transform.Find("Slider");
        if (sliderTransform != null)
        {
            alertnessSlider = sliderTransform.GetComponent<Slider>();
            if (alertnessSlider == null)
            {
                Debug.LogError("Slider component not found on Slider GameObject!");
            }
        }
        else
        {
            Debug.LogError("Slider GameObject not found! Make sure it exists in the UICanvas.");
        }

        // Locate Fill Area and add null check
        if (alertnessSlider != null)
        {
            var fillAreaTransform = alertnessSlider.transform.Find("Fill Area").transform.Find("Fill");
            if (fillAreaTransform != null)
            {
                fillImage = fillAreaTransform.GetComponent<Image>();
                if (fillImage == null)
                {
                    Debug.LogError("Image component not found on Fill Area!");
                }
            }
            else
            {
                Debug.LogError("Fill Area not found as a child of Slider!");
            }
        }



        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerScript = player.GetComponent<Player>();

            if (playerScript != null)
            {
                Debug.Log("Player script found");
            }
            else
            {
                Debug.LogError("Player script not found on Player GameObject!");
            }
        }
        else
        {
            Debug.LogError("Player object not found! Make sure it is tagged as 'Player'.");
        }

        restartButton = caughtPopup.transform.Find("Restart").GetComponent<Button>();
        quitButton = caughtPopup.transform.Find("Quit").GetComponent<Button>();

        caughtPopup.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

    }

    void Update()
    {
        alertnessSlider.value = playerScript.GetAlertness(); 
        UpdateSliderColor(playerScript.GetAlertness());
    }

    // Ensure the Canvas persists
    private void PersistCanvas()
    {
        if (UICanvas != null)
        {
            DontDestroyOnLoad(UICanvas); // Persist the Canvas and all its children across scenes
        }
        else
        {
            Debug.LogError("Canvas is not assigned in the UIManager!");
        }
    }
     private void QuitGame()
    {
        Time.timeScale = 1; 
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #else
        Application.Quit(); 
        #endif
    }

    private void ShowCaughtPopup()
    {
        caughtPopup.SetActive(true); 
        Time.timeScale = 0; 
    }

    private void RestartGame()
    {
        Debug.Log("Restarting");
    }

    public void UpdateSliderColor(float currentAlertness)
    {
        if (currentAlertness <=100f * 0.33f)
        {
            fillImage.color = lowAlertnessColor;
        }
        else if (currentAlertness <= 100f * 0.66f)
        {
            fillImage.color = mediumAlertnessColor;
        }
        else
        {
            fillImage.color = highAlertnessColor;
        }
    }
}
