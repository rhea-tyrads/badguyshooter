using UnityEngine;
using System.Collections.Generic;

namespace Watermelon
{
    [System.Serializable]
    public class NotchSaveArea
    {
        static NotchSaveArea notchSaveArea;

        [SerializeField] RectTransform[] safePanels;

        [Space]
        [SerializeField] bool conformX = true;
        [SerializeField] bool conformY = true;

        static List<RectTransform> registeredTransforms = new();

        static Rect lastSafeArea = new(0, 0, 0, 0);
        static Vector2Int lastScreenSize = new(0, 0);

        static ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

        public void Initialise()
        {
            notchSaveArea = this;

            registeredTransforms.AddRange(safePanels);

            Refresh();
        }

        public static void RegisterRectTransform(RectTransform rectTransform)
        {
            registeredTransforms.Add(rectTransform);

            if(notchSaveArea != null)
                Refresh(true);
        }

        public static void Refresh(bool forceRefresh = false)
        {
            var safeArea = Screen.safeArea;

            if (safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y || Screen.orientation != lastOrientation || forceRefresh)
            {
                lastScreenSize.x = Screen.width;
                lastScreenSize.y = Screen.height;
                lastOrientation = Screen.orientation;

                ApplySafeArea(safeArea);
            }
        }

        static void ApplySafeArea(Rect rect)
        {
            lastSafeArea = rect;

            // Ignore x-axis?
            if (!notchSaveArea.conformX)
            {
                rect.x = 0;
                rect.width = Screen.width;
            }

            // Ignore y-axis?
            if (!notchSaveArea.conformY)
            {
                rect.y = 0;
                rect.height = Screen.height;
            }

            // Check for invalid screen startup state on some Samsung devices (see below)
            if (Screen.width > 0 && Screen.height > 0)
            {
                // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
                var anchorMin = rect.position;
                var anchorMax = rect.position + rect.size;

                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                // Fix for some Samsung devices (e.g. Note 10+, A71, S20) where Refresh gets called twice and the first time returns NaN anchor coordinates
                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
                {
                    for (var i = 0; i < registeredTransforms.Count; i++)
                    {
                        if(registeredTransforms[i] != null)
                        {
                            registeredTransforms[i].anchorMax = anchorMax;
                        }
                        else
                        {
                            registeredTransforms.RemoveAt(i);

                            i--;
                        }
                    }
                }
            }
        }
    }
}
