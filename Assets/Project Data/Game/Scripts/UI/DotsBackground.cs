using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class DotsBackground
    {
        static readonly int BASE_COLOR_HASH = Shader.PropertyToID("_BaseColor");
        static readonly int DOTS_COLOR_HASH = Shader.PropertyToID("_DotsColor");

        static readonly int MOVING_SPEED_HASH = Shader.PropertyToID("_MovingSpeed");

        [SerializeField] BackgroundUI backgroundImage;
        public BackgroundUI BackgroundImage => backgroundImage;

        [SerializeField] Color baseColor = Color.white;
        public Color BaseColor => baseColor;

        [SerializeField] Color dotsColor = Color.black;
        public Color DotsColor => dotsColor;

        [Space]
        [SerializeField] Vector2 movingSpeed;

        public void ApplyParams()
        {
            var material = backgroundImage.material;

            material.SetColor(BASE_COLOR_HASH, baseColor);
            material.SetColor(DOTS_COLOR_HASH, dotsColor);

            material.SetVector(MOVING_SPEED_HASH, movingSpeed);
        }
    }
}