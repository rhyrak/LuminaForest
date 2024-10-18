using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 8f;

    public bool IsGrounded { get; private set; } = true;
    public bool IsWalking { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;
    private Rigidbody2D _rigidBody2D;
    private int counter = 0;

    void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        _rigidBody2D.velocity = new Vector2(walkSpeed * Input.GetAxis("Horizontal"), _rigidBody2D.velocity.y);
        if (IsGrounded && Input.GetButton("Jump"))
        {
            _rigidBody2D.velocity += new Vector2(0, runSpeed);
            IsGrounded = false;
        }
        if (!IsGrounded)
        {
            counter++;
            if (counter == 10)
            {
                IsGrounded = true;
                counter = 0;
            }
        }
    }
}
