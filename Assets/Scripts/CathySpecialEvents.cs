using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CathySpecialEvents : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject barrier; // 门对象

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == barrier && (barrier.tag == "close" || barrier.tag=="guard"))
        {
            OpenBarrier();
        }
    }

    private void OpenBarrier()
    {
        print("open the door");
        var navObstacle = barrier.GetComponent<UnityEngine.AI.NavMeshObstacle>();
        navObstacle.enabled = false;
    
        //将挡板旋转为竖直方向
        barrier.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        float halfWidth = barrier.transform.localScale.x * 0.5f; // 计算物体宽度的一半
        float halfHeight = barrier.transform.localScale.x * 0.5f; // 计算物体宽度的一半

        Vector3 newPosition = barrier.transform.position + new Vector3(halfWidth, -halfHeight, 0f); // 计算新位置

        barrier.transform.position = newPosition; // 将物体移动到新位置
        barrier.tag = "open";
    }

    // private void CloseBarrier()
    // {
    //     print("close barrier");
    // }
}
