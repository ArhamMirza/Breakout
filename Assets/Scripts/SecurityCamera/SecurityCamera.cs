using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private float alertnessIncreaseRate = 15f;
    private Transform player;
    private Player playerScript;

    private bool isDisabled = false; // Flag to check if the camera is disabled
    private Transform cameraLight; // Reference to the child object of the camera

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.GetComponent<Player>();
        fieldOfView = GetComponent<FieldOfView>();

        // Cache the child transform (assuming the child object is named "CameraSprite")
        cameraLight = transform.Find("Light"); // Adjust "CameraSprite" to your actual child name

        if (cameraLight == null)
        {
            Debug.LogWarning("No child object found with the name 'CameraSprite'!");
        }
    }

    void Update()
    {
        // Skip detection logic if the camera is disabled
        if (isDisabled) return;

        if (fieldOfView.targetDetected)
        {
            HandlePlayerDetection();
        }
    }

    private void HandlePlayerDetection()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayerSqr = directionToPlayer.sqrMagnitude;

        float alertnessIncrease = alertnessIncreaseRate / Mathf.Max(distanceToPlayerSqr, 1f);
        playerScript.IncreaseAlertness(alertnessIncrease * Time.deltaTime);
    }

    // Function to disable the camera
    public void Disable()
    {
        if (!isDisabled)
        {
            isDisabled = true;
            fieldOfView.enabled = false; // Disable field of view logic

            // Disable the entire child object
            if (cameraLight != null)
            {
                cameraLight.gameObject.SetActive(false);
            }

            Debug.Log($"{gameObject.name} has been disabled.");
        }
    }

    // Function to enable the camera
    public void Enable()
    {
        if (isDisabled)
        {
            isDisabled = false;
            fieldOfView.enabled = true; // Re-enable field of view logic

            // Re-enable the entire child object
            if (cameraLight != null)
            {
                cameraLight.gameObject.SetActive(true);
            }

            Debug.Log($"{gameObject.name} has been enabled.");
        }
    }
}
