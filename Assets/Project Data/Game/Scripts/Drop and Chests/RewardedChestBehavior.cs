using System.Collections.Generic;
using Applovin;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class RewardedChestBehavior : AbstractChestBehavior
    {
        protected static readonly int IS_OPEN_HASH = Animator.StringToHash("IsOpen");

        [SerializeField] Animator rvAnimator;
        [SerializeField] Button rvButton;
        [SerializeField] Transform adHolder;
        [SerializeField] Canvas adCanvas;
        [SerializeField] UIGamepadButton gamepadButton;

        void Awake()
        {
            rvButton.onClick.AddListener(OnButtonClick);
            adHolder.transform.localScale = Vector3.zero;
        }

        void LateUpdate()
        {
            adCanvas.transform.forward = Camera.main.transform.forward;
        }

        public override void Init(List<DropData> drop)
        {
            base.Init(drop);
            rvAnimator.transform.localScale = Vector3.zero;
            isRewarded = true;
        }

        public override void ChestApproached()
        {
            Appriached = true;
            if (opened) return;

            animatorRef.SetTrigger(SHAKE_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, true);

            gamepadButton.SetFocus(true);
        }

        public override void ChestLeft()
        {
            Appriached = false;
            if (opened)
                return;

            animatorRef.SetTrigger(IDLE_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, false);

            gamepadButton.SetFocus(false);
        }

        void OnButtonClick()
        {
          //  ApplovinController.Instance.ShowRewarded("rewarded chest");
            Receive();
            ApplovinController.Instance.OnRewardReceived -= Receive;
            ApplovinController.Instance.OnRewardDisplayFail -= Fail;
            ApplovinController.Instance.OnRewardReceived += Receive;
            ApplovinController.Instance.OnRewardDisplayFail += Fail;
            // AdsManager.ShowRewardBasedVideo((success) =>
            // {
            //     if (success)
            //     {
            //         opened = true;
            //
            //         animatorRef.SetTrigger(OPEN_HASH);
            //         rvAnimator.SetBool(IS_OPEN_HASH, false);
            //
            //         Tween.DelayedCall(0.3f, () =>
            //         {
            //             DropResources();
            //             particle.SetActive(false);
            //             Vibration.Vibrate(VibrationIntensity.Light);
            //         });
            //
            //         gamepadButton.SetFocus(false);
            //     } 
            // });
        }

        void Fail()
        {
            opened = true;
            animatorRef.SetTrigger(OPEN_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, false);
            Tween.DelayedCall(0.3f, () =>
            {
                particle.SetActive(false);
            });
            gamepadButton.SetFocus(false);
        }

        void Receive()
        {
            opened = true;
            animatorRef.SetTrigger(OPEN_HASH);
            rvAnimator.SetBool(IS_OPEN_HASH, false);
            Tween.DelayedCall(0.3f, () =>
            {
                DropResources();
                particle.SetActive(false);
                Vibration.Vibrate(VibrationIntensity.Light);
            });
            gamepadButton.SetFocus(false);
        }
    }
}