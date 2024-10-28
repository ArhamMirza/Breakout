using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    public FieldOfView fieldOfView;
    private Transform player;
    private Player playerScript;
    public float alertnessIncreaseRate = 15f; // Adjusted for the camera's rate of alertness

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.GetComponent<Player>();
        fieldOfView = GetComponent<FieldOfView>();
    }

    void Update()
    {
        if (fieldOfView.targetDetected)
        {
            HandlePlayerDetection();
        }
    }

    private void HandlePlayerDetection()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Increase alertness when the player is in view
        float alertnessIncrease = alertnessIncreaseRate / Mathf.Max(distanceToPlayer, 1f);
        playerScript.IncreaseAlertness(alertnessIncrease * Time.deltaTime);
    }
}
