using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UICurrencyButton : MonoBehaviour
    {
        [SerializeField] CurrencyType currencyType;
        [SerializeField] DisableMode disableMode;

        [HideIf("IsColorMode")]
        [SerializeField] Sprite activeButtonSprite;
        [HideIf("IsColorMode")]
        [SerializeField] Sprite disabledButtonSprite;

        [HideIf("IsSpriteMode")]
        [SerializeField] Color activeButtonColor;
        [HideIf("IsSpriteMode")]
        [SerializeField] Color disabledButtonColor;

        [Space]
        [SerializeField] Button buttonRef;
        [SerializeField] Image buttonImage;
        [SerializeField] Text buttonText;
        [SerializeField] Image currencyImage;
        [SerializeField] CanvasGroup textAndIconCanvasGroup;

        int currentPrice;
        Currency currency;

        bool isSubscribed;

        void OnEnable()
        {
            Subscribe();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        public void Init(int price)
        {
            Init(price, currencyType);
        }

        public void Init(int price, CurrencyType currencyType)
        {
            this.currencyType = currencyType;
            this.currency = CurrenciesController.GetCurrency(currencyType);

            currentPrice = price;

            currencyImage.sprite = currency.Icon;
            buttonText.text = currency.AmountFormatted;

            Subscribe();

            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            // activate button
            if (currency.Amount >= currentPrice)
            {
                buttonRef.interactable = true;
                textAndIconCanvasGroup.alpha = 1f;

                if (disableMode == DisableMode.Sprite)
                {
                    buttonImage.sprite = activeButtonSprite;
                }
                else
                {
                    buttonImage.color = activeButtonColor;
                }
            }
            // disable button
            else
            {
                buttonRef.interactable = false;
                textAndIconCanvasGroup.alpha = 0.6f;

                if (disableMode == DisableMode.Sprite)
                {
                    buttonImage.sprite = disabledButtonSprite;
                }
                else
                {
                    buttonImage.color = disabledButtonColor;
                }
            }
        }

        void Subscribe()
        {
            if (isSubscribed)
                return;

            if (currency == null)
                return;

            isSubscribed = true;
            currency.OnCurrencyChanged += OnCurrencyChanged;
        }

        void Unsubscribe()
        {
            if (!isSubscribed)
                return;

            if (currency == null)
                return;

            isSubscribed = false;
            currency.OnCurrencyChanged -= OnCurrencyChanged;
        }

        void OnCurrencyChanged(Currency currency, int amountDifference)
        {
            UpdateVisuals();
        }

        #region Editor

        enum DisableMode
        {
            Sprite = 0,
            Color = 1,
        }

        bool IsColorMode()
        {
            return disableMode == DisableMode.Color;
        }

        bool IsSpriteMode()
        {
            return disableMode == DisableMode.Sprite;
        }

        #endregion
    }
}