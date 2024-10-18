using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float castDistance;


    public bool IsWalking { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;
    private Rigidbody2D _rigidBody2D;
    private Vector3 originalScale;
    private float horizontalInput;
    private bool isJumping = false;

    void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleJump();
        FlipSprite();
    }

    // Handles horizontal movement
    private void HandleMovement()
    {
        horizontalInput = Input.GetAxis("Horizontal");
    }

    // Handles jumping logic
    private void HandleJump()
    {
        if (IsGrounded() && Input.GetButton("Jump"))
        {
            isJumping = true; // Set the jump flag
        }
    }

    // Handles sprite flipping based on movement direction
    private void FlipSprite()
    {
        float moveInput = Input.GetAxis("Horizontal");

        if (moveInput < 0) // Moving left
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (moveInput > 0) // Moving right
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    public bool IsGrounded()
    {
        if (Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }

    void FixedUpdate()
    {
        // Update Horizontal Velocity
        _rigidBody2D.velocity = new Vector2(horizontalInput * walkSpeed, _rigidBody2D.velocity.y);

        // Update Vertical Velocity
        if (isJumping)
        {
            _rigidBody2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            isJumping = false;  // Reset the jump flag
        }
    }
}
