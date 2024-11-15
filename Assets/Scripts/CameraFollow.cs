using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;  // Reference to the player's transform
    [SerializeField] private Vector3 offset;    // The offset for camera position
    private const string PLAYER_TAG = "Player";  // Define a tag for player object

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindGameObjectWithTag(PLAYER_TAG)?.transform;

        if (player == null)
        {
            Debug.LogWarning("Player reference is missing or not assigned.");
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 desiredPosition = player.position + offset;

            desiredPosition.z = -10f;

            transform.position = desiredPosition;
        }
        else
        {
            Debug.LogWarning("Player reference is missing. Camera cannot follow.");
        }
    }
}
