using System;
using UnityEngine;

namespace MobileTools.Utilities
{
    public class ScreenUI : MonoBehaviour
    {
        public CanvasGroup canvasGroup;

        public event Action OnHide = delegate { };
        public event Action OnShow = delegate { };
        

        protected virtual void Showing() { }
        protected virtual void Hiding() { }

        public void Show()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            OnShow();
            Showing();
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            OnHide();
            Hiding();
        }
    }
}