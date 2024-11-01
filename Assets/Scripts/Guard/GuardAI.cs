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
    [SerializeField] private List<FieldOfView.FieldOfViewDirection> lookAroundDirections;
    private Transform player;
    private Player playerScript;
    private Vector2 originalPosition;
    private string guardType;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerScript = player.GetComponent<Player>();
        originalPosition = transform.position;
        guardType = gameObject.tag;

        fieldOfView = GetComponent<FieldOfView>();

        switch (guardType)
        {
            case "Stationary":
                break;

            case "Guard":
                StartCoroutine(LookAround());
                break;

            case "Patrol":
                StartCoroutine(PatrolMovement());
                break;
        }
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

    //This line of code is used to determine if the player is directly in line of sight of guard.
    bool isPlayerInLineOfSight = Mathf.Abs(directionToPlayer.x) < 0.5f || Mathf.Abs(directionToPlayer.y) < 0.5f;

    if (isPlayerInLineOfSight)
    {
        //Alertness immediately set to 100 to prevemt players from just running past guards right on front of them
        playerScript.SetAlertness(100);
    }
    else
    {
        // Alertness increase based on distance.
        float alertnessIncrease = alertnessIncreaseRate / Mathf.Max(distanceToPlayer, 1f);
        playerScript.IncreaseAlertness(alertnessIncrease * Time.deltaTime);
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
        Vector2 patrolStart = originalPosition;
        Vector2 patrolEnd;
        if (patrolVertical)
        {
            patrolEnd = originalPosition + new Vector2(0f, patrolLength);
        }
        else
        {
            patrolEnd = originalPosition + new Vector2(patrolLength, 0f);
        }

        while (true)
        {
            SetDirectionAndLog(patrolVertical ? FieldOfView.FieldOfViewDirection.Up : FieldOfView.FieldOfViewDirection.Right);
            yield return MoveToPoint(patrolEnd);
            yield return new WaitForSeconds(pauseDuration);

            SetDirectionAndLog(patrolVertical ? FieldOfView.FieldOfViewDirection.Down : FieldOfView.FieldOfViewDirection.Left);
            yield return MoveToPoint(patrolStart);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private IEnumerator MoveToPoint(Vector2 targetPosition)
    {
        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
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
