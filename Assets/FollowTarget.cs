using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
      Transform follow;
   
    void Start()
    {
        follow = transform.parent;
        transform.SetParent(null);
    }

 
    void Update()
    {
        transform.position = follow.position;
    }
}
