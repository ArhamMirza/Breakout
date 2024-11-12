using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;  // Reference to the player's transform
    [SerializeField] private Vector3 offset;    // The offset for camera position
    private const string PLAYER_TAG = "Player";  // Define a tag for player object

    // Called when the script is enabled
    void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Called when the script is disabled
    void OnDisable()
    {
        // Unsubscribe from the sceneLoaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is triggered when a scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reassign the player reference after a scene load
        player = GameObject.FindGameObjectWithTag(PLAYER_TAG)?.transform;

        // If player is still null after searching for it, log a warning
        if (player == null)
        {
            Debug.LogWarning("Player reference is missing or not assigned.");
        }
    }

    // Called every frame to update the camera's position
    void LateUpdate()
    {
        // Ensure the player reference is valid
        if (player != null)
        {
            // Calculate the desired camera position based on player position and offset
            Vector3 desiredPosition = player.position + offset;

            // Ensure the camera remains at a fixed z position to keep the scene visible
            desiredPosition.z = -10f;

            // Update the camera's position
            transform.position = desiredPosition;
        }
        else
        {
            // Log a warning if the player is not assigned
            Debug.LogWarning("Player reference is missing. Camera cannot follow.");
        }
    }
}
