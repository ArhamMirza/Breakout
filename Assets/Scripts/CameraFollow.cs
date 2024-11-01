using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset; 

    void LateUpdate()
    {
        // This determines the location of camera.
        Vector3 desiredPosition = player.position + offset;

        // distance from z axis to make the game scene plane visible
        desiredPosition.z = -10f;

        transform.position = desiredPosition;

    }
}
