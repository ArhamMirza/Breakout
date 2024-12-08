using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneController : MonoBehaviour
{
    void Update()
    {
        // Check if the Enter key is pressed
        if (Input.GetKeyDown(KeyCode.Return)) // Enter key
        {
            Debug.Log("Going to Main Screen");
            // Load the Start scene
            SceneManager.LoadScene("Start");
        }
    }
}
