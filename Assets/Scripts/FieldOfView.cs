using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
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

    private Vector3[] rayDirections; // Cached directions for raycasting

    private LineRenderer lineRenderer; // LineRenderer for drawing field of view

    [SerializeField]  private LayerMask ignoreLayers; 


    void Start()
    {
        fieldOfViewDirection = defaultFieldOfViewDirection;

        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerComponent = player.GetComponent<Player>();
        }

        CacheRayDirections();

        // Initialize LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = rayCount + 2; // Include start and end points for a closed loop
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.startColor = lineRenderer.endColor = new Color(173f / 255f, 216f / 255f, 230f / 255f);
        lineRenderer.sortingOrder = 5; // Adjust sorting order as needed
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        Vector3 position = lineRenderer.transform.position;
        lineRenderer.transform.position = new Vector3(position.x, position.y, position.z - 0.1f); // Ensure smaller Z offset
    }

    void Update()
    {
        if (player != null && playerComponent != null)
        {
            DetectTarget();
            DrawFieldOfView();
        }
    }


    void DetectTarget()
    {
        targetDetected = false;
        wallHit = false;

        Vector3 offsetPosition = transform.position + GetBaseDirection() * -viewOffset;
        Vector2 directionToTarget = (player.transform.position - offsetPosition).normalized;
        float angleToTarget = Vector2.Angle(GetBaseDirection(), directionToTarget);

        float distanceToTargetSquared = (offsetPosition - player.transform.position).sqrMagnitude;

        bool isDirectLineOfSight = distanceToTargetSquared < detectionDistance * detectionDistance;
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

    public string DetectWall(Vector3 startPosition, Vector2 moveDirection, float detectionRadius, out float distanceToWall)
{
    // Define a minimum threshold for wall or cover detection
    float detectionThreshold = 0f; // Can be adjusted based on your needs
    Debug.Log(moveDirection);

    // Define the ray directions: straight, left, and right
    Vector2[] rayDirections = new Vector2[]
    {
        moveDirection,                        // Straight ahead
        Quaternion.Euler(0, 0, -5) * moveDirection,  // Left (perpendicular)
        Quaternion.Euler(0, 0, 5) * moveDirection   // Right (perpendicular)
    };

    // Initialize a variable to store the closest distance
    distanceToWall = Mathf.Infinity;
    string detectedObject = "";

    // Color for debugging rays
    Color debugColor = Color.red;

    // Iterate through each ray direction
    foreach (var rayDirection in rayDirections)
    {
        // Draw the ray in the Scene view for visualization
        Debug.DrawRay(startPosition, rayDirection * detectionRadius, debugColor);

        RaycastHit2D hit = Physics2D.Raycast(startPosition, rayDirection, detectionRadius, obstructionMask & ~ignoreLayers);

        if (hit.collider != null)
        {
            // Check if the distance to the detected obstacle is within the threshold
            if (hit.distance < detectionThreshold)
            {
                // Ensure we treat very close walls/obstacles as valid detections
                distanceToWall = hit.distance;
                Debug.Log("Detected obstacle within threshold at distance: " + distanceToWall);

                // Check for wall or cover and return the corresponding label
                if (hit.collider.CompareTag("Wall"))
                {
                    detectedObject = "Wall"; // Detected wall
                }
                else if (hit.collider.CompareTag("Cover"))
                {
                    detectedObject = "Cover"; // Detected cover
                }
            }
            else
            {
                // If the obstacle is further than the threshold, return the distance normally
                if (hit.distance < distanceToWall)
                {
                    distanceToWall = hit.distance;
                    detectedObject = hit.collider.CompareTag("Wall") ? "Wall" : hit.collider.CompareTag("Cover") ? "Cover" : "";
                }
            }
        }
    }

    return detectedObject;
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

    private void DrawFieldOfView()
    {
        Vector3 offsetPosition = transform.position + GetBaseDirection() * -viewOffset;
        List<Vector3> points = new List<Vector3> { offsetPosition };

        float angleStep = fieldOfViewAngle / rayCount;
        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -fieldOfViewAngle / 2 + i * angleStep;
            Vector3 direction = DirFromAngle(angle);

            RaycastHit2D hit = Physics2D.Raycast(offsetPosition, direction, detectionDistance, obstructionMask);
            if (hit.collider != null)
            {
                points.Add(hit.point);
            }
            else
            {
                points.Add(offsetPosition + direction * detectionDistance);
            }
        }

        // Close the field of view by connecting the last ray to the first position
        points.Add(offsetPosition);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public void SetFieldOfViewDirection(FieldOfViewDirection newDirection)
    {
        if (fieldOfViewDirection != newDirection)
        {
            fieldOfViewDirection = newDirection;
            CacheRayDirections(); // Recompute ray directions if direction changes
        }
    }

    public LayerMask getObstructionMask()
    {
        return obstructionMask;
    }

    public FieldOfViewDirection getDefaultDirection()
    {
        return defaultFieldOfViewDirection;
    }
}
