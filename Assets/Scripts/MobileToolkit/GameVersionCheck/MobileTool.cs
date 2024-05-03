using CoreUI;
using UnityEngine;

public class MobileTool : MonoBehaviour
{
    public MobileScreenUI mobileUI;
    public void Init(MobileTooklitSettings settings)
    {
        mobileUI.SetBackground(settings.background, settings.backgroundColor);
        mobileUI.SetDarkOverlayColor(settings.darkOverlayColor);
        mobileUI.SetFont(settings.font, settings.textColor, settings.textMaterial);
        mobileUI.SetOkayButton(settings.okButtonSprite);
    }
}
