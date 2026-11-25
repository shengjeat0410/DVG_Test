
using UnityEngine;
using UnityEngine.Rendering;

public class FlashlightToggle : MonoBehaviour
{
    [Header("手电筒装备设置")]
    public GameObject torchModel;
    public Transform torchEquipPoint;
    public Camera playerCamera;
    public float pickupRange = 3f;

    [Header("装备后的模型角度")]
    public Vector3 equippedRotation = Vector3.zero;

    [Header("拾取状态")]
    [SerializeField] private bool hasFlashlight = false;

    private Light flashlight;

    void Start()
    {
        flashlight = GetComponent<Light>();
        flashlight.enabled = false;
        hasFlashlight = false;
    }

    void Update()
    {
        if (!hasFlashlight)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUpFlashlight();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }

    void PickUpFlashlight()
    {
        if (torchModel == null ||
            torchEquipPoint == null ||
            playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.transform == torchModel.transform ||
                hit.transform.IsChildOf(torchModel.transform))
            {
                EquipFlashlight();
            }
        }
    }

    void EquipFlashlight()
    {
        hasFlashlight = true;

        foreach (Collider col in
            torchModel.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        torchModel.transform.SetParent(torchEquipPoint);
        torchModel.transform.localPosition = Vector3.zero;
        torchModel.transform.localRotation =
            Quaternion.Euler(equippedRotation);

        foreach (Renderer rend in
            torchModel.GetComponentsInChildren<Renderer>())
        {
            rend.shadowCastingMode = ShadowCastingMode.Off;
        }

        Debug.Log("Flashlight equipped!");
    }
}
