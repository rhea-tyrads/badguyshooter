using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [SerializeField] DotsBackground dotsBackground;
        [SerializeField] CanvasGroup contentCanvasGroup;

        [Space]
        [SerializeField] RewardedVideoButton reviveButton;
        [SerializeField] Button continueButton;

        [Space] 
        [SerializeField] TMP_Text tapToContinueText;
        public RewardedVideoButton button;
        public override void Initialise()
        {
            reviveButton.Initialise(Revive, GameController.Settings.RevivePrice);
            continueButton.onClick.AddListener(Replay);
        }
 
        public override void PlayShowAnimation()
        {
            dotsBackground.ApplyParams();
            contentCanvasGroup.alpha = 0.0f;
            contentCanvasGroup.DOFade(1.0f, 0.4f).SetDelay(0.1f);
            dotsBackground.BackgroundImage.color = Color.white.SetAlpha(0.0f);
            
            dotsBackground.BackgroundImage.DOFade(1.0f, 0.5f).OnComplete(delegate
            {
                reviveButton.enabled = true;
                UIController.OnPageOpened(this);
                UIGamepadButton.EnableTag(UIGamepadButtonTag.GameOver);
            });

            continueButton.enabled = false;
            reviveButton.enabled = false;
            tapToContinueText.alpha = 0;
            tapToContinueText.DOFade(1, 0.5f, 3).OnComplete(() => continueButton.enabled = true);
            UIGamepadButton.DisableTag(UIGamepadButtonTag.Game);
            button.Redraw();
        }

        public override void PlayHideAnimation()
        {
            contentCanvasGroup.DOFade(0.0f, 0.2f);
            dotsBackground.BackgroundImage.DOColor(Color.black, 0.3f).OnComplete(delegate
            {
                UIController.OnPageClosed(this);
            });
        }
        
        public void Replay()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            GameController.OnReplayLevel();
        }

        public void Revive(bool success)
        {
            if (success)
                GameController.OnRevive();
            else
                GameController.OnReplayLevel();
        }
      
    }
}
