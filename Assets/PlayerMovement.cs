using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private BoxCollider2D playerCollider;

    [SerializeField] private Vector3 direction;
    [SerializeField] private LayerMask collisionMask;

    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float colliderMargin = 0.05f;

    [Tooltip("Ground check distance is collider margin variable multiplied by this variable")]
    [SerializeField] [Range(1.25f, 5f)] private float groundCheckDistanceMultiplier = 2f;

    private float groundCheckDistance = -1f;

    [SerializeField] private bool grounded;

    private void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
        groundCheckDistance = colliderMargin * groundCheckDistanceMultiplier; //ser till att groundcheck alltid är större än collider margin
        Debug.Log("groundCheckDistance : " + groundCheckDistance);
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        direction = new Vector3(horizontal, 0, 0).normalized;

        grounded = GroundCheck();

        float distance = movementSpeed * Time.deltaTime;

        Vector3 movement = direction * distance;

        movement -= Collision(movement);

        movement += Gravity();

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            movement += Jump();
        }

        transform.position += movement;
    }

    private Vector3 Gravity()
    {
        Vector3 gravity = Physics.gravity * Time.deltaTime;

        gravity -= Collision(gravity);

        return gravity;
    }

    private Vector3 Jump()
    {
        Vector3 jumpVelocity = transform.up * jumpSpeed;
        return jumpVelocity;
    }

    private Vector3 Projection(Vector2 velocity, Vector2 normal) 
    {
        //Handling slopes
        if (Vector2.Dot(velocity, normal) > 0)
        {
            return Vector2.zero;
        }
        else
        {
            Vector2 projection = Vector2.Dot(velocity, normal) * normal;
            return -projection;
        }
    }

    private bool GroundCheck()
    {
        return Physics2D.BoxCast(transform.position, playerCollider.size, 0.0f, Vector2.down, groundCheckDistance, collisionMask);
    }

    private Vector3 Collision(Vector3 movement)
    {
        RaycastHit2D boxCast = Physics2D.BoxCast(transform.position, playerCollider.size, 0.0f, movement.normalized, movement.magnitude + colliderMargin, collisionMask);

        Vector3 result = Vector3.zero;

        if (boxCast)
        {
            //return Projection(movement, boxCast.normal);
            result = movement;
        }
        else
        {
            //return movement * (boxCast.distance - colliderMargin);
            result = movement * (boxCast.distance - colliderMargin);
        }
        result -= Projection(movement, boxCast.normal); // Velocity + (-Projection) = NormalForce(new velocity)
        return result;
    }
}