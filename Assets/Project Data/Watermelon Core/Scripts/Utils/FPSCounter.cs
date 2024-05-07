using UnityEngine;

namespace Watermelon
{
    public class FPSCounter : MonoBehaviour
    {
        float fps = 0.0f;

        bool isStyleInitted = false;

        GUIStyle labelStyle;

        void OnGUI()
        {
            if (!isStyleInitted)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleRight;
                labelStyle.fontStyle = FontStyle.Bold;

                isStyleInitted = true;
            }

            GUI.Label(new Rect(Screen.width - 210, 0, 200, 20), fps.ToString("00"), labelStyle);
        }

        void Update()
        {
            fps = (int)(1f / Time.unscaledDeltaTime);
        }
    }
}