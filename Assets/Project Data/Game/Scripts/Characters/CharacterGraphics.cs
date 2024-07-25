using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class CharacterGraphics : BaseCharacterGraphics
    {
        static readonly int ANIMATOR_MOVEMENT_SPEED = Animator.StringToHash("Speed");

        static readonly int ANIMATOR_RUNNING_HASH = Animator.StringToHash("IsRunning");
        static readonly int ANIMATOR_MOVEMENT_X_HASH = Animator.StringToHash("MovementX");
        static readonly int ANIMATOR_MOVEMENT_Y_HASH = Animator.StringToHash("MovementY");

        Vector3 _enemyPosition;
        float _angle;
        Vector2 _rotatedInput;

        void Awake()
        {

        }

        public override void OnMovingStarted()
        {
            characterAnimator.SetBool(ANIMATOR_RUNNING_HASH, true);
        }

        public override void OnMovingStoped()
        {
            characterAnimator.SetBool(ANIMATOR_RUNNING_HASH, false);
        }

        public override void OnMoving(float speedPercent, Vector3 direction, bool isTargetFound)
        {
            characterAnimator.SetFloat(ANIMATOR_MOVEMENT_SPEED, CharacterBehaviour.MovementSettings.animationMultiplier.Lerp(speedPercent));

            if (isTargetFound)
            {
                _enemyPosition = CharacterBehaviour.ClosestEnemyBehaviour.transform.position;

                _angle = Mathf.Atan2(_enemyPosition.x - transform.position.x, _enemyPosition.z - transform.position.z) * 180 / Mathf.PI;

                _rotatedInput = Quaternion.Euler(0, 0, _angle) * new Vector2(direction.x, direction.z);

                characterAnimator.SetFloat(ANIMATOR_MOVEMENT_X_HASH, _rotatedInput.x);
                characterAnimator.SetFloat(ANIMATOR_MOVEMENT_Y_HASH, _rotatedInput.y);
            }
            else
            {
                characterAnimator.SetFloat(ANIMATOR_MOVEMENT_X_HASH, 0);
                characterAnimator.SetFloat(ANIMATOR_MOVEMENT_Y_HASH, 1);
            }
        }

        public override void CustomFixedUpdate()
        {

        }

        public override void Unload()
        {

        }

        public override void Reload()
        {
            StopMovementAnimation();
        }

        public override void Activate()
        {
            StopMovementAnimation();
        }

        public override void Disable()
        {
            StopMovementAnimation();
        }

        void StopMovementAnimation()
        {
            characterAnimator.SetFloat(ANIMATOR_MOVEMENT_SPEED, 1.0f);

            characterAnimator.SetFloat(ANIMATOR_MOVEMENT_X_HASH, 0);
            characterAnimator.SetFloat(ANIMATOR_MOVEMENT_Y_HASH, 0);
        }
    }
}