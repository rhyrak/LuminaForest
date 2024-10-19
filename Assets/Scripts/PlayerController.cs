using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private bool _isMoving = false;
    private bool _isRunning = false;
    private bool _isGrounded = false;
    private bool _isJumping = false;

    public bool IsMoving
    {
        get
        {
            return _isMoving;
        }
        private set
        {
            _isMoving = value;
            _animator.SetBool("isMoving", value);
        }
    }
    public bool IsRunning
    {
        get
        {
            return _isRunning;
        }
        private set
        {
            _isRunning = value;
            _animator.SetBool("isRunning", value);
        }
    }
    public bool IsGrounded
    {
        get
        {
            return _isGrounded;
        }
        private set
        {
            _isGrounded = value;
            _animator.SetBool("isGrounded", value);
        }
    }
    public bool IsJumping
    {
        get
        {
            return _isJumping;
        }
        private set
        {
            _isJumping = value;
            _animator.SetBool("isJumping", value);
        }
    }
    public float CurrentMoveSpeed
    {
        get
        {
            if (IsMoving)
                return IsRunning ? runSpeed : walkSpeed;
            else
                return 0;
        }
    }

    private Rigidbody2D _rigidBody2D;
    private Animator _animator;
    private Vector3 originalScale;
    private Vector2 moveInput;

    void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        originalScale = transform.localScale;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        FlipSprite();
    }

    // Handles movement
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput.x != 0;
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            IsRunning = true;
        }
        else if (context.canceled)
        {
            IsRunning = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && CheckIsGrounded())
        {
            IsJumping = true; // Set the jump flag
            IsGrounded = false; // Set the grounded flag to false
        }
    }

    // Handles sprite flipping based on movement direction
    private void FlipSprite()
    {
        if (moveInput.x < 0) // Moving left
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (moveInput.x > 0) // Moving right
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    public bool CheckIsGrounded()
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
        _rigidBody2D.velocity = new Vector2(moveInput.x * CurrentMoveSpeed, _rigidBody2D.velocity.y);

        if (CheckIsGrounded() && !IsJumping)
        {
            IsGrounded = true;
        }

        // Update Vertical Velocity
        if (IsJumping)
        {
            _rigidBody2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            IsJumping = false;
        }
    }
}
