using UnityEngine;

namespace MobileTools.Utilities
{
    public class ResetPositionUI : MonoBehaviour
    {
        void Start()
        {
            var rect = (RectTransform) transform;
            rect.anchoredPosition = Vector2.zero;
        }
    }
}