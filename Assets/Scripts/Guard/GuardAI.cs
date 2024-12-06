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

    private SpriteRenderer spriteRenderer;

    private Transform player;
    private Player playerScript;
    private Vector2 originalPosition;
    private string guardType;
    private Coroutine currentRoutine;
    private bool isAlerted = false;

    private FieldOfView.FieldOfViewDirection defaultDirection;

    private float timer;

    private float minDistanceBetweenGuards = 1.5f; // Minimum distance between guards
    private float repulsionRadius = 2f; // Distance to apply repulsion from other guards
    private float repulsionStrength = 10f; // Strength of the repulsion force
    private LayerMask guardLayerMask; // Layer for detecting other guards

    public Sprite leftSprite;  // Assign this in the Inspector
    public Sprite rightSprite; // Assign this in the Inspector
    public Sprite upSprite;    // Assign this in the Inspector
    public Sprite downSprite;  
    private FieldOfView.FieldOfViewDirection previousDirection;



    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.GetComponent<Player>();
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>(); // Initialize SpriteRenderer

        guardType = gameObject.tag;

        fieldOfView = GetComponent<FieldOfView>();
        defaultDirection = fieldOfView.getDefaultDirection();
        previousDirection = defaultDirection;
        UpdateSpriteBasedOnDirection(defaultDirection);


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
        FieldOfView.FieldOfViewDirection currentDirection = fieldOfView.getDirection();
        if (currentDirection != previousDirection)
        {
            // Direction has changed, update the sprite
            UpdateSpriteBasedOnDirection(currentDirection);
            previousDirection = currentDirection;  // Store the current direction as previous
        }
    }

    private void UpdateSpriteBasedOnDirection(FieldOfView.FieldOfViewDirection direction)
    {
        switch (direction)
        {
            case FieldOfView.FieldOfViewDirection.Left:
                spriteRenderer.sprite = leftSprite;
                break;
            case FieldOfView.FieldOfViewDirection.Right:
                spriteRenderer.sprite = rightSprite;
                break;
            case FieldOfView.FieldOfViewDirection.Up:
                spriteRenderer.sprite = upSprite;
                break;
            case FieldOfView.FieldOfViewDirection.Down:
                spriteRenderer.sprite = downSprite;
                break;
        }
    }

   private void HandlePlayerDetection()
{
    Vector2 directionToPlayer = player.position - transform.position;
    float distanceToPlayer = directionToPlayer.sqrMagnitude;  // Using squared magnitude to avoid unnecessary sqrt
    Vector2 normalizedDirection = directionToPlayer.normalized;

    // Calculate the dot product to determine how aligned the player is with the NPC's forward direction
    float angleToPlayer = Mathf.Abs(Vector2.Dot(normalizedDirection, transform.right));

   
    float alertnessFactor = Mathf.Pow(angleToPlayer, 2);  // Square to heavily reduce at the sides

    if ((angleToPlayer > 0.5f && Mathf.Abs(directionToPlayer.y) < 0.5f && !playerScript.DisguiseOn))
    {
        float alertnessIncrease = 1f; // More gradual increase when aligned or in proximity
        playerScript.IncreaseAlertness(alertnessIncrease);
    }

    else
    {
        float alertnessIncrease = 2f*(Mathf.Max(1f / (distanceToPlayer + 1f), 0.01f));  // Further reduced with distance

        playerScript.IncreaseAlertness(alertnessIncrease);
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
        float distanceFactor = Mathf.Max(1f, detectionRadius - detectionRadiusSqr - distanceToPlayerSqr);
        float adjustedAlertnessIncrease = alertnessIncreaseRate * distanceFactor * Time.deltaTime;

        float newAlertness = Mathf.Min(playerScript.GetAlertness() + adjustedAlertnessIncrease, 80);
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
            // Debug.Log("Timer: " + timer);

            // If the timer exceeds 5 seconds, restore the direction and stop the coroutine
            if (timer >= 5f)
            {
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
    Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
    Vector2 lastPosition = currentPosition;
    float minDistanceToCover = 1f;
    float coverBufferZone = 1f;

    // Variables to track direction changes
    Vector2 previousMoveDirection = Vector2.zero;
    int directionChangeCount = 0;
    float directionChangeResetTime = 1f; // Time window to reset the counter
    float directionChangeTimer = 0f;

    // Check if within close range to just look at the sound
    float squaredDistanceToSound = (currentPosition - (Vector2)soundPosition).sqrMagnitude;
    if (squaredDistanceToSound <= 16f)
    {
        Vector2 directionToSound = ((Vector2)soundPosition - currentPosition).normalized;
        Vector2 moveDirection = GetCardinalDirection(directionToSound);
        UpdateFieldOfViewDirection(moveDirection);
        StartRoutineBasedOnGuardType();
        yield break;
    }

    while ((currentPosition - (Vector2)soundPosition).sqrMagnitude > 2f)
    {
        Vector2 directionToSound = ((Vector2)soundPosition - currentPosition).normalized;
        Vector2 moveDirection = GetCardinalDirection(directionToSound);

        string detectionResult = fieldOfView.DetectWall(new Vector3(currentPosition.x, currentPosition.y, transform.position.z), moveDirection, detectionRadius, out float distanceToWall);
        if (detectionResult == "Wall" && distanceToWall < 1f)
        {
            break;
        }
        else if (detectionResult == "Cover" && distanceToWall < 0.5f)
        {
            moveDirection = AdjustDirectionForCover(moveDirection, directionToSound);
        }

        moveDirection = moveDirection.normalized;

        // Track direction changes
        if (moveDirection != previousMoveDirection)
        {
            directionChangeCount++;
            previousMoveDirection = moveDirection;
        }

        // Handle excessive direction changes
        directionChangeTimer += Time.deltaTime;
        if (directionChangeTimer >= directionChangeResetTime)
        {
            directionChangeCount = 0;
            directionChangeTimer = 0f;
        }
        if (directionChangeCount > 10)
        {
            Debug.Log("Guard is stuck due to frequent direction changes!");
            break;
        }

        UpdateFieldOfViewDirection(moveDirection);
        currentPosition = Vector2.MoveTowards(currentPosition, currentPosition + moveDirection, speed * Time.deltaTime);
        transform.position = new Vector3(currentPosition.x, currentPosition.y, transform.position.z);

        if (currentPosition == lastPosition)
        {
            Debug.Log("Guard is stuck, breaking loop.");
            UpdateFieldOfViewDirection(moveDirection);
            currentRoutine = StartCoroutine(Stationary());

            break;
        }

        lastPosition = currentPosition;
        yield return null;
    }

    // StartRoutineBasedOnGuardType();
    currentRoutine = StartCoroutine(Stationary());

}

// Helper to determine cardinal and diagonal directions
private Vector2 GetCardinalDirection(Vector2 direction)
{
    float diagonalThreshold = 0f; // Threshold to consider diagonal directions

    // Check if the direction is diagonal
    if (Mathf.Abs(direction.x - direction.y) <= diagonalThreshold)
    {
        // Diagonal directions (when x and y are close enough to be considered diagonal)
        if (direction.x > 0 && direction.y > 0)
            return new Vector2(1, 1).normalized; // Up-Right diagonal
        else if (direction.x > 0 && direction.y < 0)
            return new Vector2(1, -1).normalized; // Down-Right diagonal
        else if (direction.x < 0 && direction.y > 0)
            return new Vector2(-1, 1).normalized; // Up-Left diagonal
        else
            return new Vector2(-1, -1).normalized; // Down-Left diagonal
    }
    else
    {
        // If x or y is stronger, treat it as cardinal direction
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal direction (stronger x component)
            if (direction.x > 0)
                return Vector2.right;
            else
                return Vector2.left;
        }
        else
        {
            // Vertical direction (stronger y component)
            if (direction.y > 0)
                return Vector2.up;
            else
                return Vector2.down;
        }
    }
}




// Adjust direction when close to cover
private Vector2 AdjustDirectionForCover(Vector2 moveDirection, Vector2 directionToSound)
{
    if (moveDirection == Vector2.up || moveDirection == Vector2.down)
        return directionToSound.x > 0 ? Vector2.right : Vector2.left;
    else
        return directionToSound.y > 0 ? Vector2.up : Vector2.down;
}


// Update field of view direction based on movement
private void UpdateFieldOfViewDirection(Vector2 moveDirection)
{
    // Check if the move direction is primarily horizontal or vertical
    if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
    {
        // If moving mostly horizontally, set left or right
        if (moveDirection.x > 0)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Right);
        }
        else
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Left);
        }
    }
    else if (Mathf.Abs(moveDirection.y) > Mathf.Abs(moveDirection.x))
    {
        // If moving mostly vertically, set up or down
        if (moveDirection.y > 0)
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Up);
        }
        else
        {
            fieldOfView.SetFieldOfViewDirection(FieldOfView.FieldOfViewDirection.Down);
        }
    }
}

}