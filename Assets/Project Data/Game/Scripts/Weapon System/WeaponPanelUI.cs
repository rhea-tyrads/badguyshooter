using System.Collections.Generic;
using com.adjust.sdk;
using MobileTools.SDK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class WeaponPanelUI : UIUpgradeAbstractPanel
    {
        #region Inspector

        [SerializeField] GameObject shopState;
        [SerializeField] TextMeshProUGUI weaponName;
        [SerializeField] Image weaponImage;
        [SerializeField] Image weaponBackImage;
        [SerializeField] TextMeshProUGUI rarityText;

        [Header("Locked State")]
        [SerializeField] GameObject lockedStateObject;
        [SerializeField] SlicedFilledImage cardsFillImage;
        [SerializeField] TextMeshProUGUI cardsAmountText;

        [Header("Upgrade State")]
        [SerializeField] TextMeshProUGUI levelText;
        [SerializeField] GameObject upgradeStateObject;
        [SerializeField] TextMeshProUGUI upgradePriceText;
        [SerializeField] Image upgradeCurrencyImage;

        [Space]
        [SerializeField] Color upgradeStateActiveColor = Color.white;
        [SerializeField] Color upgradeStateUnactiveColor = Color.white;
        [SerializeField] Image[] upgradesStatesImages;

        public WeaponData Data { get; private set; }

        BaseWeaponUpgrade Upgrade { get; set; }

        [Space]
        [SerializeField] Button upgradesBuyButton;
        [SerializeField] Image upgradesBuyButtonImage;
        [SerializeField] TextMeshProUGUI upgradesBuyButtonText;
        [SerializeField] Sprite upgradesBuyButtonActiveSprite;
        [SerializeField] Sprite upgradesBuyButtonDisableSprite;

        [Space]
        [SerializeField] GameObject upgradesMaxObject;

        public override bool IsUnlocked => Upgrade.UpgradeLevel > 0;
        int weaponIndex;
        public int WeaponIndex => weaponIndex;

        UIGamepadButton gamepadButton;
        public UIGamepadButton GamepadButton => gamepadButton;

        public Transform UpgradeButtonTransform => upgradesBuyButton.transform;

        WeaponsController weaponController;
        bool _AvailableOnlyInShop;

        #endregion

        public void Init(WeaponsController controller, BaseWeaponUpgrade upgrade, WeaponData data, int index,
            bool availableOnlyInShop)
        {
            _AvailableOnlyInShop = PlayerPrefs.HasKey("WEAPON_CHEAT") || availableOnlyInShop;

            Data = data;
            Upgrade = upgrade;
            panelRectTransform = (RectTransform) transform;
            gamepadButton = upgradesBuyButton.GetComponent<UIGamepadButton>();
            weaponIndex = index;
            weaponController = controller;
            weaponName.text = data.Name;
            weaponImage.sprite = data.Icon;
            weaponBackImage.color = data.RarityData.MainColor;
            rarityText.text = data.RarityData.Name;
            rarityText.color = data.RarityData.TextColor;

            UpdateUI();
            UpdateSelectionState();
            WeaponsController.OnNewWeaponSelected += UpdateSelectionState;
        }

        public bool IsNextUpgradeCanBePurchased() =>
            IsUnlocked && !Upgrade.IsMaxedOut &&
            CurrenciesController.HasAmount(CurrencyType.Coins, Upgrade.NextStage.Price);

        public void UpdateUI()
        {
            if (IsUnlocked) Unlock();
            else Lock();
        }

        void UpdateSelectionState()
        {
            if (weaponIndex == WeaponsController.SelectedWeaponIndex)
            {
                selectionImage.gameObject.SetActive(true);
                backgroundTransform.localScale = Vector3.one;
            }
            else
            {
                selectionImage.gameObject.SetActive(false);
                backgroundTransform.localScale = Vector3.one;
            }

            UpdateUI();
        }

        void Lock()
        {
            if (_AvailableOnlyInShop)
            {
                lockedStateObject.SetActive(false);
                upgradeStateObject.SetActive(false);
                shopState.SetActive(true);
            }
            else
            {
                lockedStateObject.SetActive(true);
                upgradeStateObject.SetActive(false);
                shopState.SetActive(false);

                var currentAmount = Data.CardsAmount;
                var target = Upgrade.NextStage.Price;

                cardsFillImage.fillAmount = (float) currentAmount / target;
                cardsAmountText.text = currentAmount + "/" + target;
            }

            powerObject.SetActive(false);
            powerText.gameObject.SetActive(false);
            if (SDKEvents.Instance)
                SDKEvents.Instance.WeaponUpgradeNotPossibile();
        }

        void Unlock()
        {
            lockedStateObject.SetActive(false);
            upgradeStateObject.SetActive(true);
            shopState.SetActive(false);

            if (Upgrade.NextStage != null)
            {
                if (SDKEvents.Instance)
                    SDKEvents.Instance.WeaponUpgradePossibile(Upgrade.NextStage.Price);

                upgradePriceText.text = Upgrade.NextStage.Price.ToString();
                upgradeCurrencyImage.sprite = CurrenciesController.GetCurrency(Upgrade.NextStage.CurrencyType).Icon;
            }
            else
            {
                if (SDKEvents.Instance)
                    SDKEvents.Instance.WeaponUpgradeMaxed();

                upgradePriceText.text = "MAXED OUT";
                upgradeCurrencyImage.gameObject.SetActive(false);
            }

            powerObject.SetActive(true);
            powerText.gameObject.SetActive(true);
            powerText.text = Upgrade.GetCurrentStage().Power.ToString();

            RedrawUpgradeElements();
        }

        void RedrawUpgradeElements()
        {
            levelText.text = "LEVEL " + Upgrade.UpgradeLevel;

            if (Upgrade.IsMaxedOut)
            {
                upgradesMaxObject.SetActive(true);
                // upgradesBuyButton.gameObject.SetActive(false);

                if (gamepadButton)
                    gamepadButton.SetFocus(false);
            }
            else
            {
                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);

                RedrawUpgradeButton();
            }
        }

        protected override void RedrawUpgradeButton()
        {
            if (Upgrade.IsMaxedOut) return;
            var price = Upgrade.NextStage.Price;
            var currencyType = Upgrade.NextStage.CurrencyType;

            if (CurrenciesController.HasAmount(currencyType, price))
            {
                //  upgradesBuyButtonImage.sprite = upgradesBuyButtonActiveSprite;
                if (gamepadButton)
                    gamepadButton.SetFocus(weaponIndex == WeaponsController.SelectedWeaponIndex);
            }
            else
            {
                //  upgradesBuyButtonImage.sprite = upgradesBuyButtonDisableSprite;
                if (gamepadButton)
                    gamepadButton.SetFocus(false);
            }

            upgradesBuyButtonText.text = CurrenciesHelper.Format(price);
        }

        public override void Select()
        {
            // Debug.LogError("SELECTED");
            if (!IsUnlocked) return;

            // if (weaponIndex != WeaponsController.SelectedWeaponIndex)
            //  {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            weaponController.OnWeaponSelected(weaponIndex);
            // }

            UIGeneralPowerIndicator.UpdateText();
        }

        public void UpgradePlease()
        {
             
           if (Upgrade.NextStage.Price >
               CurrenciesController.GetCurrency(Upgrade.NextStage.CurrencyType).Amount) return;
           SendAdjustEvent();
           AudioController.PlaySound(AudioController.Sounds.buttonSound);
           CurrenciesController.Add(Upgrade.NextStage.CurrencyType, -Upgrade.NextStage.Price);
           Upgrade.UpgradeStage();
           weaponController.WeaponUpgraded(Data);
           UIGeneralPowerIndicator.UpdateText(true);
        }
        public void UpgradeButton()
        {
            /*
            if (Upgrade.NextStage.Price >
                CurrenciesController.GetCurrency(Upgrade.NextStage.CurrencyType).Amount) return;
                */
   
           // SendAdjustEvent();
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            Select();
            return;
            CurrenciesController.Add(Upgrade.NextStage.CurrencyType, -Upgrade.NextStage.Price);
            Upgrade.UpgradeStage();
            weaponController.WeaponUpgraded(Data);

            UIGeneralPowerIndicator.UpdateText(true);
        }

        void SendAdjustEvent()
        {
            var token = Data.Type switch
            {
                WeaponType.CrossBow => "cqttkc",
                WeaponType.Flamethrower => "38achz",
                WeaponType.Laser => "sfh9j8",
                WeaponType.LavaLauncher => "8464n9",
                WeaponType.Minigun => "mja0ls",
                WeaponType.PoisonGun => "ht3riw",
                WeaponType.Revolver => "qqksvy",
                WeaponType.Shotgun => "8dozub",
                WeaponType.TeslaGun => "wlg785",
                _ => string.Empty
            };

            var send = new AdjustEvent(token);
            Adjust.trackEvent(send);
        }

        void OnDisable()
        {
            WeaponsController.OnNewWeaponSelected += UpdateSelectionState;
        }
    }
}