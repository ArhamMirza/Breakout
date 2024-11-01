using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Player player;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void Update()
    {
        // Check for crouch toggle
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            player.ToggleCrouch();
        }

        // Check for interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interacting");
            InteractWithEnvironment();
        }
    }

    private void HandleMovement()
    {
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        if (Input.GetKey(KeyCode.A)) moveHorizontal = -1f;
        else if (Input.GetKey(KeyCode.D)) moveHorizontal = 1f;

        if (Input.GetKey(KeyCode.W)) moveVertical = 1f;
        else if (Input.GetKey(KeyCode.S)) moveVertical = -1f;

        if (moveHorizontal != 0f) moveVertical = 0f;
        else if (moveVertical != 0f) moveHorizontal = 0f;

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);
        
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
       
        rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);
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
        
        // This code is used for checking items in front of the player for interaction. 
        float range;
        
        if (player.currentDirection == Player.Direction.Down)
        {
            range = 0.8f;
        }
        else
        {
            range = 0.5f;
        }
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, GetDirectionAsVector2(), range);
        
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject != gameObject)
                {
                    player.Interact(hit.collider.gameObject);
                
                }
            }
        }
    }

}
