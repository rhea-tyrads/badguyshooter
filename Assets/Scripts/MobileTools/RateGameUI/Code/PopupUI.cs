using MobileTools.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MobileTools.RateGameUI.Code
{
    public class PopupUI : ScreenUI
    {
        [SerializeField] Button closeButton;

        void OnEnable()
        {
            if (closeButton) closeButton.onClick.AddListener(Hide);
        }

        void OnDisable()
        {
            if (closeButton) closeButton.onClick.RemoveListener(Hide);
        }
    }
}