using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.LevelSystem;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        [SerializeField] ExperienceUIController experienceUIController;
        public ExperienceUIController ExperienceUIController => experienceUIController;

        [SerializeField] LevelProgressionPanel levelProgressionPanel;
        public LevelProgressionPanel LevelProgressionPanel => levelProgressionPanel;

        [Space]
        [SerializeField] GameObject areaAndPowerPanel;
        [SerializeField] TextMeshProUGUI areaText;
        [SerializeField] TextMeshProUGUI recomendedPowerText;

        [Space]
        [SerializeField] Button settingsButton;

        [Space]
        [SerializeField] GameObject tapToPlayObject;
        [SerializeField] Button tapToPlayButton;
        [SerializeField] RectTransform bottomPanelRectTransform;

        [Space]
        [SerializeField] CharacterTab characterTab;
        [SerializeField] WeaponTab weaponTab;

        [Space]
        [SerializeField] OverlayUI overlayUI;

        [Space]
        [SerializeField] GameObject dotsBackPrefab;
        public static Canvas DotsBackground { get; private set; }

        [Header("No Ads Button")]
        [SerializeField] bool useNoAdsButton = true;
        [SerializeField] Button noAdsButton;

        public CharacterTab CharacterTab => characterTab;
        public WeaponTab WeaponTab => weaponTab;

        CharacterUpgradeTutorial characterUpgradeTutorial;
        WeaponUpgradeTutorial weaponUpgradeTutorial;

        RectTransform noAdsRectTransform;

        UIGamepadButton noAdsGamepadButton;
        public UIGamepadButton NoAdsGamepadButton => noAdsGamepadButton;

        UIGamepadButton settingsGamepadButton;
        public UIGamepadButton SettingsGamepadButton => settingsGamepadButton;

        UIGamepadButton playGamepadButton;
        public UIGamepadButton PlayGamepadButton => playGamepadButton;

        public static bool DontFadeRevealNextTime { get; set; }

        #region UI Page

        public override void Initialise()
        {
            if(useNoAdsButton)
            {
                noAdsRectTransform = (RectTransform)noAdsButton.transform;
                noAdsButton.onClick.AddListener(() => OnNoAdsButtonClicked());
                noAdsButton.gameObject.SetActive(true);

                noAdsGamepadButton = noAdsButton.GetComponent<UIGamepadButton>();
            }
            else
            {
                noAdsButton.gameObject.SetActive(false);
            }

            settingsGamepadButton = settingsButton.GetComponent<UIGamepadButton>();
            playGamepadButton = tapToPlayButton.GetComponent<UIGamepadButton>();

            levelProgressionPanel.Initialise();

            characterTab.Initialise();
            weaponTab.Initialise();

            overlayUI.Initialise();

            // Create tutorial components
            if(TutorialController.ActivateCharacterTutorial)
            {
                characterUpgradeTutorial = new CharacterUpgradeTutorial();
                TutorialController.ActivateTutorial(characterUpgradeTutorial);
            }

            if(TutorialController.ActivateWeaponTutorial)
            {
                weaponUpgradeTutorial = new WeaponUpgradeTutorial();
                TutorialController.ActivateTutorial(weaponUpgradeTutorial);
            }

            DotsBackground = Instantiate(dotsBackPrefab).GetComponent<Canvas>();
            DotsBackground.worldCamera = Camera.main;
            DotsBackground.planeDistance = CameraController.GetCamera(CameraType.Menu).VirtualCamera.m_Lens.FarClipPlane - 0.1f;

            if (UIController.IsTablet)
            {
                var scrollSize = bottomPanelRectTransform.sizeDelta;
                scrollSize.y += 60;
                bottomPanelRectTransform.sizeDelta = scrollSize;
            }
        }

        public void UpdateLevelText()
        {
            areaText.text = LevelController.GetCurrentAreaText();
            recomendedPowerText.text = BalanceController.PowerRequirement.ToString();
        }

        public override void PlayShowAnimation()
        {
            IAPManager.OnPurchaseComplete += OnPurchaseComplete;

            if (characterUpgradeTutorial != null && !characterUpgradeTutorial.IsFinished)
            {
                characterUpgradeTutorial.StartTutorial();
            }
            else
            {
                if (weaponUpgradeTutorial != null && !weaponUpgradeTutorial.IsFinished)
                {
                    weaponUpgradeTutorial.StartTutorial();
                }
            }

            OverlayUI.ShowOverlay();

            characterTab.OnWindowOpened();
            weaponTab.OnWindowOpened();

            levelProgressionPanel.Show();

            bottomPanelRectTransform.anchoredPosition = new Vector2(0, -500);
            bottomPanelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetEasing(Ease.Type.CubicOut).OnComplete(() => { 
                UIController.OnPageOpened(this);
            });

            tapToPlayObject.SetActive(true);

            if (!DontFadeRevealNextTime)
            {
                Overlay.Hide(0.3f, null);
            }
            else
            {
                DontFadeRevealNextTime = false;
            }

            DotsBackground.gameObject.SetActive(true);

            if (useNoAdsButton)
            {
                if (AdsManager.IsForcedAdEnabled())
                {
                    noAdsRectTransform.gameObject.SetActive(true);
                    noAdsRectTransform.anchoredPosition = new Vector2(noAdsRectTransform.sizeDelta.x, noAdsRectTransform.anchoredPosition.y);
                    noAdsRectTransform.DOAnchoredPosition(new Vector2(-35, noAdsRectTransform.anchoredPosition.y), 0.5f).SetEasing(Ease.Type.CubicOut);
                }
                else
                {
                    noAdsRectTransform.gameObject.SetActive(false);
                }
            }

            UIGamepadButton.EnableTag(UIGamepadButtonTag.MainMenu);
        }

        public override void PlayHideAnimation()
        {
            IAPManager.OnPurchaseComplete -= OnPurchaseComplete;

            UIController.OnPageClosed(this);
            tapToPlayObject.SetActive(false);
            SettingsPanel.HidePanel(true);

            if (useNoAdsButton)
            {
                if (AdsManager.IsForcedAdEnabled())
                {
                    noAdsRectTransform.gameObject.SetActive(true);
                    noAdsRectTransform.DOAnchoredPosition(new Vector2(noAdsRectTransform.sizeDelta.x, noAdsRectTransform.anchoredPosition.y), 0.3f).SetEasing(Ease.Type.CubicIn);
                }
                else
                {
                    noAdsRectTransform.gameObject.SetActive(false);
                }
            }
        }

        #endregion

        void OnPurchaseComplete(ProductKeyType productKeyType)
        {
            if (productKeyType == ProductKeyType.NoAds)
            {
                noAdsRectTransform.gameObject.SetActive(false);
            }
        }

        #region Buttons
        public void OnNoAdsButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            IAPManager.BuyProduct(ProductKeyType.NoAds);
        }

        public void PlayButton()
        {
            levelProgressionPanel.Hide();

            bottomPanelRectTransform.DOAnchoredPosition(new Vector2(0, -500), 0.3f).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
            {
                characterTab.OnWindowClosed();
                weaponTab.OnWindowClosed();
            });

            Overlay.Show(0.3f, () =>
            {
                LevelController.OnGameStarted();
                AudioController.PlaySound(AudioController.Sounds.buttonSound);

                Overlay.Hide(0.3f, null);
            });

            SettingsPanel.HidePanel(true);
        }
        #endregion
    }
}