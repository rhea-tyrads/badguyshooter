using UnityEngine;

namespace Watermelon.LevelSystem
{
    public sealed class GateExitPointBehaviour : ExitPointBehaviour
    {
        readonly int IDLE_HASH = Animator.StringToHash("Idle");
        readonly int OPEN_HASH = Animator.StringToHash("Open");

        [SerializeField] Animator gatesAnimator;

        public override void Initialise()
        {
            gatesAnimator.Play(IDLE_HASH);
        }

        public override void OnExitActivated()
        {
            isExitActivated = true;
            gatesAnimator.Play(OPEN_HASH);
            RingEffectController.Spawn(transform.position.SetY(0.1f), 4.5f, 2, Ease.Type.Linear);
            AudioController.Play(AudioController.Sounds.complete);
            Tween.DelayedCall(0.15f, () =>
            {
                AudioController.Play(AudioController.Sounds.door);
            });
        }

        public override void OnPlayerEnteredExit()
        {
            isExitActivated = false;
            LevelController.OnPlayerExitLevel();
        }

        public override void Unload()
        {

        }
    }
}