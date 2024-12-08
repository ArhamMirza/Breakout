using System.Collections;
using UnityEngine;

public class CutscenePlayerMovement : MonoBehaviour
{
    [Header("Sprite Settings")]
    public SpriteRenderer spriteRenderer;    // Reference to the SpriteRenderer component
    public Sprite movingSprite;             // Normal moving sprite (e.g., walking sprite)
    public Sprite backSprite;               // Sprite to display when the player is moving

    [Header("Animator")]
    public Animator animator;               // Reference to the Animator component

    [Header("Movement")]
    public float movementSpeed = 2.0f;      // Speed of movement for animations
    private bool isMoving = false;          // Tracks if the player is currently moving

    private Vector3 targetPosition;         // Target position for movement
    private bool movementEnabled = false;   // Flag to allow controlled movement

    private Vector2 velocity;               // Tracks the current velocity of the player
    private const float stopDistance = 0.1f; // Threshold distance to stop (use squared value)

    private void Update()
    {
        if (movementEnabled && isMoving)
        {
            // Calculate squared distance to avoid using Vector3.Distance
            float sqrDistanceToTarget = (targetPosition - transform.position).sqrMagnitude;

            if (sqrDistanceToTarget <= stopDistance * stopDistance)
            {
                StopMoving();
            }
            else
            {
                // Move towards the target
                Vector3 previousPosition = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
                velocity = (transform.position - previousPosition) / Time.deltaTime;

                SetMovementDirection(velocity);
            }
        }
    }

    /// <summary>
    /// Starts moving the player to a specific target position.
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        targetPosition = destination;
        isMoving = true;
        movementEnabled = true;

        // Update the sprite direction based on the target position
        Vector3 direction = (destination - transform.position).normalized;

        if (direction.y > 0)
        {
            // Player is moving up, use the back sprite
            if (spriteRenderer != null && backSprite != null)
            {
                spriteRenderer.sprite = backSprite;
            }
        }
        else if (direction.y < 0)
        {
            // Player is moving down, use the normal sprite
            if (spriteRenderer != null && movingSprite != null)
            {
                spriteRenderer.sprite = movingSprite;
            }
        }

        // Switch to moving animations
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }
    }

    /// <summary>
    /// Sets the movement direction for animations.
    /// </summary>
    public void SetMovementDirection(Vector2 velocity)
    {
        if (animator != null)
        {
            animator.SetFloat("MoveX", velocity.x);
            animator.SetFloat("MoveY", velocity.y);
            Debug.Log($"SetMovementDirection called with velocity: {velocity}");
        }
    }

    /// <summary>
    /// Stops the movement and resets states.
    /// </summary>
    private void StopMoving()
    {
        isMoving = false;
        movementEnabled = false;

        // Reset velocity and animator state
        velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
        }
    }
}
