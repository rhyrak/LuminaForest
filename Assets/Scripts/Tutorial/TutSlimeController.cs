using UnityEngine.Rendering.Universal;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TutSlimeController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4f;
    private Rigidbody2D _rigidBody2D;
    private Animator _animator;
    private Light2D _light2D;
    private int walkDirection = 1;
    public int health = 1;
    public bool isDead = false;
    public Vector2 velocity;

    private void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _light2D = GetComponentInChildren<Light2D>();
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

    void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }
        _rigidBody2D.velocity = new Vector2(walkSpeed * walkDirection, _rigidBody2D.velocity.y);
        velocity = _rigidBody2D.velocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("InvisibleWall"))
        {
            walkDirection *= -1;
        }
    }

    public void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
        _animator.enabled = false;
        _light2D.intensity = 0.0f;
        _rigidBody2D.velocity = Vector2.zero;
    }
}