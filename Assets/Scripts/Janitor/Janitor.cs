using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // For checking the current scene

public class Janitor : MonoBehaviour
{
    // Enum to represent menu options
    public enum MenuOption
    {
        Hint,
        CraftDevice
    }

    // List to store hints
    private List<string> hints = new List<string>
    {
        "The cafeteria has some useful items.",
        "You can escape this prison through the Electric Fence. You need to shut down the power and find pliers.",
        "The security camera in the room at far left can be disabled with a device that I can build. However, I require 2 electronic parts.",
        "Check inside boxes for items.",
        "You can save your game by interacting with objects resembling a typewriter.",
        "The card for opening the power door is located on the top floor. However, you will need a disguise to get past so many guards."
    };

    private bool isInteracting = false; // Flag to check if the player is interacting with the janitor

    private void Update()
    {
        // Check for player input to interact with the janitor (e.g., pressing "E")
        if (Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            StartInteraction();
        }
    }

    // Method to start interaction with the janitor
    void StartInteraction()
    {
        isInteracting = true;
        StartCoroutine(ShowJanitorDialogue());
    }

    // Coroutine for the janitor's dialogue and then opening the menu
    System.Collections.IEnumerator ShowJanitorDialogue()
    {
        // Get the current active scene name
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Courtyard")
        {
            // If the scene is Courtyard, discuss the escape plan
            Debug.Log("Janitor: If you want to escape, you should head towards the Electric Fence. But beware, you'll need to shut down the power and find pliers!");
        }
        else if (currentScene == "Basement")
        {
            // If the scene is Basement, give hints and show the menu
            Debug.Log("Janitor: Hey there! Here's what I can offer...");

            // Wait a moment before showing the menu options
            yield return new WaitForSeconds(2);
            ShowMenu(); // Show the menu after dialogue
        }
        else
        {
            // Default dialogue if in another scene
            Debug.Log("Janitor: Hmm, this place doesn't look familiar. But I can still help if you need me.");
        }
    }

    // Method to show the menu with options
    void ShowMenu()
    {
        Debug.Log("Janitor Menu: \n1. Hint \n2. Craft Device");

        // Simulate the player selecting '1' for Hint
        int selectedOption = 1; // For now, simulate selecting Hint
        HandleMenuSelection((MenuOption)selectedOption);
    }

    // Handle selected menu option
    void HandleMenuSelection(MenuOption selectedOption)
    {
        switch (selectedOption)
        {
            case MenuOption.Hint:
                ProvideHint();
                break;
            case MenuOption.CraftDevice:
                CraftDevice();
                break;
            default:
                Debug.Log("Invalid option selected.");
                break;
        }
    }

    // Provide a random hint to the player
    void ProvideHint()
    {
        // Choose a random hint from the list
        int randomIndex = Random.Range(0, hints.Count);
        Debug.Log("Hint: " + hints[randomIndex]);
    }

    // Craft a device
    void CraftDevice()
    {
        Debug.Log("Crafting electronic device... You need parts from the basement and cafeteria.");
        // Add logic for crafting the device, like collecting parts, etc.
    }
}
