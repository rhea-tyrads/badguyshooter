using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class CharacterPanelUI : UIUpgradeAbstractPanel
    {
        const string LOCKED_NAME = "???";
        const string UPGRADE_TEXT = "UPGRADE";
        const string EVOLVE_TEXT = "EVOLVE";

        [SerializeField] Image previewImage;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] Button mainButton;

        [Header("Upgrades")]
        [SerializeField] GameObject upgradesStateObject;

        [Space]
        [SerializeField] Color upgradeStateActiveColor = Color.white;
        [SerializeField] Image[] upgradesStatesImages;

        [Space]
        [SerializeField] Button upgradesBuyButton;
        [SerializeField] Image upgradesBuyButtonImage;
        [SerializeField] Image upgradesBuyCurrencyImage;
        [SerializeField] TextMeshProUGUI upgradesBuyButtonText;
        [SerializeField] Sprite upgradesBuyButtonActiveSprite;
        [SerializeField] Sprite upgradesBuyButtonDisableSprite;
        [SerializeField] TextMeshProUGUI upgradesText;

        [Space]
        [SerializeField] GameObject upgradesMaxObject;

        [Header("Locked State")]
        [SerializeField] GameObject lockedStateObject;
        [SerializeField] TextMeshProUGUI lockedStateText;
        [SerializeField] Color lockedPreviewColor = Color.white;

        public override bool IsUnlocked => Character.IsUnlocked();

        Character character;
        public Character Character => character;

        bool storedIsLocked;

        public Transform UpgradeButtonTransform => upgradesBuyButton.transform;

        UICharactersPanel charactersPanel;

        UIGamepadButton gamepadButton;
        public UIGamepadButton GamepadButton => gamepadButton;

        static CharacterPanelUI selectedCharacterPanelUI;

        public bool IsSelected() => selectedCharacterPanelUI == this;

        public void Initialise(Character с, UICharactersPanel panel)
        {
            character = с;
            charactersPanel = panel;

            panelRectTransform = (RectTransform) transform;
            gamepadButton = upgradesBuyButton.GetComponent<UIGamepadButton>();

            previewImage.sprite = с.GetCurrentStage().PreviewSprite;

            for (var i = 0; i < upgradesStatesImages.Length; i++)
            {
                if (!с.Upgrades.IsInRange(i + 1)) continue;
                if (!с.Upgrades[i + 1].ChangeStage) continue;

                var stageStarObject = panel.GetStageStarObject();
                stageStarObject.transform.SetParent(upgradesStatesImages[i].rectTransform);
                stageStarObject.transform.ResetLocal();

                var stageStarRectTransform = (RectTransform) stageStarObject.transform;
                stageStarRectTransform.sizeDelta = new Vector2(19.4f, 19.4f);
                stageStarRectTransform.anchoredPosition = Vector2.zero;

                stageStarObject.SetActive(true);
            }

            if (с.IsUnlocked())
            {
                titleText.text = с.Name.ToUpper();
                storedIsLocked = false;

                if (CharactersController.SelectedCharacter.Type == с.Type)
                    Select();

                lockedStateObject.SetActive(false);
                upgradesStateObject.SetActive(true);
                powerObject.SetActive(true);

                RedrawUpgradeElements();
                RedrawPower();
            }
            else
            {
                titleText.text = LOCKED_NAME;
                storedIsLocked = true;
                powerObject.SetActive(false);
                previewImage.sprite = с.LockedSprite;
                previewImage.color = lockedPreviewColor;
                SetRequiredLevel(с.RequiredLevel);
            }

            mainButton.onClick.AddListener(OnSelectButtonClicked);
        }

        void PlayOpenAnimation()
        {
            lockedStateObject.SetActive(false);
            powerObject.SetActive(true);
            upgradesStateObject.SetActive(true);
            titleText.text = character.Name.ToUpper();
            previewImage.sprite = character.Stages[0].PreviewSprite;
            previewImage.DOColor(Color.white, 0.6f);
            RedrawUpgradeElements();
            RedrawPower();
        }

        void PlayUpgradeAnimation(int stage)
        {
            var tempStage = character.Stages[stage];
            previewImage.sprite = tempStage.PreviewSprite;
            previewImage.rectTransform.localScale = Vector2.one * 1.3f;
            previewImage.rectTransform.DOScale(1.0f, 0.2f, 0.03f).SetEasing(Ease.Type.SineIn);
        }

        public override void OnPanelOpened()
        {
            var dynamicAnimations = new List<CharacterDynamicAnimation>();
            var isSelected = character.IsSelected();

            // Character was locked on start
            if (storedIsLocked)
            {
                // Now character is unlocked
                if (character.IsUnlocked())
                {
                    storedIsLocked = false;
                    var unlockAnimation =
                        new CharacterDynamicAnimation(this, 0.5f, onAnimationStarted: PlayOpenAnimation);
                    dynamicAnimations.Add(unlockAnimation);
                }
            }

            if (!dynamicAnimations.IsNullOrEmpty())
            {
                charactersPanel.AddAnimations(dynamicAnimations, isSelected);
            }

            RedrawUpgradeElements();
            RedrawPower();
        }

        public void SetRequiredLevel(int level)
        {
            lockedStateObject.SetActive(true);
            lockedStateText.text = level.ToString();
        }

        public override void Select()
        {
            if (selectedCharacterPanelUI) selectedCharacterPanelUI.UnselectCharacter();
            selectionImage.gameObject.SetActive(true);
            selectedCharacterPanelUI = this;
            UIGeneralPowerIndicator.UpdateText();
            RedrawUpgradeButton();
        }

        public void UnselectCharacter()
        {
            selectionImage.gameObject.SetActive(false);
            if (gamepadButton)
                gamepadButton.SetFocus(false);
        }

        void PlayUpgradeAnimation()
        {
            var upgradeStateIndex = character.GetCurrentUpgradeIndex() - 1;
            upgradesStatesImages[upgradeStateIndex].DOColor(upgradeStateActiveColor, 0.3f).OnComplete(delegate
            {
                isUpgradeAnimationPlaying = false;

                RedrawUpgradeButton();
            });

            if (!character.IsMaxUpgrade())
            {
                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);
            }
            else
            {
                upgradesMaxObject.SetActive(true);
                upgradesBuyButton.gameObject.SetActive(false);
            }
        }

        void RedrawPower()
        {
            var currentUpgrade = character.GetCurrentUpgrade();
            powerText.text = currentUpgrade.Stats.Power.ToString();
        }

        protected override void RedrawUpgradeButton()
        {
            if (character.IsMaxUpgrade()) return;
            var upgradeState = character.Upgrades[character.GetCurrentUpgradeIndex() + 1];
            var currency = CurrenciesController.GetCurrency(upgradeState.CurrencyType);

            var price = upgradeState.Price;
            if (CurrenciesController.HasAmount(upgradeState.CurrencyType, price))
            {
                upgradesBuyButtonImage.sprite = upgradesBuyButtonActiveSprite;

                if (gamepadButton  )
                    gamepadButton.SetFocus(selectedCharacterPanelUI == this);
            }
            else
            {
                upgradesBuyButtonImage.sprite = upgradesBuyButtonDisableSprite;

                if (gamepadButton != null)
                    gamepadButton.SetFocus(false);
            }

            upgradesBuyCurrencyImage.sprite = currency.Icon;
            upgradesBuyButtonText.text = CurrenciesHelper.Format(price);

            if (upgradeState.ChangeStage)
            {
                upgradesText.text = EVOLVE_TEXT;
            }
            else
            {
                upgradesText.text = UPGRADE_TEXT;
            }
        }

        public bool IsNewCharacterOpened() => storedIsLocked && character.IsUnlocked();

        public bool IsNextUpgradeCanBePurchased()
        {
            if (!character.IsUnlocked()) return false;
            if (character.IsMaxUpgrade()) return false;
            var upgradeState = character.Upgrades[character.GetCurrentUpgradeIndex() + 1];
            return CurrenciesController.HasAmount(upgradeState.CurrencyType, upgradeState.Price);
        }

        void RedrawUpgradeElements()
        {
            if (!character.IsMaxUpgrade())
            {
                var upgradeStateIndex = character.GetCurrentUpgradeIndex();
                for (var i = 0; i < upgradeStateIndex; i++)
                {
                    upgradesStatesImages[i].color = upgradeStateActiveColor;
                }

                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);

                RedrawUpgradeButton();
            }
            else
            {
                foreach (var img in upgradesStatesImages)
                    img.color = upgradeStateActiveColor;
                upgradesMaxObject.SetActive(true);
                upgradesBuyButton.gameObject.SetActive(false);
                if (gamepadButton  )
                    gamepadButton.SetFocus(false);
            }
        }

        public void OnUpgradeButtonClicked()
        {
            if (UICharactersPanel.IsControlBlocked) return;
            if (character.IsMaxUpgrade()) return;

            OnSelectButtonClicked();
            var upgradeStateIndex = character.GetCurrentUpgradeIndex() + 1;
            var price = character.Upgrades[upgradeStateIndex].Price;
            var currencyType = character.Upgrades[upgradeStateIndex].CurrencyType;
            if (CurrenciesController.HasAmount(currencyType, price))
            {
                isUpgradeAnimationPlaying = true;
                CurrenciesController.Substract(currencyType, price);
                character.UpgradeCharacter();
                if (CharactersController.SelectedCharacter.Type == character.Type)
                {
                    var characterBehaviour = CharacterBehaviour.GetBehaviour();
                    var currentUpgrade = character.GetCurrentUpgrade();
                    if (currentUpgrade.ChangeStage)
                    {
                        PlayUpgradeAnimation(currentUpgrade.StageIndex);
                        characterBehaviour.SetGraphics(character.Stages[currentUpgrade.StageIndex].Prefab, true,
                            true);
                    }
                    else
                    {
                        var characterGraphics = characterBehaviour.Graphics;
                        characterGraphics.PlayUpgradeParticle();
                        characterGraphics.PlayBounceAnimation();
                    }

                    // Update character stats
                    characterBehaviour.SetStats(currentUpgrade.Stats);
                }
                PlayUpgradeAnimation();
                RedrawUpgradeButton();
                RedrawPower();
                UIGeneralPowerIndicator.UpdateText(true);
            }

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

        public void OnSelectButtonClicked()
        {
            if (UICharactersPanel.IsControlBlocked) return;
            if (character.IsSelected()) return;

            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (!character.IsUnlocked()) return;
            CharactersController.SelectCharacter(character.Type);
            Select();
        }
    }
}