using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // Import this namespace

public class TestTransition : MonoBehaviour
{
    // Name of the ground floor scene
    public string groundFloorSceneName = "GroundFloor"; // Make sure the scene is added to the build settings


    // Start is called before the first frame update
    void Start()
    {
        // Optionally, initialize something at the start of the game
    }

    // Update is called once per frame
    void Update()
    {
        // If the player presses the "G" key, transition to the ground floor scene
        if (Input.GetKeyDown(KeyCode.G))
        {
            TransitionToGroundFloor();
        }
    }

    // Function to handle the scene transition
    void TransitionToGroundFloor()
    {
        SceneManager.LoadScene("GroundFloor", LoadSceneMode.Single);
    }
}
