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
        public GameObject shopStateMode;
    
        
        [Space]
        [SerializeField] GameObject upgradesMaxObject;

        [Header("Locked State")]
        [SerializeField] GameObject lockedStateObject;
        [SerializeField] TextMeshProUGUI lockedStateText;
        [SerializeField] Color lockedPreviewColor = Color.white;

        public override bool IsUnlocked => Character.IsUnlocked();

        Character _character;
        public Character Character => _character;

        bool _storedIsLocked;

        public Transform UpgradeButtonTransform => upgradesBuyButton.transform;

        UICharactersPanel _charactersPanel;

        UIGamepadButton _gamepadButton;
        public UIGamepadButton GamepadButton => _gamepadButton;

        static CharacterPanelUI _selectedCharacterPanelUI;

        public bool IsSelected() => _selectedCharacterPanelUI == this;

        public void Initialise(Character с, UICharactersPanel panel)
        {
            _character = с;
            _charactersPanel = panel;

            panelRectTransform = (RectTransform) transform;
            _gamepadButton = upgradesBuyButton.GetComponent<UIGamepadButton>();

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
                _storedIsLocked = false;

                if (CharactersController.SelectedCharacter.Type == с.Type)
                    Select();

                lockedStateObject.SetActive(false);
                shopStateMode.SetActive(false);
                upgradesStateObject.SetActive(true);
                powerObject.SetActive(true);

                RedrawUpgradeElements();
                RedrawPower();
            }
            else
            {
                titleText.text = LOCKED_NAME;
                _storedIsLocked = true;
                powerObject.SetActive(false);
                previewImage.sprite = с.LockedSprite;
                previewImage.color = lockedPreviewColor;
                SetRequiredLevel(с.RequiredLevel);

                if (_character.onlyShop)
                {
                    shopStateMode.SetActive(true);
                    lockedStateObject.SetActive(false);
                }
            }

            mainButton.onClick.AddListener(OnSelectButtonClicked);
        }

        void PlayOpenAnimation()
        {
            lockedStateObject.SetActive(false);
            powerObject.SetActive(true);
            upgradesStateObject.SetActive(true);
            titleText.text = _character.Name.ToUpper();
            previewImage.sprite = _character.Stages[0].PreviewSprite;
            previewImage.DOColor(Color.white, 0.6f);
            RedrawUpgradeElements();
            RedrawPower();
        }

        void PlayUpgradeAnimation(int stage)
        {
            var tempStage = _character.Stages[stage];
            previewImage.sprite = tempStage.PreviewSprite;
            previewImage.rectTransform.localScale = Vector2.one * 1.3f;
            previewImage.rectTransform.DOScale(1.0f, 0.2f, 0.03f).SetEasing(Ease.Type.SineIn);
        }

        public override void OnPanelOpened()
        {
            var dynamicAnimations = new List<CharacterDynamicAnimation>();
            var isSelected = _character.IsSelected();

            // Character was locked on start
            if (_storedIsLocked)
            {
                // Now character is unlocked
                if (_character.IsUnlocked())
                {
                    _storedIsLocked = false;
                    var unlockAnimation =
                        new CharacterDynamicAnimation(this, 0.5f, onAnimationStarted: PlayOpenAnimation);
                    dynamicAnimations.Add(unlockAnimation);
                }
            }

            if (!dynamicAnimations.IsNullOrEmpty())
            {
                _charactersPanel.AddAnimations(dynamicAnimations, isSelected);
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
            if (_selectedCharacterPanelUI) _selectedCharacterPanelUI.UnselectCharacter();
            selectionImage.gameObject.SetActive(true);
            _selectedCharacterPanelUI = this;
            UIGeneralPowerIndicator.UpdateText();
            RedrawUpgradeButton();
        }

        public void UnselectCharacter()
        {
            selectionImage.gameObject.SetActive(false);
            if (_gamepadButton)
                _gamepadButton.SetFocus(false);
        }

        void PlayUpgradeAnimation()
        {
            var upgradeStateIndex = _character.GetCurrentUpgradeIndex() - 1;
            upgradesStatesImages[upgradeStateIndex].DOColor(upgradeStateActiveColor, 0.3f).OnComplete(delegate
            {
                isUpgradeAnimationPlaying = false;

                RedrawUpgradeButton();
            });

            if (!_character.IsMaxUpgrade())
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
            var currentUpgrade = _character.GetCurrentUpgrade();
            powerText.text = currentUpgrade.Stats.Power.ToString();
        }

        protected override void RedrawUpgradeButton()
        {
            if (_character.IsMaxUpgrade()) return;
            var upgradeState = _character.Upgrades[_character.GetCurrentUpgradeIndex() + 1];
            var currency = CurrenciesController.GetCurrency(upgradeState.CurrencyType);

            var price = upgradeState.Price;
            if (CurrenciesController.Has(upgradeState.CurrencyType, price))
            {
                upgradesBuyButtonImage.sprite = upgradesBuyButtonActiveSprite;

                if (_gamepadButton  )
                    _gamepadButton.SetFocus(_selectedCharacterPanelUI == this);
            }
            else
            {
                upgradesBuyButtonImage.sprite = upgradesBuyButtonDisableSprite;

                if (_gamepadButton != null)
                    _gamepadButton.SetFocus(false);
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

        public bool IsNewCharacterOpened() => _storedIsLocked && _character.IsUnlocked();

        public bool IsNextUpgradeCanBePurchased()
        {
            if (!_character.IsUnlocked()) return false;
            if (_character.IsMaxUpgrade()) return false;
            var upgradeState = _character.Upgrades[_character.GetCurrentUpgradeIndex() + 1];
            return CurrenciesController.Has(upgradeState.CurrencyType, upgradeState.Price);
        }

        void RedrawUpgradeElements()
        {
            if (!_character.IsMaxUpgrade())
            {
                var upgradeStateIndex = _character.GetCurrentUpgradeIndex();
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
                if (_gamepadButton  )
                    _gamepadButton.SetFocus(false);
            }
        }

        public void OnUpgradeButtonClicked()
        {
            if (UICharactersPanel.IsControlBlocked) return;
            if (_character.IsMaxUpgrade()) return;

            OnSelectButtonClicked();
            var upgradeStateIndex = _character.GetCurrentUpgradeIndex() + 1;
            var price = _character.Upgrades[upgradeStateIndex].Price;
            var currencyType = _character.Upgrades[upgradeStateIndex].CurrencyType;
            if (CurrenciesController.Has(currencyType, price))
            {
                isUpgradeAnimationPlaying = true;
                CurrenciesController.Substract(currencyType, price);
                _character.UpgradeCharacter();
                if (CharactersController.SelectedCharacter.Type == _character.Type)
                {
                    var characterBehaviour = CharacterBehaviour.GetBehaviour();
                    var currentUpgrade = _character.GetCurrentUpgrade();
                    if (currentUpgrade.ChangeStage)
                    {
                        PlayUpgradeAnimation(currentUpgrade.StageIndex);
                        characterBehaviour.SetGraphics(_character.Stages[currentUpgrade.StageIndex].Prefab, true,
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

            AudioController.Play(AudioController.Sounds.buttonSound);
        }

        public void OnSelectButtonClicked()
        {
            if (UICharactersPanel.IsControlBlocked) return;
            if (_character.IsSelected()) return;

            AudioController.Play(AudioController.Sounds.buttonSound);

            if (!_character.IsUnlocked()) return;
            CharactersController.Select(_character.Type);
            Select();
        }
    }
}