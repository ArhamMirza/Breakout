using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Janitor : MonoBehaviour
{
    private GameObject dialoguePanel; // UI Panel for dialogue
    private Text dialogueText;        // Text component for displaying dialogue
    private GameObject choicePanel;   // UI Panel for choice options
    private Button[] choiceButtons;   // Buttons for menu options
    private Text[] choiceTexts;       // Text components for menu options
    public Canvas gameUI;            // Reference to the main UI (for freezing the game)

    private int selectedChoice = 0;  // Index of the currently selected choice
    private bool isDialogueActive = false;
    private bool inputBlocked = false;
    private PlayerControl playerControl;

    private Player player;

    private string openingDialogue = "Hey there! Looks like you escaped from your cell. Let me know what you need help with.";
    private List<string> hints = new List<string>
    {
        "The cafeteria has some useful items.",
        "You can escape this prison through the Electric Fence at far right accessed through the vent. You need to shut down the power and find pliers.",
        "The security camera in the room at the far left can be disabled with a device that I can build. However, I require 2 electronic parts.",
        "Check inside boxes for items.",
        "You can save your game by interacting with objects resembling a typewriter.",
        "You can use ropes to get out of windows.",
        "The card for opening the power door is located on the top floor in the chief's room. However, you will need a disguise to get past so many guards."
    };

    private void Start()
{
    playerControl = FindObjectOfType<PlayerControl>();
    player = FindObjectOfType<Player>();


    GameObject canvasObject = GameObject.Find("Canvas");
    if (canvasObject == null)
    {
        Debug.LogError("Canvas not found in the scene.");
        return;
    }

    // Find DialoguePanel inside Canvas
    dialoguePanel = canvasObject.transform.Find("DialoguePanel")?.gameObject;
    if (dialoguePanel == null)
    {
        Debug.LogError("DialoguePanel not found under Canvas.");
        return;
    }

    // Get the Text component inside DialoguePanel
    dialogueText = dialoguePanel.GetComponentInChildren<Text>();
    if (dialogueText == null)
    {
        Debug.LogError("Text component not found in DialoguePanel.");
        return;
    }

    // Find ChoicePanel inside Canvas
    choicePanel = canvasObject.transform.Find("ChoicePanel")?.gameObject;
    if (choicePanel == null)
    {
        Debug.LogError("ChoicePanel not found under Canvas.");
        return;
    }

    // Get the buttons inside ChoicePanel
    choiceButtons = choicePanel.GetComponentsInChildren<Button>();
    if (choiceButtons.Length == 0)
    {
        Debug.LogError("No buttons found in ChoicePanel.");
        return;
    }

    // Cache button texts for later
    choiceTexts = new Text[choiceButtons.Length];
    for (int i = 0; i < choiceButtons.Length; i++)
    {
        choiceTexts[i] = choiceButtons[i].GetComponentInChildren<Text>();
    }

    
}

    private void Update()
    {
        if (isDialogueActive)
        {
            HandleChoiceInput();
        }
    }

    public void StartInteraction()
    {
        StartCoroutine(ShowDialogueWithChoices());
    }

    private IEnumerator ShowDialogueWithChoices()
{
    // Freeze the game (optional logic)
    FreezeGame(true);
    inputBlocked = true;

    // Show the opening dialogue
    dialoguePanel.SetActive(true);
    dialogueText.text = openingDialogue;
    Debug.Log(openingDialogue);

    // Wait for 1 second before allowing input
    yield return new WaitForSeconds(1f);

    // Allow the player to press Enter to proceed
    inputBlocked = false;
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
    Debug.Log("Enter pressed. Proceeding...");

    // Hide the dialogue and show the choice menu
    dialoguePanel.SetActive(false);
    ShowChoiceMenu();
}


    private void ShowChoiceMenu()
    {
        choicePanel.SetActive(true);
        UpdateChoiceUI();
        isDialogueActive = true;

        // Add click listeners for the choices (in case you want to allow clicking)
        choiceButtons[0].onClick.RemoveAllListeners();
        choiceButtons[0].onClick.AddListener(() => ExecuteChoice(0));
        
        choiceButtons[1].onClick.RemoveAllListeners();
        choiceButtons[1].onClick.AddListener(() => ExecuteChoice(1));
    }

    private void HandleChoiceInput()
    {
        if (inputBlocked) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedChoice = (selectedChoice - 1 + choiceButtons.Length) % choiceButtons.Length;
            UpdateChoiceUI();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedChoice = (selectedChoice + 1) % choiceButtons.Length;
            UpdateChoiceUI();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteChoice(selectedChoice);
        }
    }

    private void UpdateChoiceUI()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            // Highlight the selected button with a color change (e.g., yellow)
            choiceButtons[i].GetComponentInChildren<Text>().color = (i == selectedChoice) ? Color.red : Color.black;
        }
    }

    private void ExecuteChoice(int choice)
{
    choicePanel.SetActive(false);
    dialoguePanel.SetActive(false);
    isDialogueActive = false;

    if (choice == 0)
    {
        // Select and show a random hint
        if (hints.Count > 0)
        {
            int randomIndex = Random.Range(0, hints.Count);
            string selectedHint = hints[randomIndex];

            // Start the hint dialogue coroutine
            StartCoroutine(ShowHintDialogue(selectedHint));
        }
    }
    else if (choice == 1)
    {
        // Start the craft dialogue coroutine
        StartCoroutine(ShowCraftDialogue());
    }
    else
    {
        StartCoroutine(Exit());
    }

    // Unfreeze the game after the dialogue
}


    private IEnumerator ShowHintDialogue(string hint)
{
    // Show the hint dialogue
    dialoguePanel.SetActive(true);
    dialogueText.text = $"Hint: {hint}";
    Debug.Log("Hint: " + hint);

    yield return new WaitForSeconds(1f);


    // Wait for player to press Enter
    inputBlocked = false;
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));

    // Proceed after Enter is pressed
    dialoguePanel.SetActive(false);
    ShowChoiceMenu(); // Return to the choices menu
}

private IEnumerator ShowCraftDialogue()
{
    // Show the craft dialogue
    dialoguePanel.SetActive(true);
    dialogueText.text = "I can craft a device to disable the camera. Please bring me 2 electronic parts.";
    Debug.Log("Crafting device... Collect the required parts!");

    yield return new WaitForSeconds(1f);

    // Check if the player has at least 2 electronic parts
    if (player.inventory.HasItem("Part") && player.inventory.items["Part"] >= 2)
    {
        dialogueText.text = "Great! You have the parts. Let's craft the device!";
        Debug.Log("Player has enough parts to craft the device.");
    }
    else
    {
        dialogueText.text = "You don't have enough electronic parts. Please bring me 2 parts.";
        Debug.Log("Player doesn't have enough parts.");
    }

    // Wait for player to press Enter
    inputBlocked = false;
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));

    // Proceed after Enter is pressed
    dialoguePanel.SetActive(false);

    ShowChoiceMenu(); // Show the choices menu for crafting
   
   
}



    private void FreezeGame(bool freeze)
{
    // Use a boolean to manage the frozen state instead of time scale
    inputBlocked = freeze;

    // Optionally disable player movement or other systems
    // Example: Assuming you have a PlayerController script
    if (playerControl != null)
    {
        playerControl.enabled = !freeze; // Disable movement when frozen
    }

    // Dim the background or stop animations visually, if needed
    if (freeze)
    {
        Debug.Log("Game frozen.");
    }
    else
    {
        Debug.Log("Game resumed.");
    }
}


    private IEnumerator Exit()
    {
        // Show the exit dialogue
        dialoguePanel.SetActive(true);
        dialogueText.text = "Alright, I'll be here if you need any more help!";
        Debug.Log("Exit Dialogue");
        yield return new WaitForSeconds(1f);


        // Wait for player to press Enter
        inputBlocked = false;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));

        // Proceed after Enter is pressed
        dialoguePanel.SetActive(false);

        // Unfreeze the game after the dialogue
        FreezeGame(false);
    }

}
