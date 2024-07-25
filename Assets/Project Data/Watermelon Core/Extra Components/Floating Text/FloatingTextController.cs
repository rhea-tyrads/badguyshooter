using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FloatingTextController : MonoBehaviour
    {
        static FloatingTextController _floatingTextController;

        [SerializeField] FloatingTextCase[] floatingTextCases;
        Dictionary<int, FloatingTextCase> _floatingTextLink;

        public void Inititalise()
        {
            _floatingTextController = this;
            _floatingTextLink = new Dictionary<int, FloatingTextCase>();
            foreach (var txt in floatingTextCases)
            {
                txt.Initialise();
                _floatingTextLink.Add(txt.Name.GetHashCode(), txt);
            }
        }

        public static FloatingTextBaseBehaviour Spawn(string floatingTextName, string text, Vector3 position,
            Quaternion rotation, float scale) => Spawn(floatingTextName.GetHashCode(), text, position, rotation, scale);

        public static FloatingTextBaseBehaviour Spawn(int floatingTextNameHash, string text, Vector3 position,
            Quaternion rotation, float scale)
        {
            if (!_floatingTextController._floatingTextLink.ContainsKey(floatingTextNameHash)) return null;
            
            var floatingTextCase = _floatingTextController._floatingTextLink[floatingTextNameHash];
            var floatingTextObject = floatingTextCase.FloatingTextPool.Get();
            floatingTextObject.transform.position = position;
            floatingTextObject.transform.rotation = rotation;
            floatingTextObject.SetActive(true);
            var floatingTextBehaviour = floatingTextObject.GetComponent<FloatingTextBaseBehaviour>();
            floatingTextBehaviour.Activate(text, scale);
            return floatingTextBehaviour;

        }

        public static void Unload()
        {
            var floatingTextCases = _floatingTextController.floatingTextCases;
            foreach (var txt in floatingTextCases)
                txt.FloatingTextPool.ReturnToPoolEverything(true);
        }

        public static int GetHash(string textStyleName) 
            => textStyleName.GetHashCode();
    }
}