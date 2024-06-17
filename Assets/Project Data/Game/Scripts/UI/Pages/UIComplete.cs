using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        public float claimButtonDelay = 2f;
        public Button claimButton;
        const string LEVEL_TEXT = "LEVEL {0}-{1}";
        const string PLUS_TEXT = "+{0}";
        [SerializeField] Button doubleRewardButton;
        [SerializeField] DotsBackground dotsBackground;
        [SerializeField] RectTransform panelRectTransform;

        [SerializeField] CanvasGroup panelContentCanvasGroup;
        [SerializeField] TextMeshProUGUI levelText;

        [Space]
        [SerializeField] GameObject dropCardPrefab;
        [SerializeField] Transform cardsContainerTransform;

        [Space]
        [SerializeField] TextMeshProUGUI experienceGainedText;
        [SerializeField] TextMeshProUGUI moneyGainedText;

        int currentWorld;
        int currentLevel;
        int collectedMoney;
        int collectedExperience;
        List<WeaponType> collectedCards;

        Pool cardsUIPool;

        public override void Initialise()
        {
            cardsUIPool = new Pool(new PoolSettings(dropCardPrefab.name, dropCardPrefab, 1, true, cardsContainerTransform));
        }

        public void SetData(int currentWorld, int currentLevel, int collectedMoney, int collectedExperience, List<WeaponType> collectedCards)
        {
            this.currentWorld = currentWorld;
            this.currentLevel = currentLevel;
            this.collectedMoney = collectedMoney;
            this.collectedExperience = collectedExperience;
            this.collectedCards = collectedCards;
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            claimButton.gameObject.SetActive(false);
            var showTime = 0.7f;

            dotsBackground.ApplyParams();

            cardsUIPool.ReturnToPoolEverything();

            // RESET
            panelRectTransform.sizeDelta = new Vector2(0, 335f);
            dotsBackground.BackgroundImage.color = Color.white.SetAlpha(0.0f);
            panelContentCanvasGroup.alpha = 0;

            levelText.text = string.Format(LEVEL_TEXT, currentWorld, currentLevel);

            dotsBackground.BackgroundImage.DOColor(Color.white, 0.3f);
            panelContentCanvasGroup.DOFade(1.0f, 0.3f, 0.1f);

            moneyGainedText.text = "0";
            Tween.DoFloat(0, collectedMoney, 0.4f, (result) =>
            {
                moneyGainedText.text = string.Format(PLUS_TEXT, result.ToString("00"));
            }, 0.2f);

            experienceGainedText.text = "0";
            Tween.DoFloat(0, collectedExperience, 0.4f, (result) =>
            {
                experienceGainedText.text = string.Format(PLUS_TEXT, result.ToString("00"));
            }, 0.3f);

            var cardsDropped = !collectedCards.IsNullOrEmpty();
            if(cardsDropped)
            {
                var uniqueCards = new List<WeaponType>();
                foreach (var type in collectedCards.Where(type => uniqueCards.FindIndex(x => x == type) == -1))
                    uniqueCards.Add(type);

                for (var i = 0; i < uniqueCards.Count; i++)
                {
                    var cardUIObject = cardsUIPool.Get();
                    cardUIObject.SetActive(true);

                    var droppedCardPanel = cardUIObject.GetComponent<DroppedCardPanel>();
                    droppedCardPanel.Initialise(uniqueCards[i]);

                    var droppedCardCanvasGroup = droppedCardPanel.CanvasGroup;
                    droppedCardCanvasGroup.alpha = 0.0f;
                    droppedCardCanvasGroup.DOFade(1.0f, 0.5f, 0.1f * i + 0.45f).OnComplete(delegate
                    {
                        droppedCardPanel.OnDisplayed();
                    });
                }

                panelRectTransform.DOSize(new Vector2(0, 815), 0.4f).SetEasing(Ease.Type.BackOut);

                showTime = 1.1f;
            }

            Tween.DelayedCall(showTime, () => { 
                UIController.OnPageOpened(this);
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Complete);
            });

            UIGamepadButton.DisableTag(UIGamepadButtonTag.Game);
            
            Invoke(nameof(ShowClaim), claimButtonDelay);
        }

        void ShowClaim()
        {
            claimButton.gameObject.SetActive(true);
        }
        public override void PlayHideAnimation()
        {
            if (!isPageDisplayed)
                return;

            Overlay.Show(0.3f, () =>
            {
                UIController.OnPageClosed(this);

                Overlay.Hide(0.3f, null);
            });
        }

        #endregion

        #region Experience
        public void UpdateExperienceLabel(int experienceGained)
        {
            experienceGainedText.text = experienceGained.ToString();
        }

        #endregion

        public void ContinueButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            GameController.OnLevelCompleteClosed();
        }
    }
}
