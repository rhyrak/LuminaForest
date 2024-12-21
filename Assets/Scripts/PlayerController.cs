using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Photon.Pun;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float airGlideSpeed = 2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private int health = 2;
    [SerializeField] private float dashStrength = 3f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashStamina = 0f;
    [SerializeField] private float maxDashStamina = 16f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playersLayer;
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float castDistance;

    private PhotonView _photonView;
    private Rigidbody2D _rigidBody2D;
    private Animator _animator;
    private Vector3 originalScale;
    private Vector2 moveInput;
    private Vector2 baseVelocity;
    private float dashTimer = 0f;
    private bool regenerating = false;
    private bool _isMoving = false;
    private bool _isRunning = false;
    private bool _isGrounded = false;
    private bool _isJumping = false;
    private bool _isDashing = false;
    private bool isBossKilled = false;

    public float MaxEnergy => maxDashStamina;
    public float CurrentEnergy => dashStamina;
    public bool IsBossKilled => isBossKilled;

    private Vector2 platformVelocity = Vector2.zero; // Velocity inherited from platform
    private MovingPlatform currentPlatform;

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
    public bool IsDashing
    {
        get
        {
            return _isDashing;
        }
        private set
        {
            _isDashing = value;
            _animator.SetBool("isDashing", value);
        }
    }
    public float CurrentMoveSpeed
    {
        get
        {
            if (IsMoving)
                if (!IsGrounded)
                    return airGlideSpeed;
                else
                    return IsRunning ? runSpeed : walkSpeed;
            else
                return 0;
        }
    }

    void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        originalScale = transform.localScale;
        _photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (health <= 0 || transform.position.y < -28)
        {
            _animator.enabled = false;
            ScreenFXManager.instance.EnablePlayerDiedEffects();
            Destroy(gameObject);
        }

        FlipSprite();

        if (IsDashing)
            dashTimer += Time.deltaTime;
        if (dashTimer >= dashDuration)
        {
            IsDashing = false;
            dashTimer = 0f;
        }

        if (regenerating)
        {
            dashStamina += Time.deltaTime * 2;
            if (dashStamina > maxDashStamina)
            {
                dashStamina = maxDashStamina;
            }
        }
    }

    void FixedUpdate()
    {
        // Update Horizontal Velocity
        if (IsDashing)
        {
            float dashDirection = transform.localScale.x;
            baseVelocity = new Vector2(dashDirection * dashStrength, 0);
        }
        else
        {
            baseVelocity = new Vector2(moveInput.x * CurrentMoveSpeed, _rigidBody2D.velocity.y);
        }

        // update velocity
        _rigidBody2D.velocity = baseVelocity + platformVelocity;

        if (CheckIsGrounded() && !IsJumping)
        {
            IsGrounded = true;
        }
        else
        {
            _isGrounded = false;
            currentPlatform = null;
            platformVelocity = Vector2.zero;
        }

        // Update Vertical Velocity
        if (IsJumping)
        {
            _rigidBody2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            IsJumping = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (collision.gameObject.CompareTag("Platform"))
        {
            currentPlatform = collision.gameObject.GetComponent<MovingPlatform>();
            if (currentPlatform != null)
            {
                platformVelocity = currentPlatform.PlatformVelocity;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform") && currentPlatform != null)
        {
            platformVelocity = currentPlatform.PlatformVelocity;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            if (collision.gameObject.GetComponent<MovingPlatform>() == currentPlatform)
            {
                platformVelocity = Vector2.zero;
                currentPlatform = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ManaZone"))
        {
            regenerating = true;
        }

        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
        {
            SlimeController enemy = collision.gameObject.GetComponent<SlimeController>();
            if (!enemy.isDead)
            {
                // Decrement health
                if (!IsDashing) /** Player is invincible whilst dashing */
                {
                    health--;
                }
                else
                {
                    enemy.TakeDamage();
                }
                if (enemy.isDead && collision.gameObject.CompareTag("Boss"))
                {
                    isBossKilled = true;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (collision.gameObject.CompareTag("ManaZone"))
        {
            regenerating = false;
        }
    }

    // Handles movement
    public void OnMove(InputAction.CallbackContext context)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        moveInput = context.ReadValue<Vector2>();

        IsMoving = moveInput.x != 0;
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
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
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (context.started && CheckIsGrounded())
        {
            IsJumping = true; // Set the jump flag
            IsGrounded = false; // Set the grounded flag to false
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (context.started && dashStamina >= 2f && !IsDashing)
        {
            IsDashing = true;
            ScreenFXManager.instance.RunChromaticAberration(1.0f);
            dashStamina -= 2f;
            _rigidBody2D.velocity = new Vector2(moveInput.x * dashStrength, 0);
            _rigidBody2D.AddForce(new Vector2(dashStrength, 0), ForceMode2D.Force);
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
            return true;
        var playerHits = Physics2D.BoxCastAll(transform.position, boxSize, 0, -transform.up, castDistance, playersLayer);
        if (playerHits.Length > 1)
            return true;
        return false;
    }

    private void OnDrawGizmos()
    {
        if (gameObject == null)
        {
            return;
        }

        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }
}
