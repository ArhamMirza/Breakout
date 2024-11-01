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

    [SerializeField] private bool adjustToRotation = true; 

    void Start()
    {
        fieldOfViewDirection = defaultFieldOfViewDirection;
    }

    void Update()
    {
        DetectTarget();
    }

    void DetectTarget()
    {
        targetDetected = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 offsetPosition = transform.position + GetBaseDirection() * -viewOffset;
        Vector2 directionToTarget = (player.transform.position - offsetPosition).normalized;

        float angleToTarget = Vector2.Angle(GetBaseDirection(), directionToTarget);
        float distanceToTarget = Vector2.Distance(offsetPosition, player.transform.position);
        bool isDirectLineOfSight = distanceToTarget < detectionDistance;

        if (angleToTarget < fieldOfViewAngle / 2 && isDirectLineOfSight)
        {
            float halfAngle = fieldOfViewAngle / 2;
            float angleStep = halfAngle / (rayCount / 2);

            RaycastHit2D hitCenter = Physics2D.Raycast(offsetPosition, GetBaseDirection(), detectionDistance, obstructionMask | targetMask);
            if (hitCenter.collider != null && hitCenter.collider.CompareTag("Player"))
            {
                targetDetected = true;
                return;
            }

            for (int i = 1; i <= rayCount / 2; i++)
            {
                float angle = -halfAngle + i * angleStep;
                RaycastHit2D hitLeft = Physics2D.Raycast(offsetPosition, DirFromAngle(angle), detectionDistance, obstructionMask | targetMask);
                if (hitLeft.collider != null && hitLeft.collider.CompareTag("Player"))
                {
                    targetDetected = true;
                    return;
                }

                angle = halfAngle - i * angleStep;
                RaycastHit2D hitRight = Physics2D.Raycast(offsetPosition, DirFromAngle(angle), detectionDistance, obstructionMask | targetMask);
                if (hitRight.collider != null && hitRight.collider.CompareTag("Player"))
                {
                    targetDetected = true;
                    return;
                }
            }
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
        if (targetDetected)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        
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
        }
    }
}
