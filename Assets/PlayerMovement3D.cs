using UnityEngine;

public class PlayerMovement3D : MonoBehaviour
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
    private CapsuleCollider playerCollider;
    private Vector3 capsulePointOne;
    private Vector3 capsulePointTwo;

    [SerializeField] private float castRange = 1f;

    private float colliderMargin = 0.015f;
    //private float groundCheckDistance = 0.0125f; //Longer than colliderMargin

    private float staticFrictionCoefficient = 0.35f;
    private float kineticFrictionCoefficient = 0.35f;
    private float airResistance = 0.5f;

    private float vertical;
    private float horizontal;

    [SerializeField] private bool grounded;

    //Jumping
    private bool isJumping;
    private bool cancelJump;

    private float jumpSpeed = 10f;

    private float jumpTimer = 0f;

    private const float JUMP_MIN_TIMER = 0.02f;
    private const float JUMP_MAX_TIMER = 0.1f;

    //Gravity
    private float gravityValue = -5f;

    //Other
    private int collisionCounter;

    private void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        velocity = Vector3.zero;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        //playerCamera.transform.eulerAngles += new Vector3(0, rotationY, 0) * Time.deltaTime;

        //Vector3 input = Vector3.right * horizontal + Vector3.forward * vertical; //FEL!!!

        Vector3 input = transform.right * horizontal + transform.forward * vertical; 

        capsulePointOne = playerCollider.center + transform.up * (playerCollider.height / 2 - playerCollider.radius);

        capsulePointTwo = playerCollider.center + -transform.up * (playerCollider.height / 2 - playerCollider.radius);

        grounded = Physics.CapsuleCast(capsulePointOne, capsulePointTwo, playerCollider.radius, -transform.up, castRange, collisionMask);

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

        transform.localPosition += velocity;

        //Ignore
        inspectorVelocity = velocity; //debugging
    }

    private Vector3 Gravity()
    {
        Vector3 gravity = Vector3.zero;
        gravity = new Vector3(0, gravityValue * Time.deltaTime, 0);
        gravity = CollisionDetection(gravity);
        return gravity;
    }

    private void JumpInput() //Only checks jump input
    {
        if (grounded && Input.GetKeyDown(KeyCode.Space) && isJumping == false) //Jump
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

    private Vector3 Decelerate()
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

    private Vector3 CollisionDetection(Vector3 velocityParameter) //Parameter needed for recursion 
    {

        Vector3 collisionVelocity = velocityParameter;

        //RaycastHit hit;

        //Physics.CapsuleCast(capsulePointOne, capsulePointTwo, playerCollider.radius - Physics.defaultContactOffset, collisionVelocity.normalized, out hit, castRange, collisionMask);

        Collider[] colliders = Physics.OverlapCapsule(capsulePointOne, capsulePointTwo, playerCollider.radius - colliderMargin, collisionMask);

        if (colliders.Length > 1)
        {
            Vector3 separationVector = Vector3.zero;
            float distance = 0f;

            foreach (Collider item in colliders)
            {
                if(Physics.ComputePenetration(playerCollider, transform.position, transform.rotation,
                    item, item.transform.position, item.transform.rotation, out separationVector, out distance))
                {                
                    //transform.position += NormalForceProjection(collisionVelocity, separationVector.normalized);
                    return NormalForceProjection(collisionVelocity, separationVector.normalized);
                }
            }
            return collisionVelocity;
        }
        else
        {
            RaycastHit hit;

            //Physics.CapsuleCast(capsulePointOne, capsulePointTwo, playerCollider.radius - colliderMargin, collisionVelocity.normalized, out hit, castRange, collisionMask);

            Physics.BoxCast(transform.position, transform.position, collisionVelocity, out hit, Quaternion.identity, castRange, collisionMask);

            float distanceToCollider = colliderMargin / Vector3.Dot(collisionVelocity.normalized, hit.normal);

            float allowedMovementDistance = hit.distance + distanceToCollider;

            if (collisionVelocity.magnitude < 0.0001f)
            {
                return Vector3.zero;
            }

            if (allowedMovementDistance < collisionVelocity.magnitude && collisionCounter <= 10)
            {
                collisionCounter++;
                Vector3 normal = (Vector3)NormalForceProjection(collisionVelocity, hit.normal);
                Debug.Log("asdasdasdasd");
                return CollisionDetection(collisionVelocity + normal);
            }
            else
            {
                collisionCounter = 0;
                return collisionVelocity;
            }
        }
    }

    private Vector3 Friction()
    {
        Vector3 friction = Vector3.zero;
        RaycastHit hit;
        Physics.CapsuleCast(capsulePointOne, capsulePointTwo, playerCollider.radius - Physics.defaultContactOffset, velocity.normalized, out hit, castRange, collisionMask);
        //Physics.BoxCast(transform.position, playerCollider.size, Vector3.down, out hit, Quaternion.identity, velocity.magnitude, collisionMask);
        Vector3 normalForce = NormalForceProjection(velocity, hit.normal);

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

    private Vector3 NormalForceProjection(Vector3 velocityNormal, Vector3 normal)
    {
        if (Vector3.Dot(velocityNormal, normal) > 0)
        {
            return Vector3.zero;
        }
        else
        {
            Vector3 projection = Vector3.Dot(velocityNormal, normal) * normal;
            return -projection;
        }
    }
}
