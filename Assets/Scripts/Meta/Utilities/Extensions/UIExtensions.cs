using UnityEngine;

namespace Utilities.Extensions
{
    public static class UIExtensions
    {
        public static void EnableGroup(CanvasGroup group)
        {
            group.alpha = 1;
            group.interactable = true;
            group.blocksRaycasts = true;
        }

        public static void DisableGroup(CanvasGroup group)
        {
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}