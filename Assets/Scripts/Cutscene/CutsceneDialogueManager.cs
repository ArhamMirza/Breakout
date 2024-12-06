using UnityEngine;
using UnityEngine.UI;

public class CutsceneDialogueManager : MonoBehaviour
{
    public static CutsceneDialogueManager instance;
    public GameObject dialogueBox;
    public Text dialogueText;
    private bool isDialogueActive = false;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static void ShowDialogue(string speaker, string message)
    {
        instance.dialogueBox.SetActive(true);
        instance.dialogueText.text = $"{speaker}: {message}";
        instance.isDialogueActive = true;
    }

    public static bool IsDialogueFinished()
    {
        return !instance.isDialogueActive;
    }

    public void CloseDialogue()
    {
        dialogueBox.SetActive(false);
        isDialogueActive = false;
    }
}
