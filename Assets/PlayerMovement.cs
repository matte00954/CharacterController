using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private BoxCollider2D playerCollider;
    
    [SerializeField] private Vector3 inspectorVelocity; //debug

    [SerializeField] private LayerMask collisionMask;

    [SerializeField] private float movementSpeed = 2f;

    [SerializeField] private float jumpHeight = 20f;

    private float horizontalInput;

    private float colliderMargin = 0.005f;

    private float groundCheckDistance = 0.0125f; //Longer than colliderMargin

    private RaycastHit2D groundedBoxCastHit;

    private RaycastHit2D collisionBoxCastHit;

    private void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        groundedBoxCastHit = Physics2D.BoxCast(transform.position, playerCollider.size, 0.0f, Vector2.down, groundCheckDistance, collisionMask);

        float gravityValue = 0;

        if (groundedBoxCastHit == false) //Gravity
        {
            gravityValue = -1f;
        }

        if (groundedBoxCastHit == true && Input.GetKeyDown(KeyCode.Space)) //Jump
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + jumpHeight, transform.position.z);
        }

        Vector3 direction = new Vector3(horizontalInput, gravityValue, 0).normalized;

        Vector3 velocity = direction * movementSpeed * Time.deltaTime;

        velocity = CollisionDetection(velocity);

        transform.position += velocity;

        //
        inspectorVelocity = velocity; //debugging
        //

    }

    private Vector3 CollisionDetection(Vector3 velocity)
    {
        collisionBoxCastHit = Physics2D.BoxCast(transform.position, playerCollider.size, 0.0f, velocity.normalized, velocity.magnitude, collisionMask);

        if (velocity.magnitude < 0.001f)
        {
            return velocity;
        }
        else if (collisionBoxCastHit)
        {
            //return CollisionDetection(velocity + (Vector3)NormalForceProjection(velocity, collisionBoxCastHit.normal)); //Rekursiv lösning, verkar inte göra något annorlunda???
            return velocity + (Vector3)NormalForceProjection(velocity, collisionBoxCastHit.normal);
        }
        else
        {
            return velocity;
        }
    }

    private Vector2 NormalForceProjection(Vector2 velocity, Vector2 normal)
    {
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

}

/*
private BoxCollider2D playerCollider;s

[SerializeField] private Vector3 direction;
[SerializeField] private Vector3 velocity;
[SerializeField] private LayerMask collisionMask;

[SerializeField] private float movementSpeed = 2f;
[SerializeField] private float jumpSpeed = 20f;
[SerializeField] private float colliderMargin = 0.05f;

[Tooltip("Ground check distance is collider margin variable multiplied by this variable")]
[SerializeField] [Range(1.25f, 5f)] private float groundCheckDistanceMultiplier = 2f;

//private RaycastHit2D boxCast;

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

    movement += Collision(movement);

    movement += Gravity();

    if (Input.GetKeyDown(KeyCode.Space) && grounded)
    {
        movement += Jump();
    }

    velocity = movement;

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

    if (movement.magnitude < 0.001f)
    {
        return result;
    }
    else if (boxCast)
    {
        Debug.Log("test");
        Vector3 normalForce = Projection(movement, boxCast.normal);
        result = movement + normalForce;
        return Collision(result);

        //Velocity + (-Projection) = NormalForce(new velocity)
        //Vector3 movement = movement * (boxCast.distance - colliderMargin);
    }
    else 
    {
        return movement;
    }
}*/