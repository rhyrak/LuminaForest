using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float range = 1.0f;
    [SerializeField] private float speed = 4.0f;
    [SerializeField] private bool startMovingLeft = true;

    private float _currentPos = 0.0f;
    private bool _moveLeft = true;
    private float _startingX;

    public Vector2 PlatformVelocity { get; private set; } // Exposed velocity

    private void Start()
    {
        _startingX = transform.position.x;

        // Set the initial position and direction based on startMovingLeft
        if (startMovingLeft)
        {
            _currentPos = 1.0f; // Start at the rightmost position
            _moveLeft = true;   // Move left initially
            _startingX -= range; // Shift starting position to the left
        }
        else
        {
            _currentPos = 0.0f; // Start at the leftmost position
            _moveLeft = false;  // Move right initially
        }
    }

    private void FixedUpdate()
    {
        var previousX = transform.position.x;

        // Move the platform
        if (_moveLeft)
        {
            _currentPos -= speed * Time.fixedDeltaTime / range;
        }
        else
        {
            _currentPos += speed * Time.fixedDeltaTime / range;
        }

        // Clamp and reverse at boundaries
        if (_currentPos >= 1.0f || _currentPos <= 0.0f)
        {
            _moveLeft = !_moveLeft;
            _currentPos = Mathf.Clamp(_currentPos, 0.0f, 1.0f);
        }

        // Update position and velocity
        var newX = _startingX + _currentPos * range;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        PlatformVelocity = new Vector2((newX - previousX) / Time.fixedDeltaTime, 0);
    }
}
