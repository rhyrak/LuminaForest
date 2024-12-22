using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
public class SlimeController : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private float walkSpeed = 4f;
    private Rigidbody2D _rigidBody2D;
    private Collider2D _collider2D;
    private Animator _animator;
    private Light2D _light2D;
    private int walkDirection = 1;
    public int health = 1;
    public bool isDead = false;
    public Vector2 velocity;

    // Event to notify when this slime dies
    public event System.Action<SlimeController> OnDeath;

    private void Awake()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<CapsuleCollider2D>();
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

    [PunRPC]
    public void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            PhotonView.Get(this).RPC("Die", RpcTarget.All);
        }
    }

    [PunRPC]
    public void Die()
    {
        if (isDead) return;

        isDead = true;
        if (gameObject.tag == "Boss")
        {
            StartCoroutine(UnlockPlatforms.instance.ResetBoss());
        }
        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
        _animator.enabled = false;
        _light2D.intensity = 0.0f;
        _rigidBody2D.velocity = Vector2.zero;
        OnDeath?.Invoke(null);
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DelayedSlimeDespawn());
        }

    }

    private IEnumerator DelayedSlimeDespawn()
    {
        yield return new WaitForSeconds(2f);
        PhotonNetwork.Destroy(this.gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(isDead);
            stream.SendNext(health);
        }
        if (stream.IsReading)
        {
            // Network player, receive data
            this.isDead = (bool)stream.ReceiveNext();
            this.health = (int)stream.ReceiveNext();
            if (health <= 0)
            {
                Die();
            }
        }
    }
}
