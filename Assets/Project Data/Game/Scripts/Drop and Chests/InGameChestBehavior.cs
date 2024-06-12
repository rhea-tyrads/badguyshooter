using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class InGameChestBehavior : AbstractChestBehavior
    {
        [SerializeField] float openDuration = 3f;

        [SerializeField] Transform fillCircleHolder;
        [SerializeField] Image fillCircleImage;

        Coroutine openCoroutine;
        TweenCase circleTween;

        public override void Init(List<DropData> drop)
        {
            base.Init(drop);

            fillCircleHolder.localScale = Vector3.zero;
            fillCircleImage.fillAmount = 0f;

            isRewarded = false;
        }

        public override void ChestApproached()
        {
            Appriached = true;
            if (opened) return;

            if (openCoroutine != null)
            {
                StopCoroutine(openCoroutine);
                openCoroutine = null;
            }

            openCoroutine = StartCoroutine(ChestOpenCoroutine());
        }

        IEnumerator ChestOpenCoroutine()
        {
            animatorRef.SetTrigger(SHAKE_HASH);

            var timer = 0f;
            circleTween.KillActive();
            circleTween = fillCircleHolder.DOScale(1f, 0.2f).SetEasing(Ease.Type.CubicOut);

            while (timer < openDuration)
            {
                timer += Time.deltaTime;
                fillCircleImage.fillAmount = timer / openDuration;
                yield return null;
            }

            opened = true;
            animatorRef.SetTrigger(OPEN_HASH);
            fillCircleHolder.localScale = Vector3.zero;

            Tween.DelayedCall(0.3f, () =>
            {
                DropResources();
                particle.SetActive(false);
                Vibration.Vibrate(VibrationIntensity.Light);
            });

            openCoroutine = null;
        }

        public override void ChestLeft()
        {
            Appriached = false;
            if (opened)
                return;

            circleTween.KillActive();

            circleTween = fillCircleHolder.DOScale(0f, 0.2f).SetEasing(Ease.Type.CubicOut);

            animatorRef.SetTrigger(IDLE_HASH);

            if (openCoroutine != null)
            {
                StopCoroutine(openCoroutine);
                openCoroutine = null;
            }
        }
    }
}