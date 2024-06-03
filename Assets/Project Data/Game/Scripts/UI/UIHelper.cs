using UnityEngine;

namespace Watermelon.SquadShooter
{
    public static class UIHelper
    {
        public const float PANEL_HEIGHT = 115.0f;

        static readonly Vector2[] PANEL_SIZES = new Vector2[]
        {
        new(200.0f, PANEL_HEIGHT),
        new(215.0f, PANEL_HEIGHT),
        new(240.0f, PANEL_HEIGHT),
        new(268.0f, PANEL_HEIGHT),
        new(285.0f, PANEL_HEIGHT),
        new(300.0f, PANEL_HEIGHT)
        };

        public static Vector2 GetPanelSize(int charactersCount)
        {
            if (PANEL_SIZES.IsInRange(charactersCount))
                return PANEL_SIZES[charactersCount];

            return PANEL_SIZES[PANEL_SIZES.Length - 1];
        }
    }
}