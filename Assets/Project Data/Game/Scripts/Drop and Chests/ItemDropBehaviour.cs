using UnityEngine;
 
using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class ItemDropBehaviour : BaseDropBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] Collider triggerRef;
        [SerializeField] bool useAutoPickup = true;

        TweenCase[] _throwTweenCases;

        public override void Initialise(DropData dropData, float availableToPickDelay = -1, float autoPickDelay = -1,
            bool ignoreCollector = false)
        {
            this.dropData = dropData;
            this.AvailableToPickDelay = availableToPickDelay;
            this.AutoPickDelay = autoPickDelay;
            isPicked = false;
            animator.enabled = false;
            CharacterBehaviour.Died += ItemDisable;
        }

        public override void Drop()
        {
            animator.enabled = true;
            triggerRef.enabled = true;
        }

        public override void Throw(Vector3 position, AnimationCurve movemenHorizontalCurve,
            AnimationCurve movementVerticalCurve, float time)
        {
            LevelController.OnPlayerExitLevelEvent += AutoPick;
            _throwTweenCases = new TweenCase[2];
            triggerRef.enabled = false;
            _throwTweenCases[0] =
                transform.DOMoveXZ(position.x, position.z, time).SetCurveEasing(movemenHorizontalCurve);
            _throwTweenCases[1] = transform.DOMoveY(position.y, time).SetCurveEasing(movementVerticalCurve).OnComplete(
                delegate
                {
                    animator.enabled = true;

                    Tween.DelayedCall(AvailableToPickDelay, () => { triggerRef.enabled = true; });

                    if (AutoPickDelay != -1f)
                    {
                        Tween.DelayedCall(AutoPickDelay, () =>
                        {
                            Pick();
                            CharacterBehaviour.GetBehaviour().OnItemPicked(this);
                        });
                    }
                });
        }

        void AutoPick()
        {
            if (useAutoPickup)
            {
                CharacterBehaviour.GetBehaviour().OnItemPicked(this);
                Pick(false);
            }
            else
            {
                ItemDisable();
            }

            LevelController.OnPlayerExitLevelEvent -= AutoPick;
        }

        public override void Pick(bool moveToPlayer = true)
        {
            LevelController.OnPlayerExitLevelEvent -= AutoPick;

            if (isPicked) return;
            isPicked = true;

            if (!_throwTweenCases.IsNullOrEmpty())
            {
                foreach (var tween in _throwTweenCases)
                    tween.KillActive();
            }

            animator.enabled = false;
            triggerRef.enabled = false;

            if (moveToPlayer)
            {
                transform.DOMove(CharacterBehaviour.Transform.position.SetY(0.625f), 0.3f).SetEasing(Ease.Type.SineIn)
                    .OnComplete(() =>
                    {
                        ItemDisable();
                        if (dropData.dropType == DropableItemType.Currency)
                            AudioController.Play(AudioController.Sounds.coinPickUp, 0.4f);
                    });
            }
            else
            {
                ItemDisable();
            }
        }

        public void ItemDisable()
        {
            CharacterBehaviour.Died -= ItemDisable;
            gameObject.SetActive(false);
        }
    }
}