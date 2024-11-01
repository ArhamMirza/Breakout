using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private float alertnessIncreaseRate = 15f; 
    private Transform player;
    private Player playerScript;

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

        float alertnessIncrease = alertnessIncreaseRate / Mathf.Max(distanceToPlayer, 1f);
        playerScript.IncreaseAlertness(alertnessIncrease * Time.deltaTime);
    }
}
