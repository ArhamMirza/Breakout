using System.Collections;
using UnityEngine;

public class CutscenePlayerMovement : MonoBehaviour
{
    [Header("Sprite Settings")]
    public SpriteRenderer spriteRenderer;    // Reference to the SpriteRenderer component
    public Sprite movingSprite;  // Normal moving sprite (e.g., walking sprite)
    public Sprite backSprite;                // Sprite to display when the player is moving

    [Header("Animator")]
    public Animator animator;               // Reference to the Animator component

    [Header("Movement")]
    public float movementSpeed = 2.0f;      // Speed of movement for animations
    private bool isMoving = false;          // Tracks if the player is currently moving

    private Vector3 targetPosition;         // Target position for movement
    private bool movementEnabled = false;   // Flag to allow controlled movement

    private void Update()
    {
        if (movementEnabled && isMoving)
        {
            // Move towards the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

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

    // Calculate movement direction
    Vector3 direction = (destination - transform.position).normalized;

    // Switch to the correct sprite based on direction
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
    /// Stops the player's movement and switches to the idle animation.
    /// </summary>

    /// <summary>
    /// Enables animations based on direction (optional).
    /// </summary>
    public void SetMovementDirection(Vector2 direction)
    {
        if (animator != null)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }
    }
}
