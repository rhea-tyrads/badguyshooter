using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Canvas))]
    public class OverlayUI : MonoBehaviour
    {
        [SerializeField] CurrencyUIPanelSimple coinsUI;

        static Canvas _canvas;

        public void Initialise()
        {
            _canvas = GetComponent<Canvas>();
        }

        public static void ShowOverlay()
        {
            _canvas.enabled = true;
        }

        public static void HideOverlay()
        {
            _canvas.enabled = false;
        }
    }
}