using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public CutscenePlayer cutscenePlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            cutscenePlayer.StartCutscene();
            gameObject.SetActive(false); // Disable trigger after activation
        }
    }
}
