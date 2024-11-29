using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardAI : MonoBehaviour
{
    [SerializeField] private FieldOfView fieldOfView;
    [SerializeField] private float pauseDuration = 2f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool patrolVertical;
    [SerializeField] private float patrolLength = 5f;
    [SerializeField] private float alertnessIncreaseRate = 10f;
    [SerializeField] private float detectionRadius = 5f; 
    [SerializeField] private List<FieldOfView.FieldOfViewDirection> lookAroundDirections;

    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private float footstepInterval = 0.5f; 

    // [SerializeField] private AudioSource alertAudioSource;  
    // [SerializeField] private float alertAudioThreshold = 66f; 


    private Transform player;
    private Player playerScript;
    private Vector2 originalPosition;
    private string guardType;
    private Coroutine currentRoutine;
    private bool isAlerted = false;

    private FieldOfView.FieldOfViewDirection defaultDirection;

    private float timer;




    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.GetComponent<Player>();
        originalPosition = transform.position;
        guardType = gameObject.tag;

        fieldOfView = GetComponent<FieldOfView>();
        defaultDirection = fieldOfView.getDefaultDirection();
        Debug.Log(defaultDirection);

        StartRoutineBasedOnGuardType();
    }

    void Update()
    {
        if (fieldOfView.targetDetected)
        {
            HandlePlayerDetection();
        }
        else
        {
            HandleUncrouchedMovementAlertness();
        }

        ManageAlertState();
    }

    private void HandlePlayerDetection()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        bool isPlayerInLineOfSight = Mathf.Abs(directionToPlayer.x) < 0.5f || Mathf.Abs(directionToPlayer.y) < 0.5f;

        if (isPlayerInLineOfSight && !playerScript.DisguiseOn)
        {
            playerScript.SetAlertness(100);
        }
        else
        {
            float alertnessIncrease = alertnessIncreaseRate / Mathf.Max(distanceToPlayer, 1f);
            playerScript.IncreaseAlertness(alertnessIncrease * Time.deltaTime);
        }

        if (playerScript.GetAlertness() > 33 && !playerScript.IsCrouching)
        {
            FacePlayerDirection();
        }
    }

    private void HandleUncrouchedMovementAlertness()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        float distanceToPlayerSqr = directionToPlayer.sqrMagnitude;
        float detectionRadiusSqr = detectionRadius * detectionRadius;

        if (distanceToPlayerSqr <= detectionRadiusSqr && playerScript.IsMoving && !playerScript.IsCrouching && !playerScript.DisguiseOn)
        {
            float distanceFactor = Mathf.Max(1f, detectionRadius - Mathf.Sqrt(distanceToPlayerSqr));
            float adjustedAlertnessIncrease = alertnessIncreaseRate * distanceFactor * Time.deltaTime;

            float newAlertness = Mathf.Min(playerScript.GetAlertness() + adjustedAlertnessIncrease*2f,80);
            playerScript.SetAlertness(newAlertness);

            if (newAlertness > 33)
            {
                FacePlayerDirection();
            }
        }
    }

    private void ManageAlertState()
    {
        float alertness = playerScript.GetAlertness();

        if (alertness > 33 && alertness <= 66 && !isAlerted)
        {
            StopCurrentRoutine();
            currentRoutine = StartCoroutine(AlertedRoutine());
            isAlerted = true;
        }
        else if (alertness < 33 && isAlerted)
        {
            StopCurrentRoutine();
            StartRoutineBasedOnGuardType();
            isAlerted = false;
        }

        // Play alert sound if alertness exceeds the threshold
        
    }

    private void StartRoutineBasedOnGuardType()
    {
        switch (guardType)
        {
            case "Stationary":
                currentRoutine = StartCoroutine(Stationary());
                break;
            case "Guard":
                currentRoutine = StartCoroutine(LookAround());
                break;
            case "Patrol":
                currentRoutine = StartCoroutine(PatrolMovement());
                break;
        }
    }

    private void StopCurrentRoutine()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
    }

    private IEnumerator AlertedRoutine()
    {
        while (playerScript.GetAlertness() > 33 && !playerScript.IsCrouching)
        {
            FacePlayerDirection();
            yield return null;
        }
    }

    private void FacePlayerDirection()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        FieldOfView.FieldOfViewDirection closestDirection;

        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.y))
        {
            closestDirection = directionToPlayer.x > 0 ? FieldOfView.FieldOfViewDirection.Right : FieldOfView.FieldOfViewDirection.Left;
        }
        else
        {
            closestDirection = directionToPlayer.y > 0 ? FieldOfView.FieldOfViewDirection.Up : FieldOfView.FieldOfViewDirection.Down;
        }

        SetDirectionAndLog(closestDirection);
    }

    private IEnumerator Stationary()
    {
        // Reset the timer every time this coroutine starts
        timer = 0f;

        while (true)
        {
            // Increment the timer with the elapsed time
            timer += Time.deltaTime;

            // Debug log to monitor the timer
            Debug.Log("Timer: " + timer);

            // If the timer exceeds 5 seconds, restore the direction and stop the coroutine
            if (timer >= 5f)
            {
                Debug.Log("Resetting to default direction");
                fieldOfView.SetFieldOfViewDirection(defaultDirection);
                yield break; // Exit the coroutine
            }

            // Wait for the next frame
            yield return null;
        }
    }


    private IEnumerator LookAround()
    {
        if (lookAroundDirections == null || lookAroundDirections.Count == 0)
        {
            Debug.LogWarning("No directions specified for LookAround");
            yield break;
        }

        int directionIndex = 0;

        while (true)
        {
            FieldOfView.FieldOfViewDirection direction = lookAroundDirections[directionIndex];
            SetDirectionAndLog(direction);
            yield return new WaitForSeconds(pauseDuration);
            directionIndex = (directionIndex + 1) % lookAroundDirections.Count;
        }
    }


    private IEnumerator PatrolMovement()
    {
        Vector2 patrolEnd = patrolVertical ? originalPosition + new Vector2(0f, patrolLength) : originalPosition + new Vector2(patrolLength, 0f);

        while (true)
        {
            SetDirectionAndLog(patrolVertical ? FieldOfView.FieldOfViewDirection.Up : FieldOfView.FieldOfViewDirection.Right);
            yield return MoveToPoint(patrolEnd);
            yield return new WaitForSeconds(pauseDuration);

            SetDirectionAndLog(patrolVertical ? FieldOfView.FieldOfViewDirection.Down : FieldOfView.FieldOfViewDirection.Left);
            yield return MoveToPoint(originalPosition);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private IEnumerator MoveToPoint(Vector2 targetPosition)
    {
        float threshold = 0.1f * 0.1f;
        StartCoroutine(PlayFootstepSounds());
        while ((new Vector2(transform.position.x, transform.position.y) - targetPosition).sqrMagnitude > threshold)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        StopCoroutine(PlayFootstepSounds());
    }

    private IEnumerator PlayFootstepSounds()
    {
        while (true)
        {
            if (footstepAudioSource != null && !footstepAudioSource.isPlaying)
            {
                footstepAudioSource.PlayOneShot(footstepAudioSource.clip); 
            }
            yield return new WaitForSeconds(footstepInterval);
        }
    }

    private void SetDirectionAndLog(FieldOfView.FieldOfViewDirection direction)
    {
        fieldOfView.SetFieldOfViewDirection(direction);
    }

    public void OnSoundHeard(Vector3 soundPosition)
    {
        // Stop the current routine and focus on the sound
        StopCurrentRoutine();
        StartCoroutine(FocusOnSound(soundPosition));
    }

 private IEnumerator FocusOnSound(Vector3 soundPosition)
{
    // Convert the transform position to a Vector2 for consistent 2D logic
    Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
    Vector2 lastPosition = currentPosition; // Store the last position to check if movement is making progress

    // Minimum distance from cover to prevent getting too close
    float minDistanceToCover = 1f;
    float coverBufferZone = 1f; // Buffer zone to avoid getting too close to cover

    // Calculate the squared distance to the sound position
    float squaredDistanceToSound = (currentPosition - (Vector2)soundPosition).sqrMagnitude;

    // If the distance to the sound is within 4f, just look in the direction of the sound
    if (squaredDistanceToSound <= 16f) // 4f * 4f = 16f
    {
        // Calculate the direction to the sound (as a Vector2)
        Vector2 directionToSound = (Vector2)soundPosition - currentPosition;

        // Normalize the direction, but only move in the cardinal directions (up, down, left, right)
        Vector2 moveDirection = Vector2.zero;

        // Determine which direction is closest (horizontal or vertical)
        if (Mathf.Abs(directionToSound.x) > Mathf.Abs(directionToSound.y))
        {
            // Horizontal movement (left or right)
            moveDirection = directionToSound.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            // Vertical movement (up or down)
            moveDirection = directionToSound.y > 0 ? Vector2.up : Vector2.down;
        }

        // Set the field of view direction based on the sound direction
        if (moveDirection == Vector2.right)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Right);
        }
        else if (moveDirection == Vector2.left)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Left);
        }
        else if (moveDirection == Vector2.up)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Up);
        }
        else if (moveDirection == Vector2.down)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Down);
        }

        // Simply look towards the sound without moving
        StartRoutineBasedOnGuardType();

        yield break; // Exit the routine as no movement is required
    }

   

    // Move toward the sound position with obstacle avoidance
    while ((currentPosition - (Vector2)soundPosition).sqrMagnitude > 2f)
    {
        // Update the timer

        // Calculate the direction to the sound (as a Vector2)
        Vector2 directionToSound = (Vector2)soundPosition - currentPosition;

        // Normalize the direction, but only move in the cardinal directions (up, down, left, right)
        Vector2 moveDirection = Vector2.zero;

        // Determine which direction is closest (horizontal or vertical)
        if (Mathf.Abs(directionToSound.x) > Mathf.Abs(directionToSound.y))
        {
            // Horizontal movement (left or right)
            moveDirection = directionToSound.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            // Vertical movement (up or down)
            moveDirection = directionToSound.y > 0 ? Vector2.up : Vector2.down;
        }

        // Check for a wall or cover using the updated DetectWall method
        string detectionResult = fieldOfView.DetectWall(new Vector3(currentPosition.x, currentPosition.y, transform.position.z), moveDirection, detectionRadius, out float distanceToWall);

        if (detectionResult == "Wall")
        {
            // If a wall is detected, stop the movement
            if (distanceToWall < 1f)
            {
                break;  // Wall is too close, stop moving
            }
        }
        else if (detectionResult == "Cover")
        {
            // If cover is detected, use the same distanceToWall to check the proximity to cover
            if (distanceToWall < 0.5f)
            {
                // If the guard is too close to the cover, adjust the direction more decisively
                // Move further away first if stuck close to the cover
                if (moveDirection == Vector2.up || moveDirection == Vector2.down)
                {
                    // If we're moving vertically, try to switch to horizontal direction to move around the cover
                    moveDirection = directionToSound.x > 0 ? Vector2.right : Vector2.left;
                }
                else
                {
                    // If we're moving horizontally, try to switch to vertical direction
                    moveDirection = directionToSound.y > 0 ? Vector2.up : Vector2.down;
                }

                // If still too close, move a bit further from the cover to make space for maneuvering
                if ((currentPosition - (Vector2)soundPosition).sqrMagnitude < 1f)
                {
                    moveDirection = directionToSound.x > 0 ? Vector2.right : Vector2.left;
                }
            }
        }

        // Set the field of view direction based on the guard's movement direction
        if (moveDirection == Vector2.right)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Right);
        }
        else if (moveDirection == Vector2.left)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Left);
        }
        else if (moveDirection == Vector2.up)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Up);
        }
        else if (moveDirection == Vector2.down)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Down);
        }

        // Move in the selected direction
        currentPosition = Vector2.MoveTowards(currentPosition, currentPosition + moveDirection, speed * Time.deltaTime);

        // Update the transform position with the new calculated position
        transform.position = new Vector3(currentPosition.x, currentPosition.y, transform.position.z);

        // Check if the guard is stuck (no progress has been made)
        if (currentPosition == lastPosition)
        {
            // The guard hasn't moved, break out of the loop and try a different approach
            break;
        }

        // Update lastPosition for the next iteration
        lastPosition = currentPosition;

        yield return null;
    }

    // Call another method when the guard reaches the sound position
   
    StartRoutineBasedOnGuardType();
}


}
