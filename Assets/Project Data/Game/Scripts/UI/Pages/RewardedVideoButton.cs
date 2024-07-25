using System;
using Applovin;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Watermelon
{
    public class RewardedVideoButton : MonoBehaviour
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] Sprite activeBackgroundSprite;
        [SerializeField] Sprite blockedBackgroundSprite;

        [Space]
        [SerializeField] GameObject adsContentObject;

        [Space]
        [SerializeField] GameObject currencyContentObject;
        [SerializeField] Image currencyIconImage;
        [SerializeField] TextMeshProUGUI currencyText;

        Button button;
        public Button Button => button;

        CurrencyPrice currencyPrice;
        SimpleBoolCallback completeCallback;

        bool isInitialised;
        Currency currency;

        public void Initialise(SimpleBoolCallback completeCallback, CurrencyPrice currencyPrice)
        {
            if(currencyPrice == null) Debug.LogWarning("!!!!!!!!!!!!!!!!!!!!!");
            NOT_SHOW_YET = true;
            this.completeCallback = completeCallback;
            this.currencyPrice = currencyPrice;

            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);

            currency = currencyPrice.Currency;
            currency.OnCurrencyChanged += OnCurrencyChanged;

            isInitialised = true;

            Redraw();
        }

        void OnCurrencyChanged(Currency currency, int difference)
        {
            if (!isInitialised) return;
            if (AdsManager.Settings.RewardedVideoType != AdProvider.Disable) return;

            Redraw();
        }

        void FixedUpdate()
        {
            Redraw();
        }

        public bool NOT_SHOW_YET;
        public void Redraw()
        {
//            Debug.LogError("REDRAW");

         //   var span = DateUtils.SecondsPassed("REWARDED_TIMER");
            // Activate currency purchase option if RV is disabled
            if (ApplovinController.Instance.IsRewardedLoaded && NOT_SHOW_YET)
            {
 
                //DateUtils.Save("REWARDED_TIMER", DateTime.Now);
           //     Debug.LogError("ads!");
                //Debug.LogError("ADS");
                currencyContentObject.SetActive(false);
                adsContentObject.SetActive(true);
                backgroundImage.sprite = activeBackgroundSprite;
            }
            else
            {
             //   Debug.LogError("no ads!");
                //Debug.LogError("SIMLEEEEEEEEEEe");
                adsContentObject.SetActive(false);
                currencyContentObject.SetActive(true);
                var currency = currencyPrice.Currency;
                currencyIconImage.sprite = currency.Icon;
                currencyText.text = currencyPrice.FormattedPrice;
                backgroundImage.sprite = currencyPrice.EnoughMoneyOnBalance()
                    ? activeBackgroundSprite
                    : blockedBackgroundSprite;
            }
        }

        void OnButtonClicked()
        {
            //Debug.LogError("BUTTON CLICK");
            AudioController.Play(AudioController.Sounds.buttonSound);

            if (currencyContentObject.activeSelf)
            {
                Debug.LogError("REWIWWW BY COINS");
                if (currencyPrice.EnoughMoneyOnBalance())
                {
                    currencyPrice.SubstractFromBalance();
                    completeCallback?.Invoke(true);
                }
                else
                {
                    completeCallback?.Invoke(false);
                }
            }
            else
            {
                NOT_SHOW_YET = false;
                Debug.LogError("REWIWWW BY ADS");
                AdsManager.ShowRewardBasedVideo(success => { completeCallback?.Invoke(success); });
                ApplovinController.Instance.OnRewardReceived -= Good;
                ApplovinController.Instance.OnRewardReceived += Good;
                ApplovinController.Instance.OnRewardDisplayFail -= NotGood;
                ApplovinController.Instance.OnRewardDisplayFail += NotGood;
            }
        }

        void NotGood()
        {
            completeCallback?.Invoke(false);
        }

        void Good()
        {
            completeCallback?.Invoke(true);
        }

        public void Clear()
        {
            isInitialised = false;
            completeCallback = null;
            currencyPrice = null;

            if (currency != null)
            {
                currency.OnCurrencyChanged -= OnCurrencyChanged;
                currency = null;
            }

            button.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }
    }
}