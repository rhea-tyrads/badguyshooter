using UnityEngine.Serialization;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterDynamicAnimation
    {
        [FormerlySerializedAs("CharacterPanel")] public CharacterPanelUI characterPanel;

        [FormerlySerializedAs("Delay")] public float delay;

        public SimpleCallback AnimationStarted;

        public CharacterDynamicAnimation(CharacterPanelUI characterPanel, float delay, SimpleCallback onAnimationStarted)
        {
            this.characterPanel = characterPanel;
            this.delay = delay;

            AnimationStarted = onAnimationStarted;
        }
    }
}