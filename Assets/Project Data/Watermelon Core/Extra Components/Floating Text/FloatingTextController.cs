using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FloatingTextController : MonoBehaviour
    {
        static FloatingTextController floatingTextController;

        [SerializeField] FloatingTextCase[] floatingTextCases;
        Dictionary<int, FloatingTextCase> floatingTextLink;

        public void Inititalise()
        {
            floatingTextController = this;

            floatingTextLink = new Dictionary<int, FloatingTextCase>();
            for (var i = 0; i < floatingTextCases.Length; i++)
            {
                floatingTextCases[i].Initialise();

                floatingTextLink.Add(floatingTextCases[i].Name.GetHashCode(), floatingTextCases[i]);
            }
        }

        public static FloatingTextBaseBehaviour SpawnFloatingText(string floatingTextName, string text, Vector3 position, Quaternion rotation, float scale)
        {
            return SpawnFloatingText(floatingTextName.GetHashCode(), text, position, rotation, scale);
        }

        public static FloatingTextBaseBehaviour SpawnFloatingText(int floatingTextNameHash, string text, Vector3 position, Quaternion rotation, float scale)
        {
            if (floatingTextController.floatingTextLink.ContainsKey(floatingTextNameHash))
            {
                var floatingTextCase = floatingTextController.floatingTextLink[floatingTextNameHash];

                var floatingTextObject = floatingTextCase.FloatingTextPool.GetPooledObject();
                floatingTextObject.transform.position = position;
                floatingTextObject.transform.rotation = rotation;
                floatingTextObject.SetActive(true);

                var floatingTextBehaviour = floatingTextObject.GetComponent<FloatingTextBaseBehaviour>();
                floatingTextBehaviour.Activate(text, scale);

                return floatingTextBehaviour;
            }

            return null;
        }

        public static void Unload()
        {
            var floatingTextCases = floatingTextController.floatingTextCases;
            for(var i = 0; i < floatingTextCases.Length; i++)
            {
                floatingTextCases[i].FloatingTextPool.ReturnToPoolEverything(true);
            }
        }

        public static int GetHash(string textStyleName)
        {
            return textStyleName.GetHashCode();
        }
    }
}