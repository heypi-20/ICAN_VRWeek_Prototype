using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDragMove : MonoBehaviour
{
    public Camera mainCamera;
    private bool isDragging = false;
    private Vector3 offset;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                isDragging = true;

                // 计算点击时的偏移量
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), mainCamera.nearClipPlane));
                worldPosition.z = transform.position.z;  // 固定深度
                offset = transform.position - worldPosition;
            }
        }

        if (isDragging && Mouse.current.leftButton.isPressed)
        {
            // 获取鼠标的世界坐标，仅移动 XY 平面
            Vector3 mousePosition = new Vector3(Mouse.current.position.x.ReadValue(), Mouse.current.position.y.ReadValue(), mainCamera.nearClipPlane);
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = transform.position.z;  // 固定深度
            transform.position = worldPosition + offset;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }
}