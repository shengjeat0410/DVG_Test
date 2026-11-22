using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ShadowCombinePuzzle : MonoBehaviour
{
    [Header("1. 两个拼影子的方块")]
    public Transform objectA;
    public Transform objectB;

    [Header("2. 目标角度（拼出形状时的 Rotation）")]
    public Vector3 targetEulerA;
    public Vector3 targetEulerB;

    [Header("3. 允许误差角度（度）")]
    [Tooltip("两个物体当前角度与目标角度的差值都小于该值时视为对齐")]
    public float angleTolerance = 5f;

    [Header("4. 合成后出现的物体")]
    public GameObject mergedShape;

    [Header("5. 旋转设置与调试信息")]
    public float rotateSpeed = 90f;

    [Tooltip("是否隐藏原本的 ObjectA 和 ObjectB（合成后）")]
    public bool hideOriginalObjectsOnMerge = false;

    [Header("运行时观察（只读参考）")]
    [SerializeField] private float currentDiffA;
    [SerializeField] private float currentDiffB;
    [SerializeField] private bool isMerged = false;

    private void Awake()
    {
        if (mergedShape != null)
        {
            mergedShape.SetActive(false);
        }
    }

    private void Update()
    {
        if (isMerged) return;
        if (objectA == null || objectB == null) return;

        HandleRotationInput();
        CheckAlignment();
    }

    private void HandleRotationInput()
    {
        float delta = rotateSpeed * Time.deltaTime;

        bool rotAPos = false;
        bool rotANeg = false;
        bool rotBPos = false;
        bool rotBNeg = false;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            // ObjectA：Q / E
            if (kb.qKey.isPressed) rotAPos = true;
            if (kb.eKey.isPressed) rotANeg = true;

            // ObjectB：Z / C
            if (kb.zKey.isPressed) rotBPos = true;
            if (kb.cKey.isPressed) rotBNeg = true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        // 兼容旧版 Input Manager
        if (Input.GetKey(KeyCode.Q)) rotAPos = true;
        if (Input.GetKey(KeyCode.E)) rotANeg = true;
        if (Input.GetKey(KeyCode.Z)) rotBPos = true;
        if (Input.GetKey(KeyCode.C)) rotBNeg = true;
#endif

        // 绕世界坐标 Y 轴旋转；如需改为绕 Z 轴，把 Vector3.up 改成 Vector3.forward
        if (rotAPos) objectA.Rotate(Vector3.up * delta, Space.World);
        if (rotANeg) objectA.Rotate(-Vector3.up * delta, Space.World);

        if (rotBPos) objectB.Rotate(Vector3.up * delta, Space.World);
        if (rotBNeg) objectB.Rotate(-Vector3.up * delta, Space.World);
    }

    private void CheckAlignment()
    {
        Quaternion targetRotA = Quaternion.Euler(targetEulerA);
        Quaternion targetRotB = Quaternion.Euler(targetEulerB);

        currentDiffA = Quaternion.Angle(objectA.rotation, targetRotA);
        currentDiffB = Quaternion.Angle(objectB.rotation, targetRotB);

        if (currentDiffA <= angleTolerance && currentDiffB <= angleTolerance)
        {
            CompleteMerge(targetRotA, targetRotB);
        }
    }

    private void CompleteMerge(Quaternion targetRotA, Quaternion targetRotB)
    {
        isMerged = true;

        objectA.rotation = targetRotA;
        objectB.rotation = targetRotB;

        if (hideOriginalObjectsOnMerge)
        {
            objectA.gameObject.SetActive(false);
            objectB.gameObject.SetActive(false);
        }

        if (mergedShape != null)
        {
            mergedShape.SetActive(true);
        }

        Debug.Log("[ShadowCombinePuzzle] 角度对齐成功，合成物体已激活！");
    }

    // 如需外部重置关卡可调用此方法
    public void ResetPuzzle(Quaternion startRotA, Quaternion startRotB)
    {
        isMerged = false;
        currentDiffA = 0f;
        currentDiffB = 0f;

        if (objectA != null)
        {
            objectA.gameObject.SetActive(true);
            objectA.rotation = startRotA;
        }

        if (objectB != null)
        {
            objectB.gameObject.SetActive(true);
            objectB.rotation = startRotB;
        }

        if (mergedShape != null)
        {
            mergedShape.SetActive(false);
        }
    }
}