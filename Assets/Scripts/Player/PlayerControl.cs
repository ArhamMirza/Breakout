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

    

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

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
            Debug.Log("Interacting");
            InteractWithEnvironment();
        }

        // Throwing
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Toggling Throw Mode");
            ToggleThrowMode();
        }

        if (isThrowMode && Input.GetMouseButtonDown(0))
        {
            Vector2 clickPosition = GetMouseWorldPosition2D(); // Use the 2D version of the mouse position
            Debug.Log($"Mouse Click Position: {clickPosition}");
            
            if ((clickPosition - (Vector2)transform.position).sqrMagnitude <= throwRange * throwRange)
            {
                // ThrowObject(clickPosition);
                Debug.Log("Throwing Object");
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
        mousePos.z = 0; // Not needed for 2D calculations
        return Camera.main.ScreenToWorldPoint(mousePos);
    }


    private void HandleMovement()
    {
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveHorizontal = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveHorizontal = 1f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveVertical = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveVertical = -1f;

        if (moveHorizontal != 0f) moveVertical = 0f;
        else if (moveVertical != 0f) moveHorizontal = 0f;

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        if (moveHorizontal < 0) player.SetDirection(Player.Direction.Left);
        else if (moveHorizontal > 0) player.SetDirection(Player.Direction.Right);
        else if (moveVertical > 0) player.SetDirection(Player.Direction.Up);
        else if (moveVertical < 0) player.SetDirection(Player.Direction.Down);

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
        GameObject thrownObject = Instantiate(throwablePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = thrownObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 throwDirection = (target - transform.position).normalized;
            rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
        }

        // Play sound (optional, if AudioSource is attached to the throwable prefab)
        AudioSource audioSource = thrownObject.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
