using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Applovin;
using MobileTools.Utilities;
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

        [Space] [SerializeField] GameObject dropCardPrefab;
        [SerializeField] Transform cardsContainerTransform;

        [Space] [SerializeField] TextMeshProUGUI experienceGainedText;
        [SerializeField] TextMeshProUGUI moneyGainedText;

        int currentWorld;
        int currentLevel;
        public int collectedMoney;
        int collectedExperience;
        List<WeaponType> collectedCards;

        Pool cardsUIPool;

        public override void Initialise()
        {
            cardsUIPool =
                new Pool(new PoolSettings(dropCardPrefab.name, dropCardPrefab, 1, true, cardsContainerTransform));
        }

        public void SetData(int currentWorld, int currentLevel, int collectedMoney, int collectedExperience,
            List<WeaponType> collectedCards)
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
            //    panelRectTransform.sizeDelta = new Vector2(0, 335f);
            dotsBackground.BackgroundImage.color = Color.white.SetAlpha(0.0f);
            panelContentCanvasGroup.alpha = 0;
            levelText.text = string.Format(LEVEL_TEXT, currentWorld, currentLevel);
            dotsBackground.BackgroundImage.DOColor(Color.white, 0.3f);
            panelContentCanvasGroup.DOFade(1.0f, 0.3f, 0.1f);
            moneyGainedText.text = "0";

            Tween.DoFloat(0, collectedMoney, 0.4f,
                (result) => { moneyGainedText.text = string.Format(PLUS_TEXT, result.ToString("00")); }, 0.2f);

            experienceGainedText.text = "0";
            Tween.DoFloat(0, collectedExperience, 0.4f,
                (result) => { experienceGainedText.text = string.Format(PLUS_TEXT, result.ToString("00")); }, 0.3f);

            var cardsDropped = !collectedCards.IsNullOrEmpty();
            if (cardsDropped)
            {
                var uniqueCards = new List<WeaponType>();
                foreach (var type in collectedCards.Where(type => uniqueCards.FindIndex(x => x == type) == -1))
                    uniqueCards.Add(type);

                for (var i = 0; i < uniqueCards.Count; i++)
                {
                    var ui = cardsUIPool.Get();
                    ui.SetActive(true);

                    var panel = ui.GetComponent<DroppedCardPanel>();
                    panel.Initialise(uniqueCards[i]);

                    var canvasGroup = panel.CanvasGroup;
                    canvasGroup.alpha = 0.0f;
                    canvasGroup.DOFade(1.0f, 0.5f, 0.1f * i + 0.45f).OnComplete(delegate { panel.OnDisplayed(); });
                }

                //  panelRectTransform.DOSize(new Vector2(0, 1100), 0.4f).SetEasing(Ease.Type.BackOut);
                showTime = 1.1f;
            }

            var size = cardsDropped ? 1100 : 650;
            panelRectTransform.DOSize(new Vector2(0, size), 0.4f).SetEasing(Ease.Type.BackOut);

            Tween.DelayedCall(showTime, () =>
            {
                UIController.OnPageOpened(this);
                UIGamepadButton.EnableTag(UIGamepadButtonTag.Complete);
            });

            UIGamepadButton.DisableTag(UIGamepadButtonTag.Game);

            if (Keys.IsNoAdsPurchased)
            {
                foreach (var go in noAdsModeEnable) go.SetActive(true);
                foreach (var go in noAdsModeDisable) go.SetActive(false);
                doubleRewardButton.gameObject.SetActive(true);
                
                doubleRewardButton.onClick.RemoveListener(Receive);
                doubleRewardButton.onClick.AddListener(Receive);
                
            }
            else
            {
                doubleRewardButton.gameObject.SetActive(ApplovinController.Instance.IsRewardedLoaded);
                doubleRewardButton.onClick.RemoveListener(DoubleReward);
                doubleRewardButton.onClick.AddListener(DoubleReward);
                if (ApplovinController.Instance.IsRewardedLoaded) ShowClaim();
                else Invoke(nameof(ShowClaim), claimButtonDelay);
            }
        }

        public List<GameObject> noAdsModeDisable = new();
        public List<GameObject> noAdsModeEnable = new();

        void DoubleReward()
        {
            ApplovinController.Instance.ShowRewarded("x2 Rewards");
            ApplovinController.Instance.OnRewardReceived -= Receive;
            ApplovinController.Instance.OnRewardDisplayFail -= Fail;
            ApplovinController.Instance.OnRewardReceived += Receive;
            ApplovinController.Instance.OnRewardDisplayFail += Fail;
        }

        void Receive()
        {
            var r = Random.Range(0, 3);
            switch (r)
            {
                case 0:
                    BonusController.Instance.AddCrit();
                    break;
                case 1:
                    BonusController.Instance.AddHp();
                    break;
                case 2:
                    BonusController.Instance.AddRespawn();
                    break;
            }

            GameController.IsDoubleReward = true;
            CurrenciesController.Add(CurrencyType.Coins, collectedMoney);
            ContinueButton();
        }

        void Fail()
        {
            ContinueButton();
        }

        void ShowClaim()
        {
            claimButton.gameObject.SetActive(true);
        }

        public override void PlayHideAnimation()
        {
            if (!isPageDisplayed) return;

            Overlay.Show(0.3f, () =>
            {
                UIController.OnPageClosed(this);
                Overlay.Hide(0.3f, null);
            });
        }

        #endregion


        public void UpdateExperienceLabel(int experienceGained)
        {
            experienceGainedText.text = experienceGained.ToString();
        }


        public void ContinueButton()
        {
            AudioController.Play(AudioController.Sounds.buttonSound);
            GameController.OnLevelCompleteClosed();
        }
    }
}