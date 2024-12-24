using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float airGlideSpeed = 2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private int health = 2;
    [SerializeField] private float dashStrength = 3f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashStamina;
    [SerializeField] private float maxDashStamina = 16f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playersLayer;
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float castDistance;
    [SerializeField] private Transform energyIndicator;

    private PhotonView _photonView;
    private Rigidbody2D _rigidBody2D;
    private Animator _animator;
    private Vector3 _originalScale;
    private Vector2 _moveInput;
    private Vector2 _baseVelocity;
    private float _dashTimer;
    private bool _regenerating;
    private bool _isMoving;
    private bool _isRunning;
    private bool _isGrounded;
    private bool _isJumping;
    private bool _isDashing;

    private Vector2 _platformVelocity = Vector2.zero; // Velocity inherited from platform
    private MovingPlatform _currentPlatform;
    
    public int Score { get; private set; }
    public int Ping { get; private set; }
    public int DeathCount { get; private set; }
    public string Nickname => playerNameText.text;
    
    private bool IsMoving
    {
        get => _isMoving;
        set
        {
            _isMoving = value;
            _animator.SetBool("isMoving", value);
        }
    }

    private bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            _animator.SetBool("isRunning", value);
        }
    }

    private bool IsGrounded
    {
        get => _isGrounded;
        set
        {
            _isGrounded = value;
            _animator.SetBool("isGrounded", value);
        }
    }

    private bool IsJumping
    {
        get => _isJumping;
        set
        {
            _isJumping = value;
            _animator.SetBool("isJumping", value);
        }
    }

    private bool IsDashing
    {
        get => _isDashing;
        set
        {
            _isDashing = value;
            _animator.SetBool("isDashing", value);
        }
    }

    private float CurrentMoveSpeed
    {
        get
        {
            if (!IsMoving) return 0;
            if (!IsGrounded)
                return airGlideSpeed;
            return IsRunning ? runSpeed : walkSpeed;
        }
    }

    void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _originalScale = transform.localScale;
        _photonView = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void Start()
    {
        if (!photonView.IsMine) return;
        // Load player properties and set them
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("score"))
        {
            Score = (int)PhotonNetwork.LocalPlayer.CustomProperties["score"];
        }
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("deathCount"))
        {
            DeathCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["deathCount"];
        }

        // Sync player name
        photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
    }

    public void Update()
    {
        if (health <= 0 || transform.position.y < -28)
        {
            if (_photonView != null && _photonView.IsMine)
            {
                _animator.enabled = false;
                ScreenFXManager.Instance.EnablePlayerDiedEffects();
                PhotonNetwork.Destroy(_photonView);

                DeathCount++;
                SetPlayerScoreAndDeaths();  // Save death count and score

                // Trigger automatic respawn
                var spawnManager = FindObjectOfType<SpawnPlayers>();
                if (spawnManager != null)
                {
                    spawnManager.RespawnPlayer(); // Respawn player at their unique spawn point
                }
            }
        }

        if (IsDashing)
            _dashTimer += Time.deltaTime;
        if (_dashTimer >= dashDuration)
        {
            IsDashing = false;
            _dashTimer = 0f;
        }

        if (_regenerating)
        {
            dashStamina += Time.deltaTime * 2;
            if (dashStamina > maxDashStamina)
            {
                dashStamina = maxDashStamina;
            }
        }
    }

    public void FixedUpdate()
    {
        if (_photonView != null && _photonView.IsMine)
            Ping = PhotonNetwork.GetPing();
        
        FlipSprite();

        // Update Horizontal Velocity
        if (IsDashing)
        {
            var dashDirection = transform.localScale.x;
            _baseVelocity = new Vector2(dashDirection * dashStrength, 0);
        }
        else
        {
            _baseVelocity = new Vector2(_moveInput.x * CurrentMoveSpeed, _rigidBody2D.velocity.y);
        }

        // update velocity
        _rigidBody2D.velocity = _baseVelocity + _platformVelocity;

        if (CheckIsGrounded() && !IsJumping)
        {
            IsGrounded = true;
        }
        else
        {
            _isGrounded = false;
            _currentPlatform = null;
            _platformVelocity = Vector2.zero;
        }

        // Update Vertical Velocity
        if (IsJumping)
        {
            _rigidBody2D.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            IsJumping = false;
        }
        if (energyIndicator != null)
        {
            energyIndicator.localScale = new Vector3((dashStamina / maxDashStamina) * 0.5f, 0.05f, 0f);
            energyIndicator.position = new Vector3(
                transform.localPosition.x - 0.25f + (dashStamina / maxDashStamina) * 0.25f,
                transform.localPosition.y + 0.5f, energyIndicator.position.z);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }

        if (!collision.gameObject.CompareTag("Platform")) return;
        _currentPlatform = collision.gameObject.GetComponent<MovingPlatform>();
        if (_currentPlatform != null)
        {
            _platformVelocity = _currentPlatform.PlatformVelocity;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (collision.gameObject.CompareTag("Platform") && _currentPlatform != null)
        {
            _platformVelocity = _currentPlatform.PlatformVelocity;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (!collision.gameObject.CompareTag("Platform")) return;
        if (collision.gameObject.GetComponent<MovingPlatform>() != _currentPlatform) return;
        _platformVelocity = Vector2.zero;
        _currentPlatform = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (collision.gameObject.CompareTag("ManaZone"))
        {
            _regenerating = true;
        }

        if (!collision.gameObject.CompareTag("Enemy") && !collision.gameObject.CompareTag("Boss")) return;
        var enemy = collision.gameObject.GetComponent<SlimeController>();
        if (enemy.isDead) return;
        // Decrement health
        if (!IsDashing) // Player is invincible whilst dashing
        {
            health--;
        }
        else
        {
            PhotonView.Get(enemy).RPC("TakeDamage", RpcTarget.All);
        }

        if (!enemy.isDead) return;
        // Add points based on the type of enemy killed
        if (collision.gameObject.CompareTag("Boss"))
        {
            Score += 50; // Boss killed
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            Score += 1; // Regular enemy killed
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
            _regenerating = false;
        }
    }

    // Handles movement
    public void OnMove(InputAction.CallbackContext context)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        _moveInput = context.ReadValue<Vector2>();

        IsMoving = _moveInput.x != 0;
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

        if (!context.started || !CheckIsGrounded()) return;
        IsJumping = true; // Set the jump flag
        IsGrounded = false; // Set the grounded flag to false
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }

        if (!context.started || !(dashStamina >= 2f) || IsDashing) return;
        IsDashing = true;
        ScreenFXManager.Instance.RunChromaticAberration(1.0f);
        dashStamina -= 2f;
        _rigidBody2D.velocity = new Vector2(_moveInput.x * dashStrength, 0);
        _rigidBody2D.AddForce(new Vector2(dashStrength, 0), ForceMode2D.Force);
    }

    private void SetPlayerScoreAndDeaths()
    {
        var playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            ["score"] = Score,
            ["deathCount"] = DeathCount
        };

        // Set custom properties for the player
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }


    [PunRPC]
    private void SyncFlipText(bool isFlipped)
    {
        var textScale = playerNameText.transform.localScale;
        playerNameText.transform.localScale = isFlipped ? new Vector3(-Mathf.Abs(textScale.x), textScale.y, textScale.z)
            : new Vector3(Mathf.Abs(textScale.x), textScale.y, textScale.z);
    }


    // Handles sprite flipping based on movement direction
    private void FlipSprite()
    {
        var isFlipped = false;
        switch (_moveInput.x)
        {
            // Flip the player sprite based on movement direction
            // Moving left
            case < 0:
                transform.localScale = new Vector3(-Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
                isFlipped = true;
                break;
            // Moving right
            case > 0:
                transform.localScale = new Vector3(Mathf.Abs(_originalScale.x), _originalScale.y, _originalScale.z);
                break;
        }

        // Synchronize the name text flipping across the network
        if (_photonView.IsMine && _moveInput.x != 0)
            photonView.RPC("SyncFlipText", RpcTarget.AllBuffered, isFlipped);
        
    }

    private bool CheckIsGrounded()
    {
        if (Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, castDistance, groundLayer))
            return true;
        var playerHits = Physics2D.BoxCastAll(transform.position, boxSize, 0, -transform.up, castDistance, playersLayer);
        return playerHits.Length > 1;
    }

    private void OnDrawGizmos()
    {
        if (gameObject == null)
        {
            return;
        }

        Gizmos.DrawWireCube(transform.position - transform.up * castDistance, boxSize);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(dashStamina);
            stream.SendNext(health);
            stream.SendNext(Score); // Send score to other players
            stream.SendNext(DeathCount); // Send death count to other players
            stream.SendNext(Ping);
        }
        if (stream.IsReading)
        {
            // Network player, receive data
            dashStamina = (float)stream.ReceiveNext();
            health = (int)stream.ReceiveNext();
            Score = (int)stream.ReceiveNext(); // Receive score
            DeathCount = (int)stream.ReceiveNext(); // Receive death count
            Ping = (int)stream.ReceiveNext();
        }
    }
}
