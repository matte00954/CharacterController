using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;

    [SerializeField] private Transform idealTransform;

    [SerializeField] private LayerMask collision;
    
    [SerializeField] [Range (50f,200f)] private float mouseSensitivity = 100f;

    private RaycastHit hit;

    private Vector3 direction;

    private Vector3 target;

    private Vector3 playerVelocity;

    private float xRotation = 0f;

    private float height;

    private float distance;

    [SerializeField] [Range(0.0001f, 1f)] private float smoothTime;

    public static CameraController Instance { get; private set; }
    public Vector3 PlayerVelocity { get => playerVelocity; set => playerVelocity = value; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        distance = Vector3.Distance(player.position, transform.position);
    }

    private void LateUpdate()
    {
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 45f);

        player.transform.Rotate(Vector3.up * mouseX);

        height = player.localPosition.y + 1.8f;

        direction = (player.position - idealTransform.position).normalized;

        transform.rotation = player.transform.rotation;

        if (Physics.Raycast(player.position, -direction, out hit, distance, collision)) 
        {
            target = new Vector3(hit.point.x, height, hit.point.z);
            transform.position = Vector3.SmoothDamp(transform.position,target,ref playerVelocity, smoothTime);
        }
        else
        {
            target = new Vector3(idealTransform.position.x, height, idealTransform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref playerVelocity, smoothTime);
        }
    }
}
