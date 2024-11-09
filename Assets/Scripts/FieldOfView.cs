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

    [SerializeField] private FieldOfViewDirection defaultFieldOfViewDirection;
    private FieldOfViewDirection fieldOfViewDirection;

    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private float detectionDistance = 10f;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private LayerMask targetMask;

    [SerializeField] private int rayCount = 50;
    [SerializeField] private float viewOffset = 0.1f;

    public bool targetDetected { get; private set; }
    private bool wallHit;

    [SerializeField] private bool adjustToRotation = true; 

    private GameObject player;
    private Player playerComponent;
    
    private Vector3[] rayDirections;  // Cached directions for raycasting

    void Start()
    {
        fieldOfViewDirection = defaultFieldOfViewDirection;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerComponent = player.GetComponent<Player>();
        }

        CacheRayDirections();
    }

    void Update()
    {
        if (player != null && playerComponent != null)
        {
            DetectTarget();
        }
    }

    void DetectTarget()
    {
        targetDetected = false;
        wallHit = false;

        Vector3 offsetPosition = transform.position + GetBaseDirection() * -viewOffset;
        Vector2 directionToTarget = (player.transform.position - offsetPosition).normalized;
        float angleToTarget = Vector2.Angle(GetBaseDirection(), directionToTarget);
        float distanceToTarget = Vector2.Distance(offsetPosition, player.transform.position);
        
        bool isDirectLineOfSight = distanceToTarget < detectionDistance;
        if (angleToTarget < fieldOfViewAngle / 2 && isDirectLineOfSight)
        {
            RaycastHit2D hitCenter = Physics2D.Raycast(offsetPosition, GetBaseDirection(), detectionDistance, obstructionMask | targetMask);
            if (IsPlayerHit(hitCenter)) return;

            for (int i = 0; i < rayCount / 2; i++)
            {
                // Left raycast
                RaycastHit2D hitLeft = Physics2D.Raycast(offsetPosition, rayDirections[i], detectionDistance, obstructionMask | targetMask);
                if (IsPlayerHit(hitLeft)) return;

                // Right raycast
                RaycastHit2D hitRight = Physics2D.Raycast(offsetPosition, rayDirections[rayCount / 2 + i], detectionDistance, obstructionMask | targetMask);
                if (IsPlayerHit(hitRight)) return;
            }
        }
    }

    private bool IsPlayerHit(RaycastHit2D hit)
    {
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Wall"))
            {
                return false;
            }
            if (hit.collider.CompareTag("Player"))
            {
                targetDetected = true;
                return true;
            }
        }
        return false;
    }

    private void CacheRayDirections()
    {
        rayDirections = new Vector3[rayCount];
        float halfAngle = fieldOfViewAngle / 2;
        float angleStep = halfAngle / (rayCount / 2);

        for (int i = 0; i < rayCount / 2; i++)
        {
            float angle = -halfAngle + i * angleStep;
            rayDirections[i] = DirFromAngle(angle);

            angle = halfAngle - i * angleStep;
            rayDirections[rayCount / 2 + i] = DirFromAngle(angle);
        }
    }

    Vector3 DirFromAngle(float angleInDegrees)
    {
        Vector3 baseDirection = GetBaseDirection();
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

        return new Vector3(
            baseDirection.x * Mathf.Cos(angleInRadians) - baseDirection.y * Mathf.Sin(angleInRadians),
            baseDirection.x * Mathf.Sin(angleInRadians) + baseDirection.y * Mathf.Cos(angleInRadians)
        ).normalized;
    }

    Vector3 GetBaseDirection()
    {
        Vector3 direction = Vector3.up;

        switch (fieldOfViewDirection)
        {
            case FieldOfViewDirection.Right:
                direction = Vector3.right;
                break;
            case FieldOfViewDirection.Left:
                direction = Vector3.left;
                break;
            case FieldOfViewDirection.Up:
                direction = Vector3.up;
                break;
            case FieldOfViewDirection.Down:
                direction = Vector3.down;
                break;
        }

        if (adjustToRotation)
        {
            float angleInRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
            float rotatedX = direction.x * Mathf.Cos(angleInRadians) - direction.y * Mathf.Sin(angleInRadians);
            float rotatedY = direction.x * Mathf.Sin(angleInRadians) + direction.y * Mathf.Cos(angleInRadians);
            direction = new Vector3(rotatedX, rotatedY).normalized;
        }

        return direction;
    }

    void OnDrawGizmos()
    {
        if (player == null) return; // Prevent the error if player is not found

        Gizmos.color = targetDetected ? Color.red : Color.yellow;
        
        Vector3 offsetPosition = transform.position + GetBaseDirection() * -viewOffset;

        Vector3 leftBoundary = DirFromAngle(-fieldOfViewAngle / 2) * detectionDistance;
        Vector3 rightBoundary = DirFromAngle(fieldOfViewAngle / 2) * detectionDistance;

        Gizmos.DrawLine(offsetPosition, offsetPosition + leftBoundary);
        Gizmos.DrawLine(offsetPosition, offsetPosition + rightBoundary);

        float angleStep = fieldOfViewAngle / rayCount;
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -fieldOfViewAngle / 2 + i * angleStep;
            Vector3 direction = DirFromAngle(angle) * detectionDistance;
            Gizmos.DrawLine(offsetPosition, offsetPosition + direction);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(offsetPosition, detectionDistance);
    }


    public void SetFieldOfViewDirection(FieldOfViewDirection newDirection)
    {
        if (fieldOfViewDirection != newDirection)
        {
            fieldOfViewDirection = newDirection;
            CacheRayDirections(); // Recompute ray directions if direction changes
        }
    }
}
