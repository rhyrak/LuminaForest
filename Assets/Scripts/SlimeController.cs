using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    private Rigidbody2D _rigidBody2D;
    private int walkDirection = 1;

    private void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        int rand = Random.Range(-1, 1);
        if (rand > 0)
        {
            walkDirection = 1;
        }
        else
        {
            walkDirection = -1;
        }
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        _rigidBody2D.velocity = new Vector2(walkSpeed * walkDirection, _rigidBody2D.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("InvisibleWall"))
        {
            walkDirection *= -1;
        }
    }
}
