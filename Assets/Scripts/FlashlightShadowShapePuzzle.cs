
using System.Collections.Generic;
using UnityEngine;

public class FlashlightShadowShapePuzzle : MonoBehaviour
{
    [Header("1. 手电筒与墙面引用")]
    public Transform flashlightTransform;
    public Light flashlightLight;
    public Collider shadowWallCollider;

    [Header("2. 两个物体与采样点")]
    public Transform blockA;
    public Transform blockB;
    public List<Transform> samplePoints = new List<Transform>();

    [Header("3. 目标影子形状")]
    public List<Vector2> targetWallPoints = new List<Vector2>();
    public float positionTolerance = 0.25f;

    [Header("4. 合成后出现的物体")]
    public GameObject mergedShape;

    [Header("5. 合成按键")]
    public KeyCode mergeKey = KeyCode.F1;

    [Header("6. 运行时调试信息")]
    [SerializeField] private bool flashlightIsOn;
    [SerializeField] private bool blockAIsLit;
    [SerializeField] private bool blockBIsLit;
    [SerializeField] private float currentShapeError;
    [SerializeField] private bool canMerge;
    [SerializeField] private bool isMerged;

    private readonly List<Vector2> currentWallPoints =
        new List<Vector2>();

    private void Awake()
    {
        if (mergedShape != null)
            mergedShape.SetActive(false);

        if (flashlightLight == null && flashlightTransform != null)
            flashlightLight = flashlightTransform.GetComponent<Light>();

        SetupSamplePoints();
    }

    private void SetupSamplePoints()
    {
        if (samplePoints.Count == 0)
        {
            if (blockA != null) samplePoints.Add(blockA);
            if (blockB != null) samplePoints.Add(blockB);
        }
    }

    private void LateUpdate()
    {
        if (isMerged) return;

        canMerge = false;
        currentShapeError = 999f;

        flashlightIsOn = flashlightLight != null
                      && flashlightLight.isActiveAndEnabled
                      && flashlightLight.intensity > 0f;

        blockAIsLit = false;
        blockBIsLit = false;

        if (flashlightIsOn)
        {
            blockAIsLit = IsObjectLit(blockA);
            blockBIsLit = IsObjectLit(blockB);

            if (blockAIsLit && blockBIsLit)
            {
                EvaluateWallShadowShape();
            }
        }

        if (Input.GetKeyDown(mergeKey))
        {
            if (canMerge)
            {
                CompleteShapeMatch();
            }
            else
            {
                Debug.Log("Cannot merge: shadow conditions not met.");
            }
        }
    }

    private bool IsObjectLit(Transform target)
    {
        if (target == null || flashlightTransform == null)
            return false;

        Vector3 origin = flashlightTransform.position;
        Vector3 direction = target.position - origin;
        float distance = direction.magnitude;

        // 检查灯光距离
        if (distance > flashlightLight.range || distance < 0.001f)
            return false;

        // 检查手电筒照射角度
        float angle = Vector3.Angle(
            flashlightTransform.forward,
            direction
        );

        if (angle > flashlightLight.spotAngle * 0.5f)
            return false;

        // 检查中间是否有其他物体遮挡
        if (Physics.Raycast(
            origin,
            direction.normalized,
            out RaycastHit hit,
            distance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == target ||
                hit.transform.IsChildOf(target))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    private void EvaluateWallShadowShape()
    {
        if (!CalculateWallPoints())
            return;

        if (targetWallPoints.Count != currentWallPoints.Count)
            return;

        if (currentWallPoints.Count == 0)
            return;

        float totalDist = 0f;

        for (int i = 0; i < currentWallPoints.Count; i++)
        {
            totalDist += Vector2.Distance(
                currentWallPoints[i],
                targetWallPoints[i]
            );
        }

        currentShapeError = totalDist / currentWallPoints.Count;

        canMerge = currentShapeError <= positionTolerance;
    }

    private bool CalculateWallPoints()
    {
        currentWallPoints.Clear();

        if (flashlightTransform == null ||
            shadowWallCollider == null ||
            samplePoints.Count == 0)
        {
            return false;
        }

        Vector3 lightOrigin = flashlightTransform.position;

        foreach (Transform pt in samplePoints)
        {
            if (pt == null)
                return false;

            Vector3 dir = (pt.position - lightOrigin).normalized;
            Ray ray = new Ray(pt.position, dir);

            if (shadowWallCollider.Raycast(
                ray,
                out RaycastHit hit,
                100f))
            {
                Vector3 localHit =
                    shadowWallCollider.transform
                    .InverseTransformPoint(hit.point);

                currentWallPoints.Add(
                    new Vector2(localHit.x, localHit.y)
                );
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    [ContextMenu("Capture Current Shadow As Target")]
    public void CaptureCurrentShadowAsTarget()
    {
        SetupSamplePoints();

        if (!CalculateWallPoints())
        {
            Debug.LogWarning("Failed to capture shadow target.");
            return;
        }

        targetWallPoints.Clear();
        targetWallPoints.AddRange(currentWallPoints);

        Debug.Log(
            $"Captured {targetWallPoints.Count} shadow target points."
        );
    }

    private void CompleteShapeMatch()
    {
        isMerged = true;
        canMerge = false;

        if (mergedShape != null)
            mergedShape.SetActive(true);

        Debug.Log("Shadow merged successfully!");
    }
}
