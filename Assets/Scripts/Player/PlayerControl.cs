using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private Rigidbody2D rb;
    private Player player;

    [Header("Throwing Settings")]
    public GameObject throwablePrefab; // Assign the throwable prefab in the Inspector
    public float throwForce = 10f;
    public GameObject throwRangeIndicator; // Reference to the range indicator circle
    private bool isThrowMode = false;

    private float throwRange;

    private float detectionRadius;

    private Vector3 rayStartPosition; // Store the start position of the ray
    private Vector3 rayEndPosition; // Store the end position of the ray
    private bool rayHit = false; // Flag to indicate if the ray hit something
    private Vector3 rayHitPoint; // Store the point where the ray hits

    public LayerMask ignoreLayers; // Drag and drop the layer mask in the inspector

    public LayerMask cameraLayer;
    private Animator animator; // Declare Animator

    private Inventory inventory;





    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        animator = GetComponent<Animator>(); // Get the Animator component
        inventory = GetComponent<Inventory>();

        detectionRadius = 8f;

        if (throwRangeIndicator != null)
        {
            throwRangeIndicator.SetActive(false); // Ensure the range indicator is initially hidden
        }
            throwRange = throwRangeIndicator.transform.localScale.x / 2f;
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void Update()
    {
        // Crouch toggle
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.ToggleCrouch();
        }

        // Interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithEnvironment();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (inventory.HasItem("Device")) // Replace "Device" with the actual item name in your inventory
            {
                DisableDevice(player.transform.position);  // Call the method to disable the device
            }
            else
            {
                UIManager.Instance.ShowMessageAndPause("Device not found in inventory.");
            }
        }

        // Throwing
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!inventory.HasItem("Bottle")) // Replace "Bottle" with the exact item name in your inventory
            {
                UIManager.Instance.ShowMessageAndPause("No bottles in inventory to throw.");
            }
            else
            {
                ToggleThrowMode();

            }
        }

        if (isThrowMode && Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = GetMouseWorldPosition2D(); // Use the 2D version of the mouse position
            Debug.Log($"Mouse Click Position: {clickPosition}");
            
            if ((clickPosition - (Vector2)transform.position).sqrMagnitude <= throwRange * throwRange)
            {
                // ThrowObject(clickPosition);
                ThrowObject(clickPosition);
                ToggleThrowMode();
            }
            else
            {
                Debug.Log("Click is out of throw range!");
            }
        }


    }
    private Vector2 GetMouseWorldPosition2D()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 0;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }


   private void HandleMovement()
{
    float moveHorizontal = 0f;
    float moveVertical = 0f;

    // Horizontal movement (left/right)
    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        moveHorizontal = -1f;
    else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        moveHorizontal = 1f;

    // Vertical movement (up/down)
    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        moveVertical = 1f;
    else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        moveVertical = -1f;

    // Restrict diagonal movement: if horizontal is not zero, make vertical zero, and vice versa
    if (moveHorizontal != 0f) moveVertical = 0f;
    else if (moveVertical != 0f) moveHorizontal = 0f;

    Vector2 movement = new Vector2(moveHorizontal, moveVertical);


        // Set MoveX and MoveY parameters in Animator
    animator.SetFloat("MoveX", movement.x); // Set horizontal movement
    animator.SetFloat("MoveY", movement.y); // Set vertical movement

    // Update the player direction and sprite using the SetDirection method
    if (moveHorizontal < 0)
    {
        player.SetDirection(Player.Direction.Left);
    }
    else if (moveHorizontal > 0)
    {
        player.SetDirection(Player.Direction.Right);
    }
    else if (moveVertical > 0)
    {
        player.SetDirection(Player.Direction.Up);
    }
    else if (moveVertical < 0)
    {
        player.SetDirection(Player.Direction.Down);
    }

    // Move the player using the Rigidbody2D component
    rb.MovePosition(rb.position + movement * player.MoveSpeed * Time.deltaTime);
}



    public Vector2 GetDirectionAsVector2()
    {
        if (player.currentDirection == Player.Direction.Up) return Vector2.up;
        if (player.currentDirection == Player.Direction.Down) return Vector2.down;
        if (player.currentDirection == Player.Direction.Left) return Vector2.left;
        if (player.currentDirection == Player.Direction.Right) return Vector2.right;

        return Vector2.zero;
    }

    private void InteractWithEnvironment()
    {
        Debug.Log(player.currentDirection);

        float range = (player.currentDirection == Player.Direction.Down) ? 0.8f : 0.5f;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, GetDirectionAsVector2(), range);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                player.Interact(hit.collider.gameObject);
            }
        }
    }

    public void DisableDevice(Vector3 devicePosition)
    {
        // Log the device position for debugging
        Debug.Log("Device Position: " + devicePosition);

        // Convert the device position to 2D (only using x and y)
        Vector2 devicePosition2D = new Vector2(devicePosition.x, devicePosition.y);

        // Find all colliders in the disable radius using a circle (2D check)
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(devicePosition2D, 12f, cameraLayer);

        if (hitColliders.Length > 0)
        {
            // Disable the first camera found
            GameObject cameraObject = hitColliders[0].gameObject;
            SecurityCamera securityCamera = cameraObject.GetComponent<SecurityCamera>();

            if (securityCamera != null)
            {
                Debug.Log("Camera disabled");
                StartCoroutine(DisableCameraForSeconds(securityCamera, 5f));
            }
        }
        else
        {
            Debug.Log("No colliders found in the radius.");
        }
    }


    private System.Collections.IEnumerator DisableCameraForSeconds(SecurityCamera camera, float duration)
{
    // Play the camera disable sound at the camera's position
    PlayCameraDisableSound(camera.transform.position);
    
    // Disable the camera
    camera.Disable(); // Call the disable method on the camera
    
    // Wait for the specified duration
    yield return new WaitForSeconds(duration);
    
    // Re-enable the camera after the duration
    camera.Enable(); 
}

// Method to play the camera disable sound
private void PlayCameraDisableSound(Vector3 position)
{
    // Load the camera disable sound clip (assign this through the inspector or load it via Resources)
    AudioClip cameraDisableClip = Resources.Load<AudioClip>("Sounds/CameraDisable"); // Adjust path based on where your sound clip is stored
    
    if (cameraDisableClip != null)
    {
        // Play the sound at the camera's position
        AudioSource.PlayClipAtPoint(cameraDisableClip, position);
        Debug.Log("Camera disable sound played.");
    }
    else
    {
        Debug.LogError("Camera disable sound clip not found!");
    }
}


    private void ToggleThrowMode()
    {
        isThrowMode = !isThrowMode;
        if (throwRangeIndicator != null)
        {
            throwRangeIndicator.SetActive(isThrowMode);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

   private void ThrowObject(Vector3 target)
{
    

    // Calculate the direction and distance between the player and the target
    Vector2 direction = (target - transform.position).normalized;
    float distanceSquared = (target - transform.position).sqrMagnitude;
    float distance = Mathf.Sqrt(distanceSquared) * 0.8f;

    // Cast a ray to check for walls or obstacles in the path
    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, ~ignoreLayers); // ~ operator inverts the mask to ignore the player

    // Store ray start and end positions for Gizmo drawing
    rayStartPosition = transform.position;

    if (hit.collider != null)
    {
        // Ray hit something, check if it's an obstacle
        if (hit.collider.CompareTag("Wall"))
        {
            UIManager.Instance.ShowMessageAndPause("Throw path is blocked by a wall or obstacle.");
            return; // If there's an obstacle, don't throw the object
        }
    }
    else
    {
        rayHit = false;
        rayEndPosition = target; // If no hit, set the end position to the target
    }

    // Instantiate the thrown object at the player's position
    GameObject thrownObject = Instantiate(throwablePrefab, transform.position, Quaternion.identity);
    Rigidbody2D rb = thrownObject.GetComponent<Rigidbody2D>();

    // Apply velocity to the thrown object if there's no obstacle
    if (rb != null && (hit.collider == null || !hit.collider.CompareTag("Wall")))
    {
        Vector2 throwDirection = (target - transform.position).normalized;
        rb.velocity = throwDirection * throwForce; // Apply velocity based on throwForce
    }

    // Start moving the object over time towards the target
    if ((hit.collider == null || !hit.collider.CompareTag("Wall"))) // Only start moving if there's no obstacle
    {
        StartCoroutine(MoveObjectTowardsTarget(thrownObject, target));
    }

    // After throwing, subtract one bottle from the inventory
    inventory.RemoveItem("Bottle"); // This assumes "Bottle" is the exact key in your inventory
}



// Coroutine to move the object towards the target and destroy it at the destination
private IEnumerator MoveObjectTowardsTarget(GameObject thrownObject, Vector3 target)
{
    float startTime = Time.time;
    float journeyLength = Vector3.Distance(thrownObject.transform.position, target);
    float speed = throwForce; // Define the speed for the object

    while ((thrownObject.transform.position - target).sqrMagnitude > 1f)
    {
        // Move the object towards the target at a fixed speed
        float distanceCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distanceCovered / journeyLength;

        // Move the object smoothly towards the target
        thrownObject.transform.position = Vector3.Lerp(thrownObject.transform.position, target, fractionOfJourney);

        // Wait until the next frame before continuing the loop
        yield return null;
    }

    // Once the object reaches the target, destroy it and play the sound
    Destroy(thrownObject);
    CreateSoundAtLocation(target);
}


// This method draws the Gizmos for the ray visualization in the Scene view
// private void OnDrawGizmos()
// {
//     // Only draw the gizmos if the ray has been fired
//     if (rayStartPosition != null)
//     {
//         Gizmos.color = rayHit ? Color.red : Color.green; // Red if hit, green if no hit
//         Gizmos.DrawLine(rayStartPosition, rayHit ? rayHitPoint : rayEndPosition);
//     }
// }

private void CreateSoundAtLocation(Vector3 position)
{
    // Notify all guards within a specific radius about the sound
    PlayBottleSmashSound(position);
    Debug.Log("Checking detection");
    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, detectionRadius);
    foreach (Collider2D hit in hitColliders)
    {
        GuardAI guard = hit.GetComponent<GuardAI>();
        if (guard != null)
        {
            guard.OnSoundHeard(position);
            Debug.Log("Guard alerted to sound");
        }
    }

    // Play the bottle smash sound at the given position
}

private void PlayBottleSmashSound(Vector3 position)
{
    // Load your bottle smash sound clip (you can assign it via the inspector or load it by name)
    AudioClip bottleSmashClip = Resources.Load<AudioClip>("Sounds/BottleSmash"); // Adjust the path based on where the clip is located in Resources

    if (bottleSmashClip != null)
    {
        // Play the sound at the location of the event
        AudioSource.PlayClipAtPoint(bottleSmashClip, position);
        Debug.Log("Bottle Smash sound played");
    }
    else
    {
        Debug.LogError("Bottle Smash sound clip not found!");
    }
}


}
