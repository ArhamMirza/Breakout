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
    [SerializeField] private float detectionRadius = 5f; // Radius within which movement alerts the guard
    [SerializeField] private List<FieldOfView.FieldOfViewDirection> lookAroundDirections;

    private Transform player;
    private Player playerScript;
    private Vector2 originalPosition;
    private string guardType;
    private Coroutine currentRoutine;
    private bool isAlerted = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.GetComponent<Player>();
        originalPosition = transform.position;
        guardType = gameObject.tag;

        fieldOfView = GetComponent<FieldOfView>();

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

        if (isPlayerInLineOfSight)
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

        if (distanceToPlayerSqr <= detectionRadiusSqr && playerScript.IsMoving && !playerScript.IsCrouching)
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
    }

    private void StartRoutineBasedOnGuardType()
    {
        switch (guardType)
        {
            case "Stationary":
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
        while ((new Vector2(transform.position.x, transform.position.y) - targetPosition).sqrMagnitude > threshold)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }

    private void SetDirectionAndLog(FieldOfView.FieldOfViewDirection direction)
    {
        fieldOfView.SetFieldOfViewDirection(direction);
    }
}
