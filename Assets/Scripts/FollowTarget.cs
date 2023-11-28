using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class FollowTarget : MonoBehaviour
{
    public NavMeshAgent nav;

    public void MoveToObject(string Rolename, string Objname)
    {
        GameObject parentObject = GameObject.Find("GameObjects");
        if (parentObject != null)
        {
            GameObject role = GameObject.Find(Rolename);
            if (role != null)
            {
                nav = role.GetComponent<NavMeshAgent>();
                // 首先查找Bar目录下的一个物体
                //GameObject child = GameObject.Find(Objname);
                // 使用物体的 Transform 来查找子物体
                Transform child = parentObject.transform.Find(Objname);
                if (child != null)
                {
                    GameObject childObject = child.gameObject;
                    Debug.Log("Found child object: " + childObject.name);
                    if (nav != null)
                    {
                        nav.SetDestination(childObject.transform.position);

                    }
                    else
                    {
                        Debug.Log("NavMeshAgent component not found on the role GameObject.");
                    }
                }
                else
                {
                    Debug.Log(Objname + " not found.");
                }
            }
            else
            {
                print(Rolename+"role not found");
            }
        }
        else
        {
            Debug.Log("Bar not found.");
        }
        
        
    }
}
