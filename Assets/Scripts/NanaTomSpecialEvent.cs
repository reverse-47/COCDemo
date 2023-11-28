using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NanaTomSpecialEvent : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject barrier; // 门对象

    private void OnTriggerEnter(Collider other)
    {
        print("barrier guared");
        if (other.gameObject == barrier && barrier.tag == "close")
        {
            barrier.tag = "guard";
        }
    }

}
