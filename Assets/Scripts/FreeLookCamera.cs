
using UnityEngine;

public class FreeLookCamera : MonoBehaviour
{
    public float mouseSensitivity = 2f;

    private Transform playerBody;
    private float pitch = 0f;

    void Start()
    {
        playerBody = transform.parent;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
