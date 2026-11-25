
using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    public Transform holdPoint;
    public float pickupRange = 4f;
    public float rotationSpeed = 90f;

    [Header("物体前后移动")]
    public float scrollSpeed = 0.5f;
    public float minDistance = 1f;
    public float maxDistance = 4f;

    private Rigidbody heldObject;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                PickUp();
            }
            else
            {
                Drop();
            }
        }

        if (heldObject != null)
        {
            RotateObject();
            AdjustDistance();
        }
    }

    void LateUpdate()
    {
        if (heldObject != null)
        {
            heldObject.transform.position = holdPoint.position;
        }
    }

    void PickUp()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.collider.CompareTag("Pickupable"))
            {
                Rigidbody rb = hit.rigidbody;

                if (rb != null)
                {
                    heldObject = rb;
                    heldObject.useGravity = false;
                    heldObject.isKinematic = true;
                }
            }
        }
    }

    void Drop()
    {
        heldObject = null;
    }

    void RotateObject()
    {
        float speed = rotationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            heldObject.transform.Rotate(
                Vector3.up, -speed, Space.World);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            heldObject.transform.Rotate(
                Vector3.up, speed, Space.World);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            heldObject.transform.Rotate(
                transform.right, -speed, Space.World);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            heldObject.transform.Rotate(
                transform.right, speed, Space.World);
        }
    }

    void AdjustDistance()
    {
        float scroll = Input.mouseScrollDelta.y;

        Vector3 position = holdPoint.localPosition;

        position.z += scroll * scrollSpeed;

        position.z = Mathf.Clamp(
            position.z, minDistance, maxDistance);

        holdPoint.localPosition = position;
    }
}
