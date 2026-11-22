using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FlashlightShadowShapePuzzle : MonoBehaviour
{
    [Header("1. 手电筒与墙面引用")]
    [Tooltip("用于投射影子的手电筒 Transform（建议独立于视角或按需挂载）")]
    public Transform flashlightTransform;
    [Tooltip("接收影子的墙面 Collider（如 Cube / Plane）")]
    public Collider shadowWallCollider;

    [Header("2. 两个固定的方块（或采样点）")]
    public Transform blockA;
    public Transform blockB;
    [Tooltip("可选：手动添加采样点；留空则自动使用 blockA 和 blockB")]
    public List<Transform> samplePoints = new List<Transform>();

    [Header("3. 手电筒旋转限制（度）")]
    public float minYaw = -45f;
    public float maxYaw = 45f;
    public float minPitch = -45f;
    public float maxPitch = 45f;
    public float rotateSpeed = 60f;

    [Header("4. 目标影子形状（墙面局部坐标 2D 点）")]
    public List<Vector2> targetWallPoints = new List<Vector2>();
    [Tooltip("墙面投影点与目标点之间的允许平均误差（米）")]
    public float positionTolerance = 0.25f;

    [Header("5. 合成后出现的物体")]
    public GameObject mergedShape;
    public bool hideBlocksOnMerge = false;

    [Header("6. 运行时调试信息")]
    [SerializeField] private float currentYaw;
    [SerializeField] private float currentPitch;
    [SerializeField] private float currentShapeError;
    [SerializeField] private bool isMerged = false;

    private Quaternion baseRotation;
    private readonly List<Vector2> currentWallPoints = new List<Vector2>();

    private void Awake()
    {
        if (mergedShape != null)
            mergedShape.SetActive(false);

        if (flashlightTransform != null)
            baseRotation = flashlightTransform.localRotation;

        if (samplePoints.Count == 0)
        {
            if (blockA != null) samplePoints.Add(blockA);
            if (blockB != null) samplePoints.Add(blockB);
        }
    }

    private void Update()
    {
        if (isMerged) return;

        HandleFlashlightRotation();
        EvaluateWallShadowShape();
    }

    private void HandleFlashlightRotation()
    {
        if (flashlightTransform == null) return;

        float yawInput = 0f;
        float pitchInput = 0f;

#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            // Q / E 控制左右偏转（-45° ~ 45°）
            if (kb.qKey.isPressed) yawInput -= 1f;
            if (kb.eKey.isPressed) yawInput += 1f;

            // Z / C 控制上下俯仰（-45° ~ 45°）
            if (kb.zKey.isPressed) pitchInput -= 1f;
            if (kb.cKey.isPressed) pitchInput += 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.Q)) yawInput -= 1f;
        if (Input.GetKey(KeyCode.E)) yawInput += 1f;
        if (Input.GetKey(KeyCode.Z)) pitchInput -= 1f;
        if (Input.GetKey(KeyCode.C)) pitchInput += 1f;
#endif

        currentYaw = Mathf.Clamp(currentYaw + yawInput * rotateSpeed * Time.deltaTime, minYaw, maxYaw);
        currentPitch = Mathf.Clamp(currentPitch + pitchInput * rotateSpeed * Time.deltaTime, minPitch, maxPitch);

        Quaternion offsetRot = Quaternion.Euler(currentPitch, currentYaw, 0f);
        flashlightTransform.localRotation = baseRotation * offsetRot;
    }

    private void EvaluateWallShadowShape()
    {
        currentWallPoints.Clear();

        if (flashlightTransform == null || shadowWallCollider == null || samplePoints.Count == 0)
            return;

        Vector3 lightOrigin = flashlightTransform.position;

        for (int i = 0; i < samplePoints.Count; i++)
        {
            Transform pt = samplePoints[i];
            if (pt == null) continue;

            Vector3 dir = (pt.position - lightOrigin).normalized;
            Ray ray = new Ray(pt.position, dir);

            if (shadowWallCollider.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 localHit = shadowWallCollider.transform.InverseTransformPoint(hit.point);
                currentWallPoints.Add(new Vector2(localHit.x, localHit.y));
            }
            else
            {
                currentShapeError = 999f;
                return;
            }
        }

        if (targetWallPoints.Count != currentWallPoints.Count || currentWallPoints.Count == 0)
            return;

        float totalDist = 0f;
        for (int i = 0; i < currentWallPoints.Count; i++)
        {
            totalDist += Vector2.Distance(currentWallPoints[i], targetWallPoints[i]);
        }

        currentShapeError = totalDist / currentWallPoints.Count;

        if (currentShapeError <= positionTolerance)
        {
            CompleteShapeMatch();
        }
    }

    [ContextMenu("Capture Current Shadow As Target")]
    public void CaptureCurrentShadowAsTarget()
    {
        if (flashlightTransform == null || shadowWallCollider == null) return;

        if (samplePoints.Count == 0)
        {
            if (blockA != null) samplePoints.Add(blockA);
            if (blockB != null) samplePoints.Add(blockB);
        }

        targetWallPoints.Clear();
        Vector3 lightOrigin = flashlightTransform.position;

        foreach (Transform pt in samplePoints)
        {
            if (pt == null) continue;
            Vector3 dir = (pt.position - lightOrigin).normalized;
            Ray ray = new Ray(pt.position, dir);

            if (shadowWallCollider.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 localHit = shadowWallCollider.transform.InverseTransformPoint(hit.point);
                targetWallPoints.Add(new Vector2(localHit.x, localHit.y));
            }
        }

        Debug.Log($"[FlashlightShadowShapePuzzle] 已记录 {targetWallPoints.Count} 个墙面目标投影点。");
    }

    private void CompleteShapeMatch()
    {
        isMerged = true;

        if (hideBlocksOnMerge)
        {
            if (blockA != null) blockA.gameObject.SetActive(false);
            if (blockB != null) blockB.gameObject.SetActive(false);
        }

        if (mergedShape != null)
            mergedShape.SetActive(true);

        Debug.Log("[FlashlightShadowShapePuzzle] 墙面影子形状对齐，合成物体已出现！");
    }
}