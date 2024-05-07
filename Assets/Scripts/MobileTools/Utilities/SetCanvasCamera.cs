using UnityEngine;

namespace MobileTools.Utilities
{
    [ExecuteInEditMode]
    public class SetCanvasCamera : MonoBehaviour
    {
        public Canvas canvas;

        void Start()
        {
            SetCamera();
        }


        void SetCamera()
        {
            if (!canvas) canvas = GetComponent<Canvas>();
            if (!canvas) return;
            if (canvas.worldCamera) return;

            canvas.worldCamera = Camera.main;
        }
    }
}