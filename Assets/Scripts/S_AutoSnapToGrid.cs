using UnityEngine;
using System.Collections.Generic;

public class S_AutoSnapToGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [Range(0.08f, 1.0f)]
    public float cellSize = 1f;  // 每个网格单元格的大小

    [Header("Physics Settings")]
    public string targetTag = "Snappable";  // 需要检测的物体 Tag
    public Vector3 overlapBoxSize = new Vector3(5, 5, 5);  // OverlapBox 检测范围
    public float stopThreshold = 0.1f;  // 物体移动停止的判定阈值（速度小于该值时认为已停止）

    [Header("Gizmos Settings")]
    public Color gridColor = Color.cyan;  // 自定义网格颜色

    private void Update()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, overlapBoxSize / 2, Quaternion.identity);
        foreach (Collider col in colliders)
        {
            GameObject obj = col.gameObject;

            // 检查物体是否具有目标 Tag 且已停止
            if (obj.CompareTag(targetTag) && IsObjectStopped(obj))
            {
                SnapToGrid(obj);
            }
        }
    }

    // 检测物体是否停止移动（基于 Rigidbody 速度）
    private bool IsObjectStopped(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null) return false;  // 如果没有 Rigidbody，跳过检测

        // 当 Rigidbody 的速度小于阈值时，认为物体已停止
        return rb.linearVelocity.magnitude <= stopThreshold;
    }

    // 执行吸附到最近的网格点
    private void SnapToGrid(GameObject obj)
    {
        Vector3 currentPos = obj.transform.position;

        // 计算最近网格中心点
        Vector3 snappedPos = new Vector3(
            Mathf.Round((currentPos.x - transform.position.x) / cellSize) * cellSize + transform.position.x,
            Mathf.Round((currentPos.y - transform.position.y) / cellSize) * cellSize + transform.position.y,
            Mathf.Round((currentPos.z - transform.position.z) / cellSize) * cellSize + transform.position.z
        );

        obj.transform.position = snappedPos;

        // 调整物体的旋转角度
        Vector3 currentRotation = obj.transform.eulerAngles;
        obj.transform.rotation = Quaternion.Euler(
            RoundToNearest90(currentRotation.x),
            RoundToNearest90(currentRotation.y),
            RoundToNearest90(currentRotation.z)
        );
    }

    // 将角度四舍五入到最接近的 -90°, 0°, 90°, 或 180°
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
        Vector3 boxEnd = boxStart + overlapBoxSize;

        // 绘制底部水平面
        float y = boxStart.y;  // 只绘制底部
        for (float x = boxStart.x; x <= boxEnd.x; x += cellSize)
        {
            Vector3 startX = new Vector3(x, y, boxStart.z);
            Vector3 endX = new Vector3(x, y, boxEnd.z);
            Gizmos.DrawLine(startX, endX);
        }
        for (float z = boxStart.z; z <= boxEnd.z; z += cellSize)
        {
            Vector3 startZ = new Vector3(boxStart.x, y, z);
            Vector3 endZ = new Vector3(boxEnd.x, y, z);
            Gizmos.DrawLine(startZ, endZ);
        }

        // 绘制负 Z 墙面
        float zWall = boxStart.z;  // 负 Z 方向的墙面
        for (float x = boxStart.x; x <= boxEnd.x; x += cellSize)
        {
            Vector3 startX = new Vector3(x, boxStart.y, zWall);
            Vector3 endX = new Vector3(x, boxEnd.y, zWall);
            Gizmos.DrawLine(startX, endX);
        }
        for (float yWall = boxStart.y; yWall <= boxEnd.y; yWall += cellSize)
        {
            Vector3 startY = new Vector3(boxStart.x, yWall, zWall);
            Vector3 endY = new Vector3(boxEnd.x, yWall, zWall);
            Gizmos.DrawLine(startY, endY);
        }

        // 绘制检测区域
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, overlapBoxSize);
    }
}
