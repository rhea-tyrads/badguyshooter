using System.Collections.Generic;
using UnityEngine;

namespace TopDownEngine.Common.Scripts.BoogieScripts
{
    public class RayGun : MonoBehaviour
    {
        public MagicBeam prefab;
 
 
        public List<Transform> targets = new();

 
        readonly Dictionary<Transform, MagicBeam> _dictGun = new();
 

        void OnTriggerEnter(Collider other)
        {
            var t = other.transform;
            if (!other.CompareTag("Enemy")) return;
            if (targets.Contains(t)) return;
 

            Add(t);
        }


        void OnTriggerStay(Collider other)
        {
            var t = other.transform;
            if (!other.CompareTag("Enemy")) return;
            if (targets.Contains(t)) return;
 

            Add(t);
        }

        void OnTriggerExit(Collider other)
        {
            var t = other.transform;
            if (!other.CompareTag("Enemy")) return;
            if (!targets.Contains(t)) return;
            targets.Remove(t);
            _dictGun[t].Hide();
            _dictGun.Remove(t);
        }
 
        void Add(Transform t)
        {
         

            targets.Add(t);
 
        }

        void Update()
        {
            if (targets.Count == 0) return;
 
        }

        void FixedUpdate()
        {
            var radius = 6.5f;
            transform.localScale = new Vector3(radius, 1, radius);
 
        }
    }
}