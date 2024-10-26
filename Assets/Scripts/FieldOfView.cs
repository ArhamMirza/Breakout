using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public enum FieldOfViewDirection
    {
        Right,
        Left,
        Up,
        Down
    }

    public FieldOfViewDirection defaultFieldOfViewDirection; // Default view direction settable in Inspector
    private FieldOfViewDirection fieldOfViewDirection; // Current view direction

    public float fieldOfViewAngle = 90f; // Angle of the field of view
    public float detectionDistance = 10f; // Maximum detection distance
    public LayerMask obstructionMask; // Mask for objects that can obstruct view
    public LayerMask targetMask; // Mask to detect the player or other targets

    public int rayCount = 50; // Number of rays for casting
    public float viewOffset = 0.1f; // Offset distance for the field of view

    public bool targetDetected { get; private set; } // Flag to indicate if target is detected

    void Start()
    {
        fieldOfViewDirection = defaultFieldOfViewDirection;
    }

    void Update()
    {
        DetectTarget();
    }

    // Detect the player with raycasting
    void DetectTarget()
{
    targetDetected = false; // Reset detection flag
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) return;

    // Calculate the offset position for raycasting
    Vector3 offsetPosition = transform.position + GetBaseDirection() * -viewOffset;
    Vector2 directionToTarget = (player.transform.position - offsetPosition).normalized;

    // Check if the target is within the field of view angle
    float angleToTarget = Vector2.Angle(GetBaseDirection(), directionToTarget);
    float distanceToTarget = Vector2.Distance(offsetPosition, player.transform.position);

    // Add direct line of sight check
    bool isDirectLineOfSight = distanceToTarget < detectionDistance;

    if (angleToTarget < fieldOfViewAngle / 2 && isDirectLineOfSight)
    {
        // Cast more rays toward the center of the field of view
        float halfAngle = fieldOfViewAngle / 2;
        float angleStep = halfAngle / (rayCount / 2); // Increase density for central rays
        
        // Check the center ray
        RaycastHit2D hitCenter = Physics2D.Raycast(offsetPosition, GetBaseDirection(), detectionDistance, obstructionMask | targetMask);
        if (hitCenter.collider != null && hitCenter.collider.CompareTag("Player"))
        {
            targetDetected = true;
            return; // Return early if the player is detected
        }

        // Check additional rays towards the left and right
        for (int i = 1; i <= rayCount / 2; i++)
        {
            float angle = -halfAngle + i * angleStep; // Rays to the left
            RaycastHit2D hitLeft = Physics2D.Raycast(offsetPosition, DirFromAngle(angle), detectionDistance, obstructionMask | targetMask);
            if (hitLeft.collider != null && hitLeft.collider.CompareTag("Player"))
            {
                targetDetected = true;
                return; // Return early if the player is detected
            }

            angle = halfAngle - i * angleStep; // Rays to the right
            RaycastHit2D hitRight = Physics2D.Raycast(offsetPosition, DirFromAngle(angle), detectionDistance, obstructionMask | targetMask);
            if (hitRight.collider != null && hitRight.collider.CompareTag("Player"))
            {
                targetDetected = true;
                return; // Return early if the player is detected
            }
        }
    }
}


    // Utility function to get direction from an angle
    Vector3 DirFromAngle(float angleInDegrees)
    {
        Vector3 baseDirection = GetBaseDirection();
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        return new Vector3(
            baseDirection.x * Mathf.Cos(angleInRadians) - baseDirection.y * Mathf.Sin(angleInRadians),
            baseDirection.x * Mathf.Sin(angleInRadians) + baseDirection.y * Mathf.Cos(angleInRadians)
        ).normalized;
    }

    // Get the base direction based on the current field of view direction
    Vector3 GetBaseDirection()
    {
        switch (fieldOfViewDirection)
        {
            case FieldOfViewDirection.Right:
                return Vector3.right;
            case FieldOfViewDirection.Left:
                return Vector3.left;
            case FieldOfViewDirection.Up:
                return Vector3.up;
            case FieldOfViewDirection.Down:
                return Vector3.down;
            default:
                return Vector3.up;
        }
    }

    // Draw the field of view using Gizmos
    void OnDrawGizmos()
    {
        // Change the color based on whether the target is detected
        Gizmos.color = targetDetected ? Color.red : Color.yellow;

        // Calculate the offset position
        Vector3 offsetPosition = transform.position + GetBaseDirection() * -viewOffset;

        // Draw field of view boundary lines
        Vector3 leftBoundary = DirFromAngle(-fieldOfViewAngle / 2) * detectionDistance;
        Vector3 rightBoundary = DirFromAngle(fieldOfViewAngle / 2) * detectionDistance;

        // Draw the boundary lines of the field of view
        Gizmos.DrawLine(offsetPosition, offsetPosition + leftBoundary);
        Gizmos.DrawLine(offsetPosition, offsetPosition + rightBoundary);

        // Draw rays within the field of view angle
        float angleStep = fieldOfViewAngle / rayCount;
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -fieldOfViewAngle / 2 + i * angleStep;
            Vector3 direction = DirFromAngle(angle) * detectionDistance;
            Gizmos.DrawLine(offsetPosition, offsetPosition + direction);
        }

        // Optional: Visualize detection radius as a circle
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(offsetPosition, detectionDistance);
    }

    // Function to change the field of view direction
    public void SetFieldOfViewDirection(FieldOfViewDirection newDirection)
    {
        if (fieldOfViewDirection != newDirection)
        {
            fieldOfViewDirection = newDirection;
            // Debug.Log("Current Field of View Direction: " + fieldOfViewDirection);
        }
    }
}
