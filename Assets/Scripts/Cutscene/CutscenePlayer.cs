using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Add this for scene transition

public class CutscenePlayer : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneAction
    {
        public enum ActionType { Move, Dialogue, Animation, Wait, Sound }
        public ActionType actionType;

        public Transform target;           // Target object
        public Transform destination;      // Destination for movement
        public float duration;             // Duration for Move or Wait actions
        public string message;             // Dialogue message
        public string animationTrigger;    // Animation trigger name
        public AudioClip soundClip;        // Sound clip to play
    }

    public List<CutsceneAction> actions = new List<CutsceneAction>(); // Cutscene actions
    public GameObject player;             // Reference to player object
    public GameObject janitor;            // Reference to janitor object
    public Transform cutsceneDestination; // Target destination for player movement
    public Transform janitorPosition;    // Position where janitor stands during the conversation

    [Header("UI Elements")]
    public GameObject dialoguePanel;      // Panel to display dialogue
    public Text dialogueText;             // Text component for dialogue messages

    private bool isCutscenePlaying = false;
    private bool isDialogueActive = false; // To track if dialogue is active
    private CutscenePlayerMovement playerMovement; // Reference to the CutscenePlayerMovement script

    public GameObject fadeSprite; // Reference to your black sprite
    public GameObject endScenePanel; // Panel to display at the end
    public Text endSceneText; // Text component to display messages


    private void Start()
    {
        // Get the CutscenePlayerMovement component from the player
        playerMovement = player.GetComponent<CutscenePlayerMovement>();

        // Define the cutscene actions
        actions = new List<CutsceneAction>
        {
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Move,
                target = player.transform,
                destination = cutsceneDestination,
                duration = 2.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Player: I need to break out of this prison, but there are too many guards at the moment.",
                duration = 4.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "???: Pssst...",
                duration = 2.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Animation,
                target = player.transform,
                // animationTrigger = "LookUp",
                duration = 0.5f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Move,
                target = player.transform,
                destination = janitorPosition,
                duration = 2.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: I see you want to escape... I've been planning this for a while.",
                duration = 4.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: You don't remember me, do you? I used to be a part of your family's inner circle.",
                duration = 5.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: Your father... he helped me get this job. But now, I’m your inside man. I have strong ties, and I’ll get you out of here.",
                duration = 4.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Player: Wait... you know my father? What are you talking about?",
                duration = 4.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: Yes, I do. He had plans for you, son. But I can’t explain everything now. I’ll get you out—just trust me.",
                duration = 3.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Player: How do we get out of here?",
                duration = 3.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: You'll need this lockpick... it's the only way out. I've been working on it for months, so don't lose it.",
                duration = 5.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: Escape at night, when the guards are less vigilant. The darkness will help you stay undetected.",
                duration = 5.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: I'll make sure the hallway outside your cell is clear. But you need to move fast.",
                duration = 5.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: The timing has to be perfect. If you mess up, everything we've planned will fall apart. The guards will catch on.",
                duration = 6.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: And remember, if you alert any of the guards... they'll lock down the whole prison. Once that happens, it'll be impossible to escape.",
                duration = 6.0f
            },
            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Dialogue,
                message = "Janitor: Once you break out of your cell, make your way to the basement. I'll be waiting for you there.",
                duration = 5.0f
            },

            new CutsceneAction
            {
                actionType = CutsceneAction.ActionType.Animation,
                target = janitor.transform,
                // animationTrigger = "HandOverLockpick",
                duration = 1.5f
            },
        };

        StartCutscene();
    }

    public void StartCutscene()
    {
        if (!isCutscenePlaying)
        {
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        isCutscenePlaying = true;

        foreach (var action in actions)
        {
            switch (action.actionType)
            {
                case CutsceneAction.ActionType.Move:
                    yield return MoveToPosition(action.target, action.destination.position, action.duration);
                    break;

                case CutsceneAction.ActionType.Dialogue:
                    yield return ShowDialogue(action.message);
                    break;

                case CutsceneAction.ActionType.Animation:
                    yield return PlayAnimation(action.target, action.animationTrigger);
                    break;

                case CutsceneAction.ActionType.Wait:
                    yield return new WaitForSeconds(action.duration);
                    break;

                case CutsceneAction.ActionType.Sound:
                    PlaySound(action.soundClip);
                    break;
            }
        }

        // Fade out and show the next night scene
        yield return FadeOut();
        yield return ShowEndSceneDialogue();

        // Transition to the next scene
        SceneManager.LoadScene("GroundFloor"); // Replace with your desired scene name

        isCutscenePlaying = false;
    }

    private IEnumerator MoveToPosition(Transform target, Vector3 destination, float duration)
    {
        playerMovement.MoveTo(destination); // Trigger movement animation and sprite switch

        // Wait for the player to reach the destination
        yield return new WaitUntil(() => (target.position - destination).sqrMagnitude < 0.01f);
    }

    private IEnumerator ShowDialogue(string message)
    {
        dialoguePanel.SetActive(true);   // Show the dialogue panel
        dialogueText.text = message;    // Set the message text

        isDialogueActive = true;
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return)); // Wait for Enter key press
        isDialogueActive = false;

        dialoguePanel.SetActive(false); // Hide the dialogue panel
        yield return new WaitForSeconds(0.2f); // Wait for a brief moment before continuing
    }

    private IEnumerator PlayAnimation(Transform target, string animationTrigger)
    {
        if (target.TryGetComponent(out Animator animator))
        {
            animator.SetTrigger(animationTrigger);
            yield return new WaitForSeconds(0.5f); // Adjust based on your animation length
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }

    private IEnumerator FadeOut()
    {
        // Get the SpriteRenderer component of the black rectangle sprite
        SpriteRenderer spriteRenderer = fadeSprite.GetComponent<SpriteRenderer>();

        // Gradually increase the alpha value to make the screen fade out
        Color color = spriteRenderer.color;
        color.a = 0f; // Start fully transparent
        spriteRenderer.color = color;

        // Fade to opaque
        while (color.a < 1f)
        {
            color.a += Time.deltaTime * 0.5f; // Control fade speed here
            spriteRenderer.color = color;
            yield return null;
        }
    }

    private IEnumerator ShowEndSceneDialogue()
{
    endScenePanel.SetActive(true); // Show the end scene panel

    // Define the sentences you want to display
    string[] sentences = new string[]
    {
        "At night...",
        "The janitor distracts the guard outside the player's cell...",
        "This allows the player to escape using the lockpick..."
    };

    // Loop through each sentence
    foreach (string sentence in sentences)
    {
        endSceneText.text = ""; // Clear the text initially
        yield return FadeInText(sentence); // Fade in each sentence
        yield return WaitForEnterKey(); // Wait for the Enter key press before showing the next sentence
        yield return FadeOutText(); // Fade out the text before moving to the next sentence
    }

    // Wait for a moment before transitioning
    yield return new WaitForSeconds(3f);
}

private IEnumerator FadeInText(string sentence)
{
    endSceneText.text = sentence; // Set the text to the current sentence

    Color startColor = endSceneText.color;
    startColor.a = 0f; // Start fully transparent
    endSceneText.color = startColor;

    // Fade in the text gradually
    float fadeDuration = 1f; // Duration for each fade-in
    float timeElapsed = 0f;

    while (timeElapsed < fadeDuration)
    {
        timeElapsed += Time.deltaTime;
        startColor.a = Mathf.Lerp(0f, 1f, timeElapsed / fadeDuration);
        endSceneText.color = startColor;
        yield return null;
    }

    // Ensure text is fully visible at the end
    startColor.a = 1f;
    endSceneText.color = startColor;
}

private IEnumerator FadeOutText()
{
    Color startColor = endSceneText.color;
    float fadeDuration = 1f; // Duration for fade-out
    float timeElapsed = 0f;

    while (timeElapsed < fadeDuration)
    {
        timeElapsed += Time.deltaTime;
        startColor.a = Mathf.Lerp(1f, 0f, timeElapsed / fadeDuration);
        endSceneText.color = startColor;
        yield return null;
    }

    // Ensure text is fully transparent at the end
    startColor.a = 0f;
    endSceneText.color = startColor;
}

private IEnumerator WaitForEnterKey()
{
    // Wait until the player presses the Enter key
    while (!Input.GetKeyDown(KeyCode.Return))
    {
        yield return null; // Wait for the next frame
    }
}
}