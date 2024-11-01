using UnityEngine;

public class SimpleCarController : MonoBehaviour
{
    public float acceleration = 3f;       // Speed of forward/backward movement
    public float maxSpeed = 15f;           // Maximum speed
    public float turnSpeed = 30f;         // Speed of rotation
    public float driftFactor = 5.95f;      // Factor for controlling drift/sliding

    private Rigidbody2D rb;

    void Start()
    {
        // Try to get the Rigidbody2D component attached to the car
        rb = GetComponent<Rigidbody2D>();

        // Check if the Rigidbody2D component exists, log an error if it's missing
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing from the car. Please add it in the Inspector.");
        }
    }

    void Update()
    {
        // If Rigidbody2D is missing, don't proceed with movement logic
        if (rb == null) return;

        // Get input for movement (W/Up for forward, S/Down for backward)
        float moveInput = Input.GetAxis("Vertical");
        // Get input for rotation (A/Left for left turn, D/Right for right turn)
        float turnInput = Input.GetAxis("Horizontal");

        // Apply forward and backward movement
        if (rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(transform.up * moveInput * acceleration);
        }

        // Apply rotation based on turnInput
        if (moveInput != 0) // Rotate only when the car is moving
        {
            rb.rotation -= turnInput * turnSpeed * Time.deltaTime * moveInput;
        }
        
        // Apply drifting effect
        ApplyDrift();
    }

    private void ApplyDrift()
    {
        // Calculate the car's right direction (lateral movement direction)
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);
        // Reduce the lateral velocity to create drift effect
        rb.velocity = rb.velocity - rightVelocity * (1 - driftFactor);
    }
}
