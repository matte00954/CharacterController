using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    private Vector3 velocity;

    [Header("Debug variables")]
    [SerializeField] private Vector3 inspectorVelocity; //debug

    [Header("Movement variables")]
    private float movementSpeed = 6f;
    private float terminalVelocity = 14f;

    private float decelerationMultiplier = 2f;

    //Colliders
    [Header("Collision variables")]
    [SerializeField] private LayerMask collisionMask;
    private BoxCollider2D playerCollider;

    private float colliderMargin = 0.005f;
    private float groundCheckDistance = 0.0125f; //Longer than colliderMargin

    private float staticFrictionCoefficient = 0.35f;
    private float kineticFrictionCoefficient = 0.35f;
    private float airResistance = 0.5f;

    private RaycastHit2D groundedBoxCastHit;
    private RaycastHit2D collisionBoxCastHit;

    //Jumping
    private bool isJumping;
    private bool cancelJump;

    private float jumpSpeed = 12f;

    private float jumpTimer = 0f;

    private const float JUMP_MIN_TIMER = 0.03f;
    private const float JUMP_MAX_TIMER = 0.08f;

    //Gravity
    private float gravityValue = 10f;

    //Other
    private int collisionCounter;

    private void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        velocity = Vector3.zero;

        Vector2 input = Vector2.right * Input.GetAxisRaw("Horizontal");

        groundedBoxCastHit = Physics2D.BoxCast(transform.position, playerCollider.size, 0.0f, Vector2.down, groundCheckDistance, collisionMask);

        if (input.magnitude > float.Epsilon)
        {
            velocity = Accelerate(input);
        }
        else
        {
            velocity = Decelerate();
        }

        velocity += Gravity();

        JumpInput();

        if (isJumping == true)
        {
            velocity += Jump();
        }

        velocity += Friction();

        velocity *= Mathf.Pow(airResistance, Time.deltaTime);

        velocity = CollisionDetection(velocity);

        transform.position += velocity;

        //Ignore
        inspectorVelocity = velocity; //debugging
    }

    private Vector3 Gravity()
    {
        Vector3 gravity = Vector3.zero;
        gravity = new Vector3(0, -gravityValue * Time.deltaTime, 0);
        gravity = CollisionDetection(gravity);
        return gravity;
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

        Vector2 accelerate = Vector2.zero;

        accelerate += input * movementSpeed * Time.deltaTime;

        if (accelerate.magnitude > terminalVelocity)
        {
            accelerate = accelerate.normalized * terminalVelocity;
        }

        return accelerate;
    }

    private Vector3 Accelerate(Vector3 input)
    {
        Vector3 accelerate = Vector3.zero;

        accelerate += input * movementSpeed * Time.deltaTime;

        if (accelerate.magnitude > terminalVelocity)
        {
            accelerate = accelerate.normalized * terminalVelocity;
        }

        return accelerate;
    }

    private Vector2 Decelerate()
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

    private Vector3 CollisionDetection(Vector3 velocityValue)
    {
        Vector3 collisionVelocity = velocityValue;
        collisionBoxCastHit = Physics2D.BoxCast(transform.position, playerCollider.size, 0.0f, collisionVelocity.normalized, collisionVelocity.magnitude, collisionMask);
        float distanceToCollider = colliderMargin / Vector2.Dot(collisionVelocity.normalized, collisionBoxCastHit.normal);
        float allowedMovementDistance = collisionBoxCastHit.distance + distanceToCollider;

        if (collisionVelocity.magnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        if (allowedMovementDistance < collisionVelocity.magnitude && collisionCounter <= 25)
        {
            //checkCollisionNextFrame = true;
            collisionCounter++;
            Vector3 normal = (Vector3)NormalForceProjection(collisionVelocity, collisionBoxCastHit.normal);
            return CollisionDetection(collisionVelocity + normal);
        }
        else
        {
            collisionCounter = 0;
            return collisionVelocity;
        }
    }

    private Vector3 Friction()
    {
        Vector3 friction = Vector3.zero;

        Vector3 normalForce = NormalForceProjection(velocity, collisionBoxCastHit.normal);

        if (velocity.magnitude < normalForce.magnitude * staticFrictionCoefficient)
        {
            friction = Vector3.zero;
        }
        else
        {
            friction -= velocity.normalized * normalForce.magnitude * kineticFrictionCoefficient;
        }
        return friction;
    }

    private Vector2 NormalForceProjection(Vector2 velocityNormal, Vector2 normal)
    {
        if (Vector2.Dot(velocityNormal, normal) > 0)
        {
            return Vector2.zero;
        }
        else
        {
            Vector2 projection = Vector2.Dot(velocityNormal, normal) * normal;
            return -projection;
        }
    }
}