using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Import for UI handling
using System.IO; // Import for file handling

public class TestTransition : MonoBehaviour
{
    public string groundFloorSceneName = "GroundFloor"; // Ground floor scene name
    public string courtyardSceneName = "Courtyard"; // Courtyard scene name
    public string saveFilePath = "saveData.dat"; // Path to the save file

    // Panel and Buttons
    public GameObject controlsPanel; // Reference to controls panel
    public Button newGameButton; // New game button
    public Button continueButton; // Continue button
    public Button controlsButton; // Controls button

    // Button List for Navigation
    private List<Button> buttons;
    private int selectedIndex = 0; // Track currently selected button

    // Flag to track if controls panel is open
    private bool isControlsPanelActive = false;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize buttons and subscribe to onClick events
        buttons = new List<Button> { newGameButton, continueButton, controlsButton };

        // Add onClick listeners for each button
        newGameButton.onClick.AddListener(OnNewGame);
        continueButton.onClick.AddListener(OnContinue);
        controlsButton.onClick.AddListener(OnControls);

        // Set initial button selection
        SelectButton(selectedIndex);

        // Hide controls panel at start
        controlsPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // If controls panel is active, disable button navigation
        if (isControlsPanelActive)
        {
            // Allow closing controls panel with Enter
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CloseControls();
            }
            return; // Prevent button navigation when controls panel is active
        }

        // Handle button navigation with arrow keys
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = Mathf.Max(selectedIndex - 1, 0); // Navigate up, ensuring it's within bounds
            SelectButton(selectedIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = Mathf.Min(selectedIndex + 1, buttons.Count - 1); // Navigate down, ensuring it's within bounds
            SelectButton(selectedIndex);
        }

        // Handle button selection with Enter key
        if (Input.GetKeyDown(KeyCode.Return))
        {
            buttons[selectedIndex].onClick.Invoke();
        }
    }

    // Function to handle button selection visual change
    void SelectButton(int index)
    {
        // Deselect all buttons
        foreach (var button in buttons)
        {
            button.GetComponent<Outline>().enabled = false; // Remove the border (if using Outline component)
        }

        // Select the button
        buttons[index].GetComponent<Outline>().enabled = true; // Add a border to the selected button
    }

    // Function to handle new game logic
    public void OnNewGame()
    {
        // Check if save file exists
        if (File.Exists(saveFilePath))
        {
            // Delete the save file
            File.Delete(saveFilePath);
        }

        // Transition to Courtyard scene
        SceneManager.LoadScene(courtyardSceneName);
    }

    // Function to handle continue game logic
    public void OnContinue()
    {
        // Check if save file exists
        if (File.Exists(saveFilePath))
        {
            // Load GroundFloor scene
            SceneManager.LoadScene(groundFloorSceneName);
        }
    }

    // Function to handle controls button
    public void OnControls()
    {
        // Show controls panel and disable game input
        controlsPanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game while controls are visible
        isControlsPanelActive = true; // Mark the controls panel as active
    }

    // Function to close controls panel when Enter is pressed
    public void CloseControls()
    {
        controlsPanel.SetActive(false);
        Time.timeScale = 1f; // Resume the game
        isControlsPanelActive = false; // Mark the controls panel as inactive
    }
}
