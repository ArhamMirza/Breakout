using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of the player
    private Rigidbody2D rb; // Reference to the Rigidbody2D component

    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Get input from WASD keys
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        // Check for horizontal input
        if (Input.GetKey(KeyCode.A))
        {
            moveHorizontal = -1f; // Move left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveHorizontal = 1f; // Move right
        }

        // Check for vertical input
        if (Input.GetKey(KeyCode.W))
        {
            moveVertical = 1f; // Move up
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveVertical = -1f; // Move down
        }

        // Only one direction at a time
        if (moveHorizontal != 0f)
        {
            moveVertical = 0f; // Prevent vertical movement if horizontal is active
        }
        else if (moveVertical != 0f)
        {
            moveHorizontal = 0f; // Prevent horizontal movement if vertical is active
        }

        // Create a movement vector
        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        // Move the player using Rigidbody2D
        rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);
    }
}
