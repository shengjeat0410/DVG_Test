using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FreeLookCamera : MonoBehaviour
{
    [Header("移动与视角参数")]
    public float moveSpeed = 4f;
    public float lookSpeed = 120f;
    [Tooltip("是否需要按住鼠标右键才能转视角")]
    public bool requireRightMouseForLook = false;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    private void Update()
    {
        float moveX = 0f;
        float moveZ = 0f;
        float lookX = 0f;
        float lookY = 0f;
        bool rightMouseHeld = false;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb != null)
        {
            if (kb.aKey.isPressed) moveX -= 1f;
            if (kb.dKey.isPressed) moveX += 1f;
            if (kb.wKey.isPressed) moveZ += 1f;
            if (kb.sKey.isPressed) moveZ -= 1f;

            if (kb.leftArrowKey.isPressed) lookX -= 1f;
            if (kb.rightArrowKey.isPressed) lookX += 1f;
            if (kb.upArrowKey.isPressed) lookY += 1f;
            if (kb.downArrowKey.isPressed) lookY -= 1f;
        }

        if (mouse != null)
        {
            rightMouseHeld = mouse.rightButton.isPressed;
            if (!requireRightMouseForLook || rightMouseHeld)
            {
                Vector2 delta = mouse.delta.ReadValue();
                lookX += delta.x * 0.15f;
                lookY += delta.y * 0.15f;
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.A)) moveX -= 1f;
        if (Input.GetKey(KeyCode.D)) moveX += 1f;
        if (Input.GetKey(KeyCode.W)) moveZ += 1f;
        if (Input.GetKey(KeyCode.S)) moveZ -= 1f;

        if (Input.GetKey(KeyCode.LeftArrow)) lookX -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) lookX += 1f;
        if (Input.GetKey(KeyCode.UpArrow)) lookY += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) lookY -= 1f;

        rightMouseHeld = Input.GetMouseButton(1);
        if (!requireRightMouseForLook || rightMouseHeld)
        {
            lookX += Input.GetAxis("Mouse X") * 2f;
            lookY += Input.GetAxis("Mouse Y") * 2f;
        }
#endif

        Vector3 forwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 rightFlat = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 moveDir = (rightFlat * moveX + forwardFlat * moveZ).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        if (!requireRightMouseForLook || rightMouseHeld)
        {
            yaw += lookX * lookSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch - lookY * lookSpeed * Time.deltaTime, -80f, 80f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}