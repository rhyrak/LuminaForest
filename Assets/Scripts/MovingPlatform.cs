using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float range = 1.0f;
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private bool startMovingLeft = true;

    private float currentPos = 0.0f;
    private bool moveLeft = true;
    private float startingX;

    public Vector2 PlatformVelocity { get; private set; } // Exposed velocity

    private void Start()
    {
        startingX = transform.position.x;

        // Set the initial position and direction based on startMovingLeft
        if (startMovingLeft)
        {
            currentPos = 1.0f; // Start at the rightmost position
            moveLeft = true;   // Move left initially
            startingX -= range; // Shift starting position to the left
        }
        else
        {
            currentPos = 0.0f; // Start at the leftmost position
            moveLeft = false;  // Move right initially
        }
    }

    private void FixedUpdate()
    {
        float previousX = transform.position.x;

        // Move the platform
        if (moveLeft)
        {
            currentPos -= speed * Time.fixedDeltaTime / range;
        }
        else
        {
            currentPos += speed * Time.fixedDeltaTime / range;
        }

        // Clamp and reverse at boundaries
        if (currentPos >= 1.0f || currentPos <= 0.0f)
        {
            moveLeft = !moveLeft;
            currentPos = Mathf.Clamp(currentPos, 0.0f, 1.0f);
        }

        // Update position and velocity
        float newX = startingX + currentPos * range;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        PlatformVelocity = new Vector2((newX - previousX) / Time.fixedDeltaTime, 0);
    }
}
