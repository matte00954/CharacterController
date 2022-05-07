using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Debug variables")]
    [SerializeField] private Vector3 inspectorVelocity; //debug

    [Header("Movement variables")]
    private float movementSpeed = 4f;
    private float terminalVelocity = 10f;

    private float decelerationMultiplier = 2f;

    //Colliders
    [Header("Collision variables")]
    [SerializeField] private LayerMask collisionMask;
    private BoxCollider2D playerCollider;

    private float colliderMargin = 0.005f;
    private float groundCheckDistance = 0.0125f; //Longer than colliderMargin

    private RaycastHit2D groundedBoxCastHit;
    private RaycastHit2D collisionBoxCastHit;

    //Jumping
    private bool isJumping;
    private bool cancelJump;

    private float jumpSpeed = 15f;

    private float jumpTimer = 0f;

    private const float JUMP_MIN_TIMER = 0.03f;
    private const float JUMP_MAX_TIMER = 0.08f;

    //Gravity
    private float gravityValue = 10f;

    //Other
    private bool checkCollisionNextFrame;

    private void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        Vector2 input = Vector2.right * Input.GetAxisRaw("Horizontal");

        Vector3 velocity = Vector3.zero;

        groundedBoxCastHit = Physics2D.BoxCast(transform.position, playerCollider.size, 0.0f, Vector2.down, groundCheckDistance, collisionMask);

        if (input.magnitude > float.Epsilon)
        {
            velocity = Accelerate(input);
        }
        else
        {
            velocity = Decelerate(velocity);
        }

        velocity += Gravity(velocity);

        JumpInput();

        if (isJumping == true)
        {
            velocity += Jump();
        }

        velocity = CollisionDetection(velocity);

        transform.position += velocity;

        //Ignore
        inspectorVelocity = velocity; //debugging
    }

    private Vector3 Gravity(Vector3 velocity)
    {
        Vector3 gravity = Vector3.zero;

        if (groundedBoxCastHit == false) //Gravity
        {
            gravity = new Vector3(0, -gravityValue * Time.deltaTime, 0);
            velocity += gravity;
        }

        return velocity;
    }

    private void JumpInput() //Only checks jump input
    {
        if (groundedBoxCastHit && Input.GetKeyDown(KeyCode.Space) && isJumping == false) //Jump
        {
            isJumping = true;
            cancelJump = false;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            cancelJump = true;
        }
    }

    private Vector3 Jump() //Changes velocity based on jump input
    {
        jumpTimer += Time.deltaTime;

        if (cancelJump)
        {
            if (jumpTimer > JUMP_MIN_TIMER)
            {
                //Canceled jump
                jumpTimer = 0;
                isJumping = false;
                cancelJump = false;
                return Vector3.zero;
            }
        }
        else if (jumpTimer > JUMP_MAX_TIMER)
        {
            //Full jump
            jumpTimer = 0;
            isJumping = false;
            return Vector3.zero;
        }

        Vector3 value = Accelerate(Vector3.up * jumpSpeed);
        return value;
    }

    private Vector2 Accelerate(Vector2 input)
    {

        Vector2 velocity = Vector2.zero;

        velocity += input * movementSpeed * Time.deltaTime;

        if (velocity.magnitude > terminalVelocity)
        {
            velocity = velocity.normalized * terminalVelocity;
        }

        return velocity;
    }

    private Vector3 Accelerate(Vector3 input)
    {

        Vector3 velocity = Vector3.zero;

        velocity += input * movementSpeed * Time.deltaTime;

        if (velocity.magnitude > terminalVelocity)
        {
            velocity = velocity.normalized * terminalVelocity;
        }

        return velocity;
    }

    private Vector2 Decelerate(Vector3 velocity)
    {
        Vector3 projection = new Vector3(velocity.x, 0.0f, velocity.z).normalized;

        Vector3 deceleration = velocity;

        deceleration -= projection * decelerationMultiplier * Time.deltaTime;

        if (deceleration.x > velocity.x)
        {
            return velocity;
        }

        return deceleration;
    }

    private Vector3 CollisionDetection(Vector3 velocity)
    {
        collisionBoxCastHit = Physics2D.BoxCast(transform.position, playerCollider.size, 0.0f, velocity.normalized, velocity.magnitude, collisionMask);
        float distanceToCollider = colliderMargin / Vector2.Dot(velocity.normalized, collisionBoxCastHit.normal);
        float allowedMovementDistance = collisionBoxCastHit.distance + distanceToCollider;

        if (checkCollisionNextFrame == true)
        {
            Vector3 normal = (Vector3)NormalForceProjection(velocity, collisionBoxCastHit.normal);

            if (collisionBoxCastHit)
            {
                checkCollisionNextFrame = false;
            }

            return velocity + normal;
        }

        if (velocity.magnitude < 0.0001f)
        {
            return velocity;
        }

        if (allowedMovementDistance < velocity.magnitude)
        {
            checkCollisionNextFrame = true;
            Vector3 normal = (Vector3)NormalForceProjection(velocity, collisionBoxCastHit.normal);
            return velocity + normal;
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