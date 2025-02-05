using UnityEngine;

/// <summary>
/// 当检测到与指定Tag的物体发生碰撞时，
/// 根据接触点把自己“吸附”到对方的位置并可选择设为子物体。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SnapOnCollision : MonoBehaviour
{
    [Tooltip("需要粘连的对方物体Tag")]
    public string targetTag = "SnapObject";
    
    [Tooltip("是否只能粘连一次（true时粘住后就不再执行后续逻辑）")]
    public bool snapOnce = true;
    
    // 记录是否已粘连过（仅在 snapOnce = true 时生效）
    private bool hasSnapped = false;

    // 物体大约一半的“厚度”或需要向内/外偏移的距离
    // 如果想让物体中心贴到接触点，可以把它设为0
    public float snapOffset = 0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        // 如果设置了snapOnce并且已经执行过粘连，就不再重复
        if (snapOnce && hasSnapped) return;
        Debug.Log("Touched");

        // 判断对方是否是指定Tag
        if (collision.gameObject.CompareTag(targetTag))
        {
            // 获取第一组接触点（若有多个接触点，根据需求取平均或取其中一个）
            ContactPoint contact = collision.contacts[0];
            
            // 计算一个简单的吸附位置：
            // 以碰撞点为基准，沿接触法线 normal 做一定偏移，以免两个物体重叠穿透
            Vector3 snapPosition = contact.point - contact.normal * snapOffset;
            
            // 将当前物体的位置设置到“吸附”坐标
            transform.position = snapPosition;
            
            // 如果想让物体的某个朝向与法线对齐，可以做一个简单旋转，如下所示
            // 这里让物体的“前方”(-Z)贴到对方上（仅示例，视自己模型定向而定）
            transform.forward = -contact.normal;

            // 可选：把自己设为对方物体的子物体，让它们在层级关系上绑定
            transform.SetParent(collision.transform);

            // 可选：让自己Rigidbody变为Kinematic，不再被物理干扰
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // 标记已粘连
            hasSnapped = true;
        }
    }
}