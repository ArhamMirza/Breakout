using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Reference to the Canvas GameObject
    public Canvas UICanvas; 

    private GameObject caughtPopup;
    private Button restartButton;
    private Button quitButton;

    [Header("Alertness UI Settings")]
    [SerializeField] private Color lowAlertnessColor = Color.green;
    [SerializeField] private Color mediumAlertnessColor = Color.yellow;
    [SerializeField] private Color highAlertnessColor = Color.red;
    private Slider alertnessSlider;
    private Image fillImage;
    private GameObject player;
    private Player playerScript;

    private GameObject panel; // Reference to the panel in the UI
    private Text panelText; // Reference to the Text component inside the panel

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PersistCanvas();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Locate the panel and text child
        panel = UICanvas.transform.Find("Panel")?.gameObject;
        if (panel != null)
        {
            panelText = panel.transform.Find("Text")?.GetComponent<Text>();
            if (panelText == null)
            {
                Debug.LogError("Text component not found in Panel!");
            }
        }
        else
        {
            Debug.LogError("Panel not found in Canvas!");
        }

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

        // Initialize references to the player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerScript = player.GetComponent<Player>();
        }
        else
        {
            Debug.LogError("Player object not found!");
        }

        // Initialize buttons
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

        // Check if Enter key is pressed to resume the game
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            panel.SetActive(false);  // Hide the panel
            Time.timeScale = 1;  // Resume the game
        }
    }

    private void PersistCanvas()
    {
        if (UICanvas != null)
        {
            DontDestroyOnLoad(UICanvas);
        }
        else
        {
            Debug.LogError("Canvas is not assigned!");
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
        Time.timeScale = 0;  // Freeze the game
    }

    private void RestartGame()
    {
        Debug.Log("Restarting");
    }

    public void UpdateSliderColor(float currentAlertness)
    {
        if (currentAlertness <= 100f * 0.33f)
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

    // New function to show a message in the panel and pause the game
    public void ShowMessageAndPause(string message)
    {
        if (panel != null && panelText != null)
        {
            panel.SetActive(true);  // Enable the panel
            panelText.text = message;  // Set the message
            Time.timeScale = 0;  // Freeze the game
        }
    }
}
