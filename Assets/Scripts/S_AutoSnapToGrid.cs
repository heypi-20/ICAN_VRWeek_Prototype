using UnityEngine;
using System.Collections.Generic;

public class S_AutoSnapToGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [Range(0.08f,1.0f)]
    public float cellSize = 1f;  // 每个网格单元格的大小
    
    [Header("Physics Settings")]
    public string targetTag = "Snappable";  // 需要检测的物体 Tag
    public Vector3 overlapBoxSize = new Vector3(5, 5, 5);  // OverlapBox 检测范围
    public float stopThreshold = 0.01f;  // 物体移动停止的判定阈值

    [Header("Gizmos Settings")]
    public Color gridColor = Color.cyan;  // 自定义网格颜色

    private Dictionary<GameObject, Vector3> lastPositions = new Dictionary<GameObject, Vector3>();  // 记录物体上一帧的位置

    private void Update()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, overlapBoxSize / 2, Quaternion.identity);
        foreach (Collider col in colliders)
        {
            GameObject obj = col.gameObject;
            if (obj.CompareTag(targetTag) && IsObjectStopped(obj))
            {
                SnapToGrid(obj);
            }
        }
    }

    private bool IsObjectStopped(GameObject obj)
    {
        Vector3 currentPosition = obj.transform.position;

        if (!lastPositions.ContainsKey(obj))
        {
            lastPositions[obj] = currentPosition;
            return false;
        }

        float distanceMoved = Vector3.Distance(lastPositions[obj], currentPosition);
        lastPositions[obj] = currentPosition;

        return distanceMoved <= stopThreshold;
    }

    private void SnapToGrid(GameObject obj)
    {
        Vector3 currentPos = obj.transform.position;

        Vector3 snappedPos = new Vector3(
            Mathf.Round((currentPos.x - transform.position.x) / cellSize) * cellSize + transform.position.x,
            Mathf.Round((currentPos.y - transform.position.y) / cellSize) * cellSize + transform.position.y,
            Mathf.Round((currentPos.z - transform.position.z) / cellSize) * cellSize + transform.position.z
        );

        obj.transform.position = snappedPos;

        Vector3 currentRotation = obj.transform.eulerAngles;
        obj.transform.rotation = Quaternion.Euler(
            RoundToNearest90(currentRotation.x),
            RoundToNearest90(currentRotation.y),
            RoundToNearest90(currentRotation.z)
        );
    }

    private float RoundToNearest90(float angle)
    {
        angle = Mathf.Round(angle / 90) * 90;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;

        Vector3 boxStart = transform.position - overlapBoxSize / 2;

        // 绘制 X 和 Z 平面的网格线（覆盖 `overlapBoxSize` 区域）
        for (float x = boxStart.x; x <= boxStart.x + overlapBoxSize.x; x += cellSize)
        {
            for (float z = boxStart.z; z <= boxStart.z + overlapBoxSize.z; z += cellSize)
            {
                Vector3 startX = new Vector3(x, boxStart.y, boxStart.z);
                Vector3 endX = new Vector3(x, boxStart.y, boxStart.z + overlapBoxSize.z);
                Gizmos.DrawLine(startX, endX);

                Vector3 startZ = new Vector3(boxStart.x, boxStart.y, z);
                Vector3 endZ = new Vector3(boxStart.x + overlapBoxSize.x, boxStart.y, z);
                Gizmos.DrawLine(startZ, endZ);
            }
        }

        // 绘制检测区域
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, overlapBoxSize);
    }
}
