using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float sprintMultiplier = 3f;
    public float verticalSpeed = 6f;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    public bool holdRightMouseToLook = true;
    public bool lockCursor = true;

    float yaw;
    float pitch;

    void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // Toggle cursor lock on Escape (удобно для выхода в UI)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }

        bool canLook = !holdRightMouseToLook || Input.GetMouseButton(1);

        // Look
        if (canLook && Cursor.lockState == CursorLockMode.Locked)
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

            yaw += mx;
            pitch -= my;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        // Move
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        float up = 0f;
        if (Input.GetKey(KeyCode.E)) up += 1f;
        if (Input.GetKey(KeyCode.Q)) up -= 1f;

        Vector3 dir = (transform.right * h + transform.forward * v).normalized;
        Vector3 vertical = Vector3.up * up;

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        transform.position += dir * speed * Time.deltaTime;
        transform.position += vertical * verticalSpeed * Time.deltaTime;
    }
}
