using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Import SceneManagement for restart functionality

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

    private Button selectedButton;
    private Color defaultButtonColor;
    private Color selectedButtonColor = Color.cyan; // Color when the button is selected

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

        // Set default color for buttons
        defaultButtonColor = restartButton.colors.normalColor;

        caughtPopup.SetActive(false);

        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);

        // Set the default selected button
        selectedButton = restartButton;
        UpdateButtonColor();
    }

    void Update()
    {
        alertnessSlider.value = playerScript.GetAlertness();
        UpdateSliderColor(playerScript.GetAlertness());

        // Check if Enter key is pressed to trigger the selected button's click
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            panel.SetActive(false);  // Hide the panel
            Time.timeScale = 1;  // Resume the game
        }

        // Use arrow keys to navigate between buttons
        if (caughtPopup.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // Cycle to the left button (Restart)
                SelectButton(restartButton);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // Cycle to the right button (Quit)
                SelectButton(quitButton);
            }

            // Press Enter to activate the selected button
            if (Input.GetKeyDown(KeyCode.Return))
            {
                selectedButton.onClick.Invoke();
            }
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

    public void ShowCaughtPopup()
    {
        caughtPopup.SetActive(true);
        Time.timeScale = 0;  // Freeze the game
    }

    private void RestartGame()
    {
        // Ensure time scale is reset
        Time.timeScale = 1; 

        // Destroy all objects except the main camera
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            // Check if the object is not the main camera
            if (obj != Camera.main.gameObject)
            {
                Destroy(obj);
            }
        }

        // Reload the current scene
        SceneManager.LoadScene("GroundFloor");
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

    // Function to change the selected button and update its color
    private void SelectButton(Button newButton)
    {
        // Deselect the previous button
        DeselectButton(selectedButton);

        // Select the new button
        selectedButton = newButton;

        // Update the button colors
        UpdateButtonColor();
    }

    // Function to deselect a button and restore its default color
    private void DeselectButton(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = defaultButtonColor;
        button.colors = colors;
    }

    // Function to update the selected button's color
    private void UpdateButtonColor()
    {
        ColorBlock colors = selectedButton.colors;
        colors.normalColor = selectedButtonColor;
        selectedButton.colors = colors;
    }
}
