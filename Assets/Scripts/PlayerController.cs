using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using ExitGames.Client.Photon;
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
    [SerializeField] private float dashStamina = 0f;
    [SerializeField] private float maxDashStamina = 16f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playersLayer;
    [SerializeField] private Vector2 boxSize;
    [SerializeField] private float castDistance;
    [SerializeField] private Transform energyIndicator;

    private int score = 0;  // Player's score
    private int deathCount = 0;  // Player's death counter

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
    private int _ping;

    public float MaxEnergy => maxDashStamina;
    public float CurrentEnergy => dashStamina;
    public bool IsBossKilled => isBossKilled;
    public int Score => score;
    public int Ping => _ping;
    public string Nickname => playerNameText.text;

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

    [PunRPC]
    public void SetPlayerName(string name)
    {
        playerNameText.text = name;
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            // Load player properties and set them
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("score"))
            {
                score = (int)PhotonNetwork.LocalPlayer.CustomProperties["score"];
            }
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("deathCount"))
            {
                deathCount = (int)PhotonNetwork.LocalPlayer.CustomProperties["deathCount"];
            }

            // Sync player name
            photonView.RPC("SetPlayerName", RpcTarget.AllBuffered, PhotonNetwork.NickName);
        }
    }

    void Update()
    {
        if (health <= 0 || transform.position.y < -28)
        {
            if (_photonView != null && _photonView.IsMine)
            {
                _animator.enabled = false;
                ScreenFXManager.instance.EnablePlayerDiedEffects();
                PhotonNetwork.Destroy(_photonView);

                deathCount++;
                SetPlayerScoreAndDeaths();  // Save death count and score

                // Trigger automatic respawn
                SpawnPlayers spawnManager = FindObjectOfType<SpawnPlayers>();
                if (spawnManager != null)
                {
                    spawnManager.RespawnPlayer(); // Respawn player at their unique spawn point
                }
            }
        }

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
        _ping = PhotonNetwork.GetPing();
        FlipSprite();

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
        if (energyIndicator != null)
        {
            energyIndicator.localScale = new Vector3((CurrentEnergy / MaxEnergy) * 0.5f, 0.05f, 0f);
            energyIndicator.position = new Vector3(
                transform.localPosition.x - 0.25f + (CurrentEnergy / MaxEnergy) * 0.25f,
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
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
        if (collision.gameObject.CompareTag("Platform") && currentPlatform != null)
        {
            platformVelocity = currentPlatform.PlatformVelocity;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
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
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }
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
                    PhotonView.Get(enemy).RPC("TakeDamage", RpcTarget.All);
                }
                if (enemy.isDead)
                {
                    // Add points based on the type of enemy killed
                    if (collision.gameObject.CompareTag("Boss"))
                    {
                        score += 10; // Boss killed
                    }
                    else if (collision.gameObject.CompareTag("Enemy"))
                    {
                        score += 1; // Regular enemy killed
                    }

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

    public void SetPlayerScoreAndDeaths()
    {
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
        playerProperties["score"] = score;
        playerProperties["deathCount"] = deathCount;

        // Set custom properties for the player
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }


    [PunRPC]
    private void SyncFlipText(bool isFlipped)
    {
        Vector3 textScale = playerNameText.transform.localScale;
        if (isFlipped)
        {
            playerNameText.transform.localScale = new Vector3(-Mathf.Abs(textScale.x), textScale.y, textScale.z);
        }
        else
        {
            playerNameText.transform.localScale = new Vector3(Mathf.Abs(textScale.x), textScale.y, textScale.z);
        }
    }


    // Handles sprite flipping based on movement direction
    private void FlipSprite()
    {
        bool isFlipped = false;
        // Flip the player sprite based on movement direction
        if (moveInput.x < 0) // Moving left
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

            isFlipped = true;
        }
        else if (moveInput.x > 0) // Moving right
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);

            isFlipped = false;
        }

        // Synchronize the name text flipping across the network
        if (_photonView.IsMine && moveInput.x != 0)
        {
            photonView.RPC("SyncFlipText", RpcTarget.AllBuffered, isFlipped);
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(dashStamina);
            stream.SendNext(health);
            stream.SendNext(score); // Send score to other players
            stream.SendNext(deathCount); // Send death count to other players
            stream.SendNext(_ping);
        }
        if (stream.IsReading)
        {
            // Network player, receive data
            this.dashStamina = (float)stream.ReceiveNext();
            this.health = (int)stream.ReceiveNext();
            this.score = (int)stream.ReceiveNext(); // Receive score
            this.deathCount = (int)stream.ReceiveNext(); // Receive death count
            this._ping = (int)stream.ReceiveNext();
        }
    }
}
