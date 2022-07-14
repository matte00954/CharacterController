using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;

    private float mouseSensitivity = 100f;

    private float xRotation = 0f;
    private float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -45f, 45f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.transform.Rotate(Vector3.up * mouseX);
    }
}
