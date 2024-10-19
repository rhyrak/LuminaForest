using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] public float range = 1.0f;
    [SerializeField] public float speed = 4.0f;

    private float currentPos = 0.0f;
    private bool moveLeft = true;
    private float startingX;

    void Start()
    {
        startingX = transform.position.x;
    }

    void FixedUpdate()
    {
        if (moveLeft)
        {
            currentPos += 0.001f * speed;
        }
        else
        {
            currentPos -= 0.001f * speed;
        }
        if (currentPos >= 1.0f || currentPos <= 0.0f)
        {
            moveLeft = !moveLeft;
            Mathf.Clamp(currentPos, 0.0f, 1.0f);
        }
        transform.position = new(startingX + currentPos * range, transform.position.y, transform.position.z);
    }
}
