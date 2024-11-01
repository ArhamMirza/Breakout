using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public Transform target;          // The car that the camera will follow
    public float smoothSpeed = 0.125f; // How smooth the camera movement will be
    public Vector3 offset;            // Offset position from the target

    void LateUpdate()
    {
        if (target == null) return; // Check if target is assigned

        // Desired position with offset
        Vector3 desiredPosition = target.position + offset;
        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Optional: Keep the camera facing the same direction
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}

