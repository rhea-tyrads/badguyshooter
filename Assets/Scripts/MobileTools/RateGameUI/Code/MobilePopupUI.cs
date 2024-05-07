using MobileTools.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MobileTools.RateGameUI.Code
{
    public class MobilePopupUI : MobileScreenUI
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