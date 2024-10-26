using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public Vector3 offset; // Offset from the player's position

    void LateUpdate()
    {
        // Define the desired position of the camera
        Vector3 desiredPosition = player.position + offset;

        // Keep the z position constant at -10
        desiredPosition.z = -10f;

        // Update the camera position directly
        transform.position = desiredPosition;

    }
}
