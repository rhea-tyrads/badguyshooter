using TMPro;
using UnityEngine;

namespace MobileTools
{
    [System.Serializable]
    public class MobileTooklitSettings
    {
        [Header("Background")]
        public Sprite background;
        public Color backgroundColor;
        public Color darkOverlayColor;

        [Header("Buttons")]
        public Sprite okButtonSprite;

        [Header("Text")]
        public TMP_FontAsset font;
        public Color textColor;
        public Material textMaterial;

    }
}
