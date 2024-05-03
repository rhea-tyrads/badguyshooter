using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoreUI
{
    public class MobileScreenUI : ScreenUI
    {
        public Image background;
        public Image darkOverlay;
        public Image okButtonImage;

        public void SetBackground(Sprite sprite, Color color)
        {
            if (!background) return;
            background.sprite = sprite;
            background.color = color;
        }
        public void SetOkayButton(Sprite sprite)
        {
            if (!okButtonImage) return;
            okButtonImage.sprite = sprite;
        }
        public void SetDarkOverlayColor(Color color)
        {
            if (!darkOverlay) return;
            darkOverlay.color = color;
        }
        public void SetFont(TMP_FontAsset font, Color textColor, Material textMaterial)
        {
            var txts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var txt in txts)
            {
                txt.font = font;
                txt.color = textColor;
                txt.material = textMaterial;
            }
        }
    }
}