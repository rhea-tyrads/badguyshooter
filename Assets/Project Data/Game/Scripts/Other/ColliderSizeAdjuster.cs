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
        var scaleCoef = 0.15625f;

        var colliders = gameObject.GetComponentsInChildren<Collider>();

        for (var i = 0; i < colliders.Length    ; i++)
        {
            var box = colliders[i] as BoxCollider;
            if(box != null)
            {
                box.size *= scaleCoef;
                box.center *= scaleCoef;
            }

            var capsuleCollider = colliders[i] as CapsuleCollider;
            if (capsuleCollider != null)
            {
                capsuleCollider.height *= scaleCoef;
                capsuleCollider.radius *= scaleCoef;
                capsuleCollider.center *= scaleCoef;
            }

            var sphere = colliders[i] as SphereCollider;
            if (sphere != null)
            {
                sphere.radius *= scaleCoef;
                sphere.center *= scaleCoef;
            }
        }

        var obst = GetComponent<NavMeshObstacle>();

        if(obst != null)
        {
            obst.center *= scaleCoef;
            obst.size *= scaleCoef;
        }

    }
}
