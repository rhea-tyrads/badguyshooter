using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Watermelon;

public class ColliderSizeAdjuster : MonoBehaviour
{
    [Button]
    public void Run()
    {
        float scaleCoef = 0.15625f;

        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length    ; i++)
        {
            BoxCollider box = colliders[i] as BoxCollider;
            if(box != null)
            {
                box.size *= scaleCoef;
                box.center *= scaleCoef;
            }

            CapsuleCollider capsuleCollider = colliders[i] as CapsuleCollider;
            if (capsuleCollider != null)
            {
                capsuleCollider.height *= scaleCoef;
                capsuleCollider.radius *= scaleCoef;
                capsuleCollider.center *= scaleCoef;
            }

            SphereCollider sphere = colliders[i] as SphereCollider;
            if (sphere != null)
            {
                sphere.radius *= scaleCoef;
                sphere.center *= scaleCoef;
            }
        }

        NavMeshObstacle obst = GetComponent<NavMeshObstacle>();

        if(obst != null)
        {
            obst.center *= scaleCoef;
            obst.size *= scaleCoef;
        }

    }
}
