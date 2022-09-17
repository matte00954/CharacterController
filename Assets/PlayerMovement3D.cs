using UnityEngine;
using UnityEngine.Assertions;

public class PlayerMovement3D : MonoBehaviour
{
    [Header("Classes")]
    [SerializeField] private CameraController controller;

    [Header("Debug variables")]
    public Vector3 velocity;
    public bool grounded = false;

    [Header("Movement variables")]
    [Range(2f, 25.0f)] [SerializeField] private float movementSpeed = 6f;
    [Range(0.0f, 1.0f)] [SerializeField] private float jumpSpeed = 10f;
    [Range(0.0f, 1.0f)] [SerializeField] private float staticFrictionCoefficient = 0.35f;
    [Range(0.0f, 1.0f)] [SerializeField] private float kineticFrictionCoefficient = 0.35f;
    [Range(0.0f, 1.0f)] [SerializeField] private float airResistance = 0.5f;
    [Range(0.0f, 1.0f)] [SerializeField] private float castRange = 1f;
    [Range(0.0f, 1.0f)] [SerializeField] private float slopeAngleFactor;

    private CapsuleCollider playerCollider;

    private float jumpTimer;

    //Jumping
    private bool isJumping;
    private bool cancelJump;

    private const float JUMP_MIN_TIMER = 0.02f;
    private const float JUMP_MAX_TIMER = 0.1f;

    //Gravity
    [SerializeField] [Range(5f, 25f)] private float gravityForce = 15f;

    //Colliders
    [Header("Collision variables")]
    [SerializeField] private LayerMask collisionMask;
    private Vector3 upperLocalCapsulePoint => playerCollider.center + transform.up * (playerCollider.height * 0.5f - playerCollider.radius);
    private Vector3 lowerLocalCapsulePoint => playerCollider.center - transform.up * (playerCollider.height * 0.5f - playerCollider.radius);
    private Vector3 upperWorldCapsulePoint => transform.position + upperLocalCapsulePoint;
    private Vector3 lowerWorldCapsulePoint => transform.position + lowerLocalCapsulePoint;
    private float colliderMargin = 0.015f;
    private const int MAX_COLLISION_DETECTIONS = 30;

    //Inputs
    private float vertical;
    private float horizontal;

    private void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        RaycastHit groundHit;

        grounded = Physics.CapsuleCast(upperWorldCapsulePoint, lowerWorldCapsulePoint,
            playerCollider.radius, Vector3.down, out groundHit, castRange + colliderMargin, collisionMask);

        horizontal = Input.GetAxisRaw("Horizontal");

        vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = Vector3.right * horizontal + Vector3.forward * vertical;

        float inputMagnitude = input.magnitude;

        if (inputMagnitude > 1.0f)
            input.Normalize();

        Vector3 normal = grounded ? groundHit.normal : Vector3.up;

        input = Vector3.ProjectOnPlane(controller.transform.rotation * input,
            Vector3.Lerp(Vector3.up, normal, slopeAngleFactor).normalized * inputMagnitude);

        velocity += input * movementSpeed * Time.deltaTime;
        velocity += Vector3.down * gravityForce * Time.deltaTime;

        NormalForceProjection(velocity, groundHit.normal);

        #region Jump
        JumpInput();

        if (isJumping == true)
        {
            velocity += Jump();
        }
        #endregion

        velocity *= Mathf.Pow(airResistance, Time.deltaTime);

        CollisionDetection();

        Movement(velocity * Time.deltaTime);
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

    private Vector3 Jump() //Allows for different jump heights depending on input
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

        Vector3 value = Vector3.up * jumpSpeed;
        return value;
    }

    private void CollisionDetection() 
    {
        
        for (int count = 0; count < MAX_COLLISION_DETECTIONS && velocity.magnitude > 0.001f; count++)
        {
            RaycastHit hit;
            if(Physics.CapsuleCast(upperWorldCapsulePoint, lowerWorldCapsulePoint, playerCollider.radius, velocity.normalized, out hit, float.MaxValue, collisionMask))
            {
                float distanceToCollider = colliderMargin / Vector3.Dot(velocity.normalized, hit.normal);
                float allowedMoveDistance = hit.distance + distanceToCollider;

                if(allowedMoveDistance > velocity.magnitude * Time.deltaTime)
                {
                    return;
                }

                if (allowedMoveDistance > 0)
                {
                    transform.position += velocity.normalized * allowedMoveDistance;
                }

                Vector3 normalForce = NormalForceProjection(velocity, hit.normal);

                velocity += normalForce;

                Friction(normalForce);
            }
            else
            {
                return;
            }
        }

        velocity = Vector3.zero;
    }

    private void Movement(Vector3 movement)
    {

        Vector3 position = transform.position;

        transform.position += movement;

        for (int i = 0; i < 5; i++)
        {
            Collider[] colliders = Physics.OverlapCapsule(
                upperWorldCapsulePoint,
                lowerWorldCapsulePoint,
                playerCollider.radius,
                collisionMask);

            if(colliders.Length == 0)
            {
                return;
            }


            Vector3? seperation = null;
            foreach (Collider collider in colliders)
            {
                Vector3 direction;
                float distance;

                bool result = Physics.ComputePenetration(playerCollider,
                    transform.position, transform.rotation,
                    collider, collider.transform.position, collider.transform.rotation,
                    out direction, out distance);

                Assert.IsTrue(result);

                if(distance < (seperation?.magnitude ?? float.MaxValue))
                {
                    seperation = direction * distance;
                }
            }

            if (seperation.HasValue)
            {
                transform.position += seperation.Value + seperation.Value.normalized * colliderMargin;

                NormalForceProjection(velocity, seperation.Value.normalized);
            }
        }
        transform.position = position;
    }
    private void Friction(Vector3 normalForce)
    {
        if (velocity.magnitude < normalForce.magnitude * staticFrictionCoefficient)
        {
            velocity = Vector3.zero;
        }
        else
        {
            velocity -= velocity.normalized * normalForce.magnitude * kineticFrictionCoefficient;
        }
    }

    private Vector3 NormalForceProjection(Vector3 force, Vector3 normal)
    {
        if (Vector3.Dot(force, normal) > 0)
        {
            return Vector3.zero;
        }
        else
        {
            Vector3 projection = Vector3.Dot(force, normal) * normal;
            return -projection;
        }
    }
}